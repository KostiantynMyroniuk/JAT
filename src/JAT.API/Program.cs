using System.Text.Json.Serialization;
using FluentValidation;
using JAT.API.ExceptionHandling;
using JAT.API.Extensions;
using JAT.API.Infrastructure.Messaging;
using JAT.API.Infrastructure.Persistence;
using JAT.API.Mcp;
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
    .AddVersioning()
    .AddEndpoints(typeof(Program).Assembly)
    .AddHandlers(typeof(Program).Assembly)
    .AddValidatorsFromAssemblyContaining<Program>()
    .AddMcpServices()
    .AddServiceBusConfigurations(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Configuration.GetValue<bool>("Database:MigrateOnStart"))
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
}

app.MapCustomHealthChecks();

app.MapVersionedApi();

app.UseOpenApiDocumentation();

app.UseMcpServer();

app.Run();
