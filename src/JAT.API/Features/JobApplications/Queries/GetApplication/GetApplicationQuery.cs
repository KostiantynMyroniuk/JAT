using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Features.JobApplications.Dtos;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.JobApplications.Queries.GetApplication;

public sealed record GetApplicationQuery(Guid ApplicationId);

public sealed class GetApplicationHandler(ApplicationDbContext context)
    : IHandler<GetApplicationQuery, Result<JobApplicationDto>>
{
    public async Task<Result<JobApplicationDto>> HandleAsync(
        GetApplicationQuery query,
        CancellationToken ct = default)
    {
        var response = await context.JobApplications
            .AsNoTracking()
            .Where(a => a.Id == query.ApplicationId)
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
            .FirstOrDefaultAsync(ct);

        if (response is null)
        {
            return Result.Failure<JobApplicationDto>(
                Error.NotFound($"Application: {query.ApplicationId} not found"));
        }

        return Result.Success(response);
    }
}
