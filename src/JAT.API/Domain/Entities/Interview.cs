using JAT.API.Domain.Enums;

namespace JAT.API.Domain.Entities
{
    public class Interview
    {
        public Guid Id { get; private set; }
        public Guid JobApplicationId { get; set; }
        public JobApplication JobApplication { get; set; } = default!;

        public InterviewType InterviewType { get; set; }
        public DateTimeOffset ScheduledAt { get; set; }
        public string? Location { get; set; }
        public string? Notes { get; set; }

        public Interview()
        {
            Id = Guid.CreateVersion7();
        }
    }
}
