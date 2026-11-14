using Azure.Identity;
using Azure.Messaging.ServiceBus;
using JAT.Shared.Events;
using Notifications.Worker.Common.Abstractions;
using Notifications.Worker.Features.Interviews;
using Notifications.Worker.Infrastructure.Messaging.ServiceBus;
using Notifications.Worker.Infrastructure.Messaging.ServiceBus.Processors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Notifications.Worker.Infrastructure.Messaging
{
    public static class MessagingExtensions
    {
        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

        public static IServiceCollection AddEventHandler<TEvent, THandler>(
            this IServiceCollection services,
            string subject) where THandler : class, IEventHandler<TEvent>
        {
            services.AddScoped<IEventHandler<TEvent>, THandler>();

            services.AddSingleton(new EventRoute(subject, (sp, body, ct) =>
                sp.GetRequiredService<IEventHandler<TEvent>>()
                    .HandleAsync(body.ToObjectFromJson<TEvent>(Json)!, ct)));

            return services;
        }

        public static IServiceCollection AddServiceBusConfigurations(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("ServiceBus")
                ?? throw new InvalidOperationException("Connection string 'ServiceBus' is not configured");

            var busNamespace = configuration["Azure:BusNamespace"] ?? "example"; 

            services.AddSingleton<ServiceBusClient>(sp =>
            {
                var env = sp.GetRequiredService<IHostEnvironment>();

                if (env.IsDevelopment())
                    return new ServiceBusClient(connectionString);

                return new ServiceBusClient(
                    busNamespace,
                    new DefaultAzureCredential());
            });

            services.AddHostedService<NotificationsProcessor>();

            services.AddSingleton<HandlerDispatcher>();

            services.AddEventHandler<InterviewCreatedEvent, InterviewCreatedEventHandler>(typeof(InterviewCreatedEvent).Name);

            return services;
        }
    }
}
