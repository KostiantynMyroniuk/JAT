using JAT.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace JAT.API.Extensions
{
    public static class HealthCheckExtensions
    {
        public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>(
                    name: "database",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: ["ready"]);

            return services;
        }

        public static IEndpointRouteBuilder MapCustomHealthChecks(this IEndpointRouteBuilder app)
        {
            app.MapHealthChecks("health/live", new HealthCheckOptions
            {
                Predicate = _ => false
            });

            app.MapHealthChecks("health/ready", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("ready")
            });

            return app;
        }
    }
}
