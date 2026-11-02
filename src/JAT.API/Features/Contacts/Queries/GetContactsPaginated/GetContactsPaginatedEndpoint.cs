using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Extensions;
using JAT.API.Features.Contacts.Dtos;
using JAT.API.Filters;

namespace JAT.API.Features.Contacts.Queries.GetContactsPaginated;

public sealed record GetContactsPaginatedRequest(
    Guid? JobApplicationId = null,
    int PageNumber = 1,
    int PageSize = 10);

public sealed class GetContactsPaginatedValidator : AbstractValidator<GetContactsPaginatedRequest>
{
    public GetContactsPaginatedValidator()
    {
        RuleFor(r => r.PageNumber)
            .InclusiveBetween(1, 100_000);

        RuleFor(r => r.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(r => r.JobApplicationId)
            .NotEmpty()
            .When(r => r.JobApplicationId.HasValue);
    }
}

public sealed class GetContactsPaginatedEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/contacts", HandleAsync)
            .WithName("GetContactsPaginated")
            .WithSummary("Get contacts records by pages")
            .WithDescription("Gets contacts records by pages with optional application filter")
            .WithTags("Contacts")
            .WithValidation<GetContactsPaginatedRequest>()
            .Produces<PaginatedList<ContactDto>>(StatusCodes.Status200OK);
    }

    public static async Task<IResult> HandleAsync(
        [AsParameters] GetContactsPaginatedRequest request,
        IHandler<GetContactsPaginatedQuery, Result<PaginatedList<ContactDto>>> handler,
        CancellationToken ct)
    {
        var query = new GetContactsPaginatedQuery(
            request.JobApplicationId,
            request.PageNumber,
            request.PageSize);

        var result = await handler.HandleAsync(query, ct);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return TypedResults.Ok(result.Value);
    }
}
