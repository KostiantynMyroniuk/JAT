using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Humanizer.Configuration;
using JAT.API.Common.Abstractions;
using JAT.API.Infrastructure.Messaging.Abstractions;
using JAT.API.Infrastructure.Messaging.Publishing;
using JAT.API.Infrastructure.Messaging.Routing;
using JAT.API.Infrastructure.Messaging.ServiceBus;

namespace JAT.API.Infrastructure.Messaging
{
    public static class MessagingExtensions
    {
        public static IServiceCollection AddServiceBus(
            this IServiceCollection services,
            Action<IServiceBusOptions>? configure = null)
        {
            services.AddSingleton<ServiceBusClient>(sp =>
            {
                var env = sp.GetRequiredService<IHostEnvironment>();
                var configuration = sp.GetRequiredService<IConfiguration>();

                if (env.IsDevelopment())
                {
                    var connectionString = configuration.GetConnectionString("ServiceBus")
                        ?? throw new InvalidOperationException("Connection string 'ServiceBus' is not configured");

                    return new ServiceBusClient(connectionString);
                }

                return new ServiceBusClient(
                    "jobapps.servicebus.windows.net",
                    new DefaultAzureCredential());
            });

            services.AddOptions<EventDestinationOptions>()
                .BindConfiguration(EventDestinationOptions.Section);

            services.AddSingleton<IEventDestinationResolver, EventDestinationResolver>();
            services.AddSingleton<IMessageTransport, ServiceBusMessageTransport>();

            var configurator = new ServiceBusOptions(services);
            configure?.Invoke(configurator);

            if (configurator.OutboxRegistration is { } registerOutbox)
                registerOutbox(services);
            else
                services.AddSingleton<IEventPublisher, DirectEventPublisher>();

            return services;
        }
    }
}
