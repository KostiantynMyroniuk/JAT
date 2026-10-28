using Asp.Versioning;
using JAT.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Swashbuckle.AspNetCore.Swagger;

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

        public static IServiceCollection AddOpenApiOptions(this IServiceCollection services)
        {
            services.AddOpenApi(options =>
            {
                options.CreateSchemaReferenceId = typeInfo =>
                {
                    var type = typeInfo.Type;

                    if (type.DeclaringType is not null && !type.IsGenericType)
                        return $"{type.DeclaringType.Name}.{type.Name}";

                    return OpenApiOptions.CreateDefaultSchemaReferenceId(typeInfo);
                };
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
