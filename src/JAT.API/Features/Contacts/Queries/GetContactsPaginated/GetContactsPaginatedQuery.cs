using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Features.Contacts.Dtos;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.Contacts.Queries.GetContactsPaginated;

public sealed record GetContactsPaginatedQuery(
    Guid? JobApplicationId = null,
    int PageNumber = 1,
    int PageSize = 10);

public sealed class GetContactsPaginatedHandler(ApplicationDbContext context)
    : IHandler<GetContactsPaginatedQuery, Result<PaginatedList<ContactDto>>>
{
    public async Task<Result<PaginatedList<ContactDto>>> HandleAsync(
        GetContactsPaginatedQuery query,
        CancellationToken ct = default)
    {
        var dbQuery = context.Contacts.AsNoTracking();

        if (query.JobApplicationId.HasValue)
        {
            dbQuery = dbQuery.Where(c => c.JobApplicationId == query.JobApplicationId.Value);
        }

        var totalCount = await dbQuery.CountAsync(ct);

        var items = await dbQuery
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Id)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new ContactDto(
                c.Id,
                c.JobApplicationId,
                c.Name,
                c.Email,
                c.LinkedIn))
            .ToListAsync(ct);

        return Result.Success(
            new PaginatedList<ContactDto>(items, query.PageNumber, query.PageSize, totalCount));
    }
}
