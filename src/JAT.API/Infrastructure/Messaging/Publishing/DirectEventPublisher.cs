using JAT.API.Common.Abstractions;
using JAT.API.Infrastructure.Messaging.Abstractions;
using System.Text.Json;

namespace JAT.API.Infrastructure.Messaging.Publishing
{
    public class DirectEventPublisher(
        IMessageTransport messageTransport) : IEventPublisher
    {
        private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

        public async Task PublishAsync<T>(T eventRecord, CancellationToken ct = default) where T : class
        {
            await messageTransport.SendAsync(new OutboundMessage(
                Guid.CreateVersion7(), 
                typeof(T).Name,
                JsonSerializer.Serialize(eventRecord, Options)),
                ct);
        }
    }
}
