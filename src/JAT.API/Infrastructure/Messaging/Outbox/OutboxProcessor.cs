using JAT.API.Common.Abstractions;
using JAT.API.Infrastructure.Messaging.Abstractions;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace JAT.API.Infrastructure.Messaging.Outbox
{
    public class OutboxProcessor<TContext>(
        IServiceScopeFactory scopeFactory,
        IMessageTransport messageTransport,
        IOptions<OutboxOptions> options,
        ILogger<OutboxProcessor<TContext>> logger) : BackgroundService where TContext : DbContext
    {
        private readonly OutboxOptions _opt = options.Value;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(_opt.PollingInterval);

            try
            {
                do
                {
                    try
                    {
                        while (await ProcessBatchAsync(stoppingToken) == _opt.BatchSize) { }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        logger.LogError(ex, "Outbox batch failed");
                    }
                }
                while (await timer.WaitForNextTickAsync(stoppingToken));
            }
            catch (OperationCanceledException) { }
        }

        private async Task<int> ProcessBatchAsync(CancellationToken ct)
        {
            using var scope = scopeFactory.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<TContext>();

            var outboxMessages = await context.Set<OutboxMessage>()
                .Where(m => m.ProcessedAt == null)
                .OrderBy(m => m.OccuredAt)
                .Take(_opt.BatchSize)
                .ToListAsync(ct);

            foreach (var message in outboxMessages)
            {
                try
                {
                    await messageTransport.SendAsync(
                        new OutboundMessage(
                            message.Id,
                            message.Type,
                            message.Payload));

                    message.ProcessedAt = DateTime.UtcNow;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    message.Attempts++;
                    message.Error = ex.Message;
                    logger.LogError(ex, "Failed to publish outbox message {Id}", message.Id);
                }
            }

            await context.SaveChangesAsync(ct);

            return outboxMessages.Count;
        }
    }
}
