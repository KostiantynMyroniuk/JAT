using Microsoft.Extensions.Options;

namespace JAT.API.Infrastructure.Messaging.Routing
{
    public class EventDestinationResolver(
        IOptions<EventDestinationOptions> options) : IEventDestinationResolver
    {
        public string Resolve<T>() where T : class
        {
            if (options.Value.Destinations.TryGetValue(typeof(T).Name, out var destination))
                return destination;

            throw new InvalidOperationException($"Queue/topic for event '{typeof(T).Name}' is not configured");
        }
    }
}
