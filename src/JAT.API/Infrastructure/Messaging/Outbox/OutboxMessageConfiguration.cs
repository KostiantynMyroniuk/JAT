using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JAT.API.Infrastructure.Messaging.Outbox
{
    public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("OutboxMessages");
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Type).HasMaxLength(200).IsRequired();
            builder.Property(m => m.Payload).IsRequired();
            builder.Property(m => m.Error).HasMaxLength(200);
            builder.HasIndex(m => new { m.OccuredAt, m.ProcessedAt });
        }
    }
}
