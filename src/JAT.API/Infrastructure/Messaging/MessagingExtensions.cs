using Azure.Identity;
using Azure.Messaging.ServiceBus;
using JAT.API.Common.Abstractions;
using JAT.API.Infrastructure.Messaging.Routing;
using JAT.API.Infrastructure.Messaging.ServiceBus;

namespace JAT.API.Infrastructure.Messaging
{
    public static class MessagingExtensions
    {
        public static IServiceCollection AddServiceBusConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("ServiceBus")
                ?? throw new InvalidOperationException("Connection string 'ServiceBus' is not configured");

            services.AddSingleton<ServiceBusClient>(sp =>
            {
                var env = sp.GetRequiredService<IHostEnvironment>();

                if (env.IsDevelopment())
                    return new ServiceBusClient(connectionString);

                return new ServiceBusClient(
                    "jobapps.servicebus.windows.net",
                    new DefaultAzureCredential());
            });
                

            services.Configure<EventDestinationOptions>(
                configuration.GetSection("EventDestinations"));

            services.AddSingleton<IEventDestinationResolver, EventDestinationResolver>();

            services.AddSingleton<IEventPublisher, EventPublisher>();

            return services;
        }
    }
}
