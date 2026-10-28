using JAT.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Infrastructure.Persistence
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<Interview> Interviews { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Contact> Contacts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("jat");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
