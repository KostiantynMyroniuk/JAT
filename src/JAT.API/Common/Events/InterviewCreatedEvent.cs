namespace JAT.API.Common.Events
{
    public record InterviewCreatedEvent(string JobName, DateTimeOffset ScheduledAt);
}
