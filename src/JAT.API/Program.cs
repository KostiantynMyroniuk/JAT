using JAT.API.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services
    .AddPersistence(builder.Configuration)
    .AddHealthChecks(builder.Configuration);

var app = builder.Build();

app.MapHealthChecks();

app.Run();
