using JAT.API.Common.Abstractions;
using JAT.API.Infrastructure.Messaging.Outbox;
using JAT.API.Infrastructure.Messaging.Publishing;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Infrastructure.Messaging.ServiceBus
{
    public class ServiceBusOptions(IServiceCollection services) : IServiceBusOptions
    {
        public IServiceCollection Services => services;
        public Action<IServiceCollection>? OutboxRegistration { get; private set; }

        public IServiceBusOptions AddOutbox<TContext>(Action<OutboxOptions>? configure = null) where TContext : DbContext
        {
            OutboxRegistration = s =>
            {
                s.AddOptions<OutboxOptions>().Configure(o => configure?.Invoke(o));
                s.AddScoped<IEventPublisher, OutboxEventPublisher>();
                s.AddHostedService<OutboxProcessor<TContext>>();
            };

            return this;
        }
    }
}
