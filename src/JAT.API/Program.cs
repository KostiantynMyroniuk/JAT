using FluentValidation;
using JAT.API.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services
    .AddOpenApi()
    .AddPersistence(builder.Configuration)
    .AddHealthChecks(builder.Configuration)
    .AddEndpoints(typeof(Program).Assembly)
    .AddValidatorsFromAssembly(typeof(Program).Assembly);

var app = builder.Build();

app.MapOpenApi();
app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "v1"));

app.MapHealthChecks();
app.MapEndpoints();

app.Run();
