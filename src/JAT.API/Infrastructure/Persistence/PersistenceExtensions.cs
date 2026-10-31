using Microsoft.EntityFrameworkCore;

namespace JAT.API.Infrastructure.Persistence
{
    public static class PersistenceExtensions
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("JatDb")
                ?? throw new InvalidOperationException("Connection string 'JatDb' is not configured.");

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure());
            });

            return services;
        }
    }
}
