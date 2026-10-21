namespace JAT.API.Domain.Entities
{
    public class Tag
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;

        public Tag()
        {
            Id = Guid.CreateVersion7();
        }
    }
}
