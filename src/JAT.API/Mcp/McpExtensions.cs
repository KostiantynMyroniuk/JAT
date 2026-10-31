namespace JAT.API.Mcp
{
    public static class McpExtensions
    {
        public static IServiceCollection AddMcpServices(this IServiceCollection services)
        {
            services.AddMcpServer()
                .WithHttpTransport()
                .WithToolsFromAssembly();

            return services;
        }

        public static WebApplication UseMcpServer(this WebApplication app)
        {
            app.MapMcp("/mcp");

            return app;
        }
    }
}