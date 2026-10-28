using JAT.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JAT.API.Infrastructure.Persistence.Configurations
{
    public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
    {
        public void Configure(EntityTypeBuilder<JobApplication> builder)
        {
            builder
                .HasMany(a => a.Interviews)
                .WithOne(i => i.JobApplication)
                .HasForeignKey(i => i.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(a => a.Tags)
                .WithMany();

            builder.Property(a => a.Title)
                .HasMaxLength(100);

            builder.Property(a => a.Notes)
                .HasMaxLength(500);

            builder.Property(a => a.SalaryMin)
                .HasPrecision(18, 2);

            builder.Property(a => a.SalaryMax)
                .HasPrecision(18, 2);
        }
    }
}
