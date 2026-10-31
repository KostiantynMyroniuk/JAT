using System.Text.Json.Serialization;
using FluentValidation;
using JAT.API.Extensions;
using JAT.API.Infrastructure.ExceptionHandling;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddProblemDetails()
    .AddPersistence(builder.Configuration)
    .AddCustomHealthChecks()
    .AddOpenApiDocumentation()
    .AddVersioning()
    .AddEndpoints(typeof(Program).Assembly)
    .AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Configuration.GetValue<bool>("Database:MigrateOnStart"))
{
    var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
}

app.MapCustomHealthChecks();

app.MapVersionedApi();

app.UseOpenApiDocumentation();

app.Run();
