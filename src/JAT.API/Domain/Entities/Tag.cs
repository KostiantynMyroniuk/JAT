using JAT.API.Domain.Exceptions;

namespace JAT.API.Domain.Entities
{
    public class Tag
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = default!;

        public Tag()
        {
            Id = Guid.CreateVersion7();
        }

        public static Tag Create(string name)
        {
            var normalized = name.Trim().ToLowerInvariant();

            return new Tag { Name = normalized };
        }
    }
}
