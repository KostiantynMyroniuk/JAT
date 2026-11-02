using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Features.Tags.Dtos;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.Tags.Queries.GetTagsPaginated;

public sealed record GetTagsPaginatedQuery(
    int PageNumber = 1,
    int PageSize = 10);

public sealed class GetTagsPaginatedHandler(ApplicationDbContext context)
    : IHandler<GetTagsPaginatedQuery, Result<PaginatedList<TagDto>>>
{
    public async Task<Result<PaginatedList<TagDto>>> HandleAsync(
        GetTagsPaginatedQuery query,
        CancellationToken ct = default)
    {
        var dbQuery = context.Tags.AsNoTracking();

        var totalCount = await dbQuery.CountAsync(ct);

        var items = await dbQuery
            .OrderBy(t => t.Name)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(t => new TagDto(
                t.Id,
                t.Name))
            .ToListAsync(ct);

        return Result.Success(
            new PaginatedList<TagDto>(items, query.PageNumber, query.PageSize, totalCount));
    }
}
