namespace JAT.API.Common.Abstractions
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(T eventRecord, CancellationToken ct = default) where T : class;
    }
}
