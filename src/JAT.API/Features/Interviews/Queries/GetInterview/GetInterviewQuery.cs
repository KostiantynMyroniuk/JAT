using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Features.Interviews.Dtos;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.Interviews.Queries.GetInterview;

public sealed record GetInterviewQuery(Guid InterviewId);

public sealed class GetInterviewHandler(ApplicationDbContext context)
    : IHandler<GetInterviewQuery, Result<InterviewDto>>
{
    public async Task<Result<InterviewDto>> HandleAsync(
        GetInterviewQuery query,
        CancellationToken ct = default)
    {
        var response = await context.Interviews
            .AsNoTracking()
            .Where(i => i.Id == query.InterviewId)
            .Select(i => new InterviewDto(
                i.Id,
                i.JobApplicationId,
                i.InterviewType,
                i.ScheduledAt,
                i.Location,
                i.Notes))
            .FirstOrDefaultAsync(ct);

        if (response is null)
        {
            return Result.Failure<InterviewDto>(
                Error.NotFound($"Interview: {query.InterviewId} not found"));
        }

        return Result.Success(response);
    }
}
