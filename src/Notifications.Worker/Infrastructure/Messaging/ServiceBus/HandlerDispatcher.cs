using System;
using System.Collections.Generic;
using System.Text;

namespace Notifications.Worker.Infrastructure.Messaging.ServiceBus
{
    public sealed record EventRoute(string Subject, Func<IServiceProvider, BinaryData, CancellationToken, Task> Invoke);

    public class HandlerDispatcher(IEnumerable<EventRoute> routes)
    {
        private readonly Dictionary<string, EventRoute> _routes = routes.ToDictionary(r => r.Subject);
        public async Task<bool> DispatchAsync(
            string? subject, BinaryData body, IServiceProvider serviceProvider, CancellationToken ct)
        {
            if (subject == null || !_routes.TryGetValue(subject, out var route))
                return false;

            await route.Invoke(serviceProvider, body, ct);
            return true;
        }
    }
}
