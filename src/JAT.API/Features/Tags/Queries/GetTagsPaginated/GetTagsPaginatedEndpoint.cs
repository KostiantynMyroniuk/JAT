using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Extensions;
using JAT.API.Features.Tags.Dtos;
using JAT.API.Filters;

namespace JAT.API.Features.Tags.Queries.GetTagsPaginated;

public sealed record GetTagsPaginatedRequest(
    int PageNumber = 1,
    int PageSize = 10);

public sealed class GetTagsPaginatedValidator : AbstractValidator<GetTagsPaginatedRequest>
{
    public GetTagsPaginatedValidator()
    {
        RuleFor(r => r.PageNumber)
            .InclusiveBetween(1, 100_000);

        RuleFor(r => r.PageSize)
            .InclusiveBetween(1, 100);
    }
}

public sealed class GetTagsPaginatedEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/tags", HandleAsync)
            .WithName("GetTagsPaginated")
            .WithSummary("Get tags by pages")
            .WithTags("Tags")
            .WithValidation<GetTagsPaginatedRequest>()
            .Produces<PaginatedList<TagDto>>(StatusCodes.Status200OK);
    }

    public static async Task<IResult> HandleAsync(
        [AsParameters] GetTagsPaginatedRequest request,
        IHandler<GetTagsPaginatedQuery, Result<PaginatedList<TagDto>>> handler,
        CancellationToken ct)
    {
        var query = new GetTagsPaginatedQuery(request.PageNumber, request.PageSize);
        var result = await handler.HandleAsync(query, ct);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return TypedResults.Ok(result.Value);
    }
}
