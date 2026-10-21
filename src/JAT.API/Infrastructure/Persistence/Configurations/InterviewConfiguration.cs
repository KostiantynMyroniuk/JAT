using JAT.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JAT.API.Infrastructure.Persistence.Configurations
{
    public class InterviewConfiguration : IEntityTypeConfiguration<Interview>
    {
        public void Configure(EntityTypeBuilder<Interview> builder)
        {
            builder
                .HasOne(i => i.JobApplication)
                .WithMany(a => a.Interviews)
                .HasForeignKey(i => i.JobApplicationId);

            builder.Property(i => i.Location)
                .HasMaxLength(200);

            builder.Property(i => i.Notes)
                .HasMaxLength(500);
        }
    }
}
