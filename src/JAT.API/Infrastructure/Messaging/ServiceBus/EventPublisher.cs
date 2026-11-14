using Azure.Messaging.ServiceBus;
using JAT.API.Common.Abstractions;
using JAT.API.Infrastructure.Messaging.Routing;
using System.Collections.Concurrent;
using System.Text.Json;

namespace JAT.API.Infrastructure.Messaging.ServiceBus
{
    public class EventPublisher(
        ServiceBusClient serviceBusClient,
        IEventDestinationResolver eventDestinationResolver) : IEventPublisher, IAsyncDisposable
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        private readonly ConcurrentDictionary<string, ServiceBusSender> _senders = new();

        public async Task PublishAsync<T>(T eventRecord, CancellationToken ct = default) where T : class
        {
            var destination = eventDestinationResolver.Resolve<T>();

            var sender = _senders.GetOrAdd(destination, serviceBusClient.CreateSender);

            var message = new ServiceBusMessage(JsonSerializer.Serialize(eventRecord, jsonSerializerOptions))
            {
                MessageId = Guid.NewGuid().ToString(),
                ContentType = "application/json",
                Subject = typeof(T).Name
            };

            await sender.SendMessageAsync(message, ct);
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var sender in _senders.Values)
            {
                await sender.DisposeAsync();
            }

            _senders.Clear();
        }
    }
}
