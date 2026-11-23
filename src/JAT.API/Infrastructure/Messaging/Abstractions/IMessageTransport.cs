namespace JAT.API.Infrastructure.Messaging.Abstractions
{
    public sealed record OutboundMessage(Guid MessageId, string Type, string Payload);
    public interface IMessageTransport
    {
        Task SendAsync(OutboundMessage message, CancellationToken ct = default);
    }
}
