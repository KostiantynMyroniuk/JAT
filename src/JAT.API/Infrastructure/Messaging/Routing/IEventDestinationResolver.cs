namespace JAT.API.Infrastructure.Messaging.Routing
{
    public interface IEventDestinationResolver
    {
        string Resolve(string eventType);
    }
}
