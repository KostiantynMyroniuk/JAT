namespace JAT.API.Infrastructure.Messaging.Routing
{
    public class EventDestinationOptions
    {
        public static readonly string Section = "EventDestinations"; 
        public required Dictionary<string, string> Destinations { get; init; }
    }
}
