namespace JAT.API.Infrastructure.Messaging.Outbox
{
    public sealed class OutboxOptions
    {
        public int BatchSize { get; set; } = 50;
        public int MaxAttempts { get; set; } = 10;
        public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(2);
        public TimeSpan Retention { get; set; } = TimeSpan.FromDays(7);
    }
}
