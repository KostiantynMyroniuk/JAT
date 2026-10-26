using FluentValidation;
using JAT.API.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services
    .AddPersistence(builder.Configuration)
    .AddHealthChecks(builder.Configuration)
    .AddEndpoints(typeof(Program).Assembly)
    .AddVersioning()
    .AddProblemDetails()
    .AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

app.MapHealthChecks();

app.MapVersionedApi();

app.MapOpenApi().WithDocumentPerVersion();
app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("/openapi/v1.json", "v1");
    o.SwaggerEndpoint("/openapi/v2.json", "v2");
});

app.Run();
