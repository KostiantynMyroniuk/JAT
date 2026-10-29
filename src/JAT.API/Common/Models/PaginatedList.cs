namespace JAT.API.Common.Models
{
    public record PaginatedList<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount)
    {
        public int TotalPages => (int)Math.Ceiling(TotalCount / (decimal)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    };
}
