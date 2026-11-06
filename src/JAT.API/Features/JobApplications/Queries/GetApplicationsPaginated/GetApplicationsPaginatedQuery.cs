using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Domain.Enums;
using JAT.API.Features.JobApplications.Dtos;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.JobApplications.Queries.GetApplicationsPaginated;

public sealed record GetApplicationsPaginatedQuery(
    JobApplicationStatus? Status,
    int PageNumber = 1,
    int PageSize = 10);

public sealed class GetApplicationsPaginatedHandler(ApplicationDbContext context)
    : IHandler<GetApplicationsPaginatedQuery, Result<PaginatedList<JobApplicationDto>>>
{
    public async Task<Result<PaginatedList<JobApplicationDto>>> HandleAsync(
        GetApplicationsPaginatedQuery query,
        CancellationToken ct = default)
    {
        var dbQuery = context.JobApplications.AsNoTracking();

        if (query.Status.HasValue)
        {
            dbQuery = dbQuery.Where(a => a.Status == query.Status.Value);
        }

        var totalCount = await dbQuery.CountAsync(ct);

        var items = await dbQuery
            .OrderByDescending(i => i.CreatedAt)
            .ThenBy(i => i.Id)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(a => new JobApplicationDto(
                a.Id,
                a.Title,
                a.Notes,
                a.Status,
                a.Url,
                a.AppliedAt,
                a.SalaryMin,
                a.SalaryMax,
                a.Tags
                    .OrderBy(t => t.Name)
                    .Select(t => new JobApplicationTagDto(t.Id, t.Name))
                    .ToList()))
            .ToListAsync(ct);

        return Result.Success(
            new PaginatedList<JobApplicationDto>(items, query.PageNumber, query.PageSize, totalCount));
    }
}
