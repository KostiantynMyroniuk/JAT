using FluentValidation;
using JAT.API.Extensions;
using JAT.API.Infrastructure.ExceptionHandling;
using JAT.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddProblemDetails()
    .AddPersistence(builder.Configuration)
    .AddHealthChecks(builder.Configuration)
    .AddEndpoints(typeof(Program).Assembly)
    .AddOpenApiOptions()
    .AddVersioning()
    .AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Configuration.GetValue<bool>("Database:MigrateOnStart"))
{
    var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
}

app.MapHealthChecks();

app.MapVersionedApi();

app.MapOpenApi().WithDocumentPerVersion();
app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("/openapi/v1.json", "v1");
    o.SwaggerEndpoint("/openapi/v2.json", "v2");
});

app.Run();
