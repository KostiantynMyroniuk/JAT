using JAT.API.Infrastructure.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Infrastructure.Messaging.ServiceBus
{
    public interface IServiceBusOptions
    {
        IServiceCollection Services { get; }

        IServiceBusOptions AddOutbox<TContext>(Action<OutboxOptions>? configure = null) where TContext : DbContext;
    }
}
