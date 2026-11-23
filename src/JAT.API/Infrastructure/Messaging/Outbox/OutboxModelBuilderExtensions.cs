using Microsoft.EntityFrameworkCore;

namespace JAT.API.Infrastructure.Messaging.Outbox
{
    public static class OutboxModelBuilderExtensions
    {
        public static ModelBuilder AddOutboxEntities(this ModelBuilder builder)
        {
            builder.ApplyConfiguration(new OutboxMessageConfiguration());

            return builder;
        }
    }
}
