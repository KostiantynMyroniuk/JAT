using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Extensions;
using JAT.API.Features.Interviews.Dtos;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.Interviews.Queries.GetInterviewsPaginated;

public sealed record GetInterviewsPaginatedQuery(
    Guid? JobApplicationId = null,
    int PageNumber = 1,
    int PageSize = 10);

public sealed class GetInterviewsPaginatedValidator : AbstractValidator<GetInterviewsPaginatedQuery>
{
    public GetInterviewsPaginatedValidator()
    {
        RuleFor(r => r.PageNumber)
            .InclusiveBetween(1, 100_000);

        RuleFor(r => r.PageSize)
            .InclusiveBetween(1, 100);
    }
}

public sealed class GetInterviewsPaginatedHandler(
    ApplicationDbContext context,
    IValidator<GetInterviewsPaginatedQuery> validator)
    : IHandler<GetInterviewsPaginatedQuery, Result<PaginatedList<InterviewDto>>>
{
    public async Task<Result<PaginatedList<InterviewDto>>> HandleAsync(
        GetInterviewsPaginatedQuery query,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return Result.Failure<PaginatedList<InterviewDto>>(validationResult.ToValidationError());
        }

        var dbQuery = context.Interviews.AsNoTracking();

        if (query.JobApplicationId.HasValue)
        {
            dbQuery = dbQuery.Where(i => i.JobApplicationId == query.JobApplicationId.Value);
        }

        var totalCount = await dbQuery.CountAsync(ct);

        var items = await dbQuery
            .OrderByDescending(i => i.ScheduledAt)
            .ThenBy(i => i.Id)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(i => new InterviewDto(
                i.Id,
                i.JobApplicationId,
                i.InterviewType,
                i.ScheduledAt,
                i.Location,
                i.Notes))
            .ToListAsync(ct);

        return Result.Success(
            new PaginatedList<InterviewDto>(items, query.PageNumber, query.PageSize, totalCount));
    }
}
