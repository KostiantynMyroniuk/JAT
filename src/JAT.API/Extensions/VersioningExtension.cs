using Asp.Versioning;

namespace JAT.API.Extensions
{
    public static class VersioningExtension
    {
        public static IServiceCollection AddVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(o =>
            {
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.ReportApiVersions = true;
                o.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(o =>
            {
                o.GroupNameFormat = "'v'VVV";
                o.SubstituteApiVersionInUrl = true;
            })
            .AddOpenApi();

            return services;
        }

        public static WebApplication MapVersionedApi(this WebApplication app)
        {
            var versionedApi = app.NewVersionedApi("Jat");

            var api = versionedApi.MapGroup("/api/v{version:apiVersion}")
                .HasApiVersion(new ApiVersion(1, 0))
                .HasApiVersion(new ApiVersion(2, 0));

            api.MapEndpoints();

            return app;
        }
    }
}
