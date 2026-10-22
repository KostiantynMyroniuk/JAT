using JAT.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace JAT.API.Extensions
{
    public static class Extensions
    {
        //db configs
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("JatDb")
                ?? throw new InvalidOperationException(
                    "Connection string 'JatDb' is not configured.");

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure());
            });

            return services;
        }

        // healthchecks add + map
        public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>(
                    name: "database",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "ready" });

            return services;
        }

        public static IEndpointRouteBuilder MapHealthChecks(this IEndpointRouteBuilder app)
        {
            app.MapHealthChecks("health/live", new HealthCheckOptions()
            {
                Predicate = _ => false
            });

            app.MapHealthChecks("health/ready", new HealthCheckOptions()
            {
                Predicate = r => r.Tags.Contains("ready")
            });

            return app;
        }
    }
}
