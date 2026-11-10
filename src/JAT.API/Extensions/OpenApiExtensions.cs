using Microsoft.AspNetCore.OpenApi;

namespace JAT.API.Extensions
{
    public static class OpenApiExtensions
    {
        public static WebApplication UseOpenApiDocumentation(this WebApplication app)
        {
            app.MapOpenApi()
                .WithDocumentPerVersion();

            app.UseSwaggerUI(o =>
            {
                foreach (var description in app.DescribeApiVersions())
                {
                    o.SwaggerEndpoint(
                        $"/openapi/{description.GroupName}.json",
                        description.GroupName);
                }
            });

            return app;
        }
    }
}
