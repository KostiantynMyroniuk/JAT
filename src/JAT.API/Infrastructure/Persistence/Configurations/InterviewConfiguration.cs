using JAT.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JAT.API.Infrastructure.Persistence.Configurations
{
    public class InterviewConfiguration : IEntityTypeConfiguration<Interview>
    {
        public void Configure(EntityTypeBuilder<Interview> builder)
        {
            builder.Property(i => i.Location)
                .HasMaxLength(200);

            builder.Property(i => i.Notes)
                .HasMaxLength(500);

            builder.Property(i => i.InterviewType)
                .HasConversion<string>();
        }
    }
}
