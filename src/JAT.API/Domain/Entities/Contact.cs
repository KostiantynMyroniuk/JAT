namespace JAT.API.Domain.Entities
{
    public class Contact
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = default!;

        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? LinkedIn { get; set; }

        public Contact()
        {
            Id = Guid.CreateVersion7();
        }
    }
}
