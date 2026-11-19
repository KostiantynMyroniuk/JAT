using Notifications.Worker.Infrastructure.Email;
using Notifications.Worker.Infrastructure.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddServiceBusConfigurations(builder.Configuration)
    .AddEmailConfigurations(builder.Configuration);

var host = builder.Build();
host.Run();
