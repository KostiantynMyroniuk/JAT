using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Extensions;
using JAT.API.Features.Tags.Dtos;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.Tags.Queries.GetTagsPaginated;

public sealed record GetTagsPaginatedQuery(
    int PageNumber = 1,
    int PageSize = 10);

public sealed class GetTagsPaginatedValidator : AbstractValidator<GetTagsPaginatedQuery>
{
    public GetTagsPaginatedValidator()
    {
        RuleFor(r => r.PageNumber)
            .InclusiveBetween(1, 100_000);

        RuleFor(r => r.PageSize)
            .InclusiveBetween(1, 100);
    }
}

public sealed class GetTagsPaginatedHandler(
    ApplicationDbContext context,
    IValidator<GetTagsPaginatedQuery> validator)
    : IHandler<GetTagsPaginatedQuery, Result<PaginatedList<TagDto>>>
{
    public async Task<Result<PaginatedList<TagDto>>> HandleAsync(
        GetTagsPaginatedQuery query,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return Result.Failure<PaginatedList<TagDto>>(validationResult.ToValidationError());
        }

        var dbQuery = context.Tags.AsNoTracking();

        var totalCount = await dbQuery.CountAsync(ct);

        var items = await dbQuery
            .OrderBy(t => t.Name)
            .ThenBy(t => t.Id)
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
