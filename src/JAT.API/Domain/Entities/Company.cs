namespace JAT.API.Domain.Entities
{
    public class Company
    {
        public Guid Id { get; set; }
        public Guid ContactId { get; set; }
        public Contact Contact { get; set; } = default!;

        public string Name { get; set; } = default!;

        public ICollection<JobApplication> JobApplications { get; set; } = [];

        public Company()
        {
            Id = Guid.CreateVersion7();
        }
    }
}
