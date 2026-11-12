namespace JAT.API.Infrastructure.Messaging
{
    public class EventDestinationOptions
    {
        public required Dictionary<string, string> Destinations { get; init; }
    }
}
