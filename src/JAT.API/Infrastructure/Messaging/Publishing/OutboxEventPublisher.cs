using JAT.API.Common.Abstractions;
using JAT.API.Infrastructure.Messaging.Outbox;
using JAT.API.Infrastructure.Persistence;
using System.Text.Json;

namespace JAT.API.Infrastructure.Messaging.Publishing
{
    public class OutboxEventPublisher(
        ApplicationDbContext context) : IEventPublisher
    {
        private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        public Task PublishAsync<T>(T eventRecord, CancellationToken ct = default) where T : class
        {
            var now = DateTime.Now;
            context.Set<OutboxMessage>().Add(new OutboxMessage
            {
                Id = Guid.CreateVersion7(),
                Type = typeof(T).Name,
                Payload = JsonSerializer.Serialize(eventRecord, jsonOptions),
                OccuredAt = now,
            });

            return Task.CompletedTask;
        }
    }
}
