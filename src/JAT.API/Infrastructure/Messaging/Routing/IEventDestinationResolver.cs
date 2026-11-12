namespace JAT.API.Infrastructure.Messaging.Routing
{
    public interface IEventDestinationResolver
    {
        string Resolve<T>() where T : class;
    }
}
