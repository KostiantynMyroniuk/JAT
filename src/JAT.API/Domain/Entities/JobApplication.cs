using JAT.API.Domain.Enums;

namespace JAT.API.Domain.Entities
{
    public class JobApplication
    {
        public Guid Id { get; private set; }
        public Contact? Contact { get; set; } = default!;

        public string Title { get; set; } = default!;
        public string? Notes { get; set; }
        public JobApplicationStatus Status { get; set; }
        public string? Url { get; set; }

        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? AppliedAt { get; set; }

        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }

        private readonly List<Tag> _tags  = [];
        public IReadOnlyList<Tag> Tags => _tags;

        private readonly List<Interview> _interviews = [];
        public IReadOnlyList<Interview> Interviews => _interviews;

        public JobApplication()
        {
            Id = Guid.CreateVersion7();
            CreatedAt = DateTimeOffset.Now;
        }

        public void SetTags(List<Tag> tags)
        {
            _tags.AddRange(tags);
        }
    }
}
