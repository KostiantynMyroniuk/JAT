using Notifications.Worker.Infrastructure.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddServiceBusConfigurations(builder.Configuration);

var host = builder.Build();
host.Run();
