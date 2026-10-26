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

        public static Tag Create(string name)
        {
            var normalized = name.Trim().ToLowerInvariant();
            if (normalized.Length == 0)
                throw new ArgumentException("Tag name is required", nameof(name));

            return new Tag { Name = normalized };
        }
    }
}
