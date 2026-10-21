using JAT.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JAT.API.Infrastructure.Persistence.Configurations
{
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder
                .HasOne(c => c.Contact)
                .WithOne(c => c.Company)
                .HasForeignKey<Contact>(c => c.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(c => c.JobApplications)
                .WithOne(a => a.Company)
                .HasForeignKey(a => a.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(c => c.Name)
                .HasMaxLength(100);
        }
    }
}
