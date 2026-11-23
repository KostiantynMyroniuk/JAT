using Azure.Messaging.ServiceBus;
using JAT.API.Common.Abstractions;
using JAT.API.Infrastructure.Messaging.Abstractions;
using JAT.API.Infrastructure.Messaging.Routing;
using System.Collections.Concurrent;

namespace JAT.API.Infrastructure.Messaging.ServiceBus
{
    public class ServiceBusMessageTransport(
        ServiceBusClient serviceBusClient,
        IEventDestinationResolver eventDestinationResolver) : IMessageTransport, IAsyncDisposable
    {
        private readonly ConcurrentDictionary<string, ServiceBusSender> _senders = new();

        public async Task SendAsync(OutboundMessage message, CancellationToken ct = default)
        {
            var destination = eventDestinationResolver.Resolve(message.Type);

            var sender = _senders.GetOrAdd(destination, serviceBusClient.CreateSender);

            var busMessage = new ServiceBusMessage(message.Payload)
            {
                MessageId = message.MessageId.ToString(),
                ContentType = "application/json",
                Subject = message.Type
            };

            await sender.SendMessageAsync(busMessage, ct);
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
