using JAT.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JAT.API.Infrastructure.Persistence.Configurations
{
    public class ContactConfiguration : IEntityTypeConfiguration<Contact>
    {
        public void Configure(EntityTypeBuilder<Contact> builder)
        {
            builder.Property(c => c.Name)
                .HasMaxLength(100);

            builder.Property(c => c.Email)
                .HasMaxLength(100);

            builder.Property(c => c.LinkedIn)
                .HasMaxLength(200);
        }
    }
}
