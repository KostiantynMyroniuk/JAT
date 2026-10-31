using Microsoft.AspNetCore.OpenApi;

namespace JAT.API.Extensions
{
    public static class OpenApiExtensions
    {
        public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
        {
            services.Configure<OpenApiOptions>(options =>
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

        public static WebApplication UseOpenApiDocumentation(this WebApplication app)
        {
            app.MapOpenApi().WithDocumentPerVersion();
            app.UseSwaggerUI(o =>
            {
                o.SwaggerEndpoint("/openapi/v1.json", "v1");
                o.SwaggerEndpoint("/openapi/v2.json", "v2");
            });

            return app;
        }
    }
}
