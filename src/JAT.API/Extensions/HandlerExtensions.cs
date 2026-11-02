using JAT.API.Common.Abstractions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace JAT.API.Extensions
{
    public static class HandlerExtensions
    {
        public static IServiceCollection AddHandlers(this IServiceCollection services, Assembly assembly)
        {
            var handlerTypes = assembly.DefinedTypes
                .Where(t => t is { IsAbstract: false, IsInterface: false });

            foreach (var type in handlerTypes)
            {
                var handlerInterfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandler<,>));

                foreach (var handlerInterface in handlerInterfaces)
                {
                    services.TryAddScoped(handlerInterface, type);
                }
            }

            return services;
        }
    }
}
