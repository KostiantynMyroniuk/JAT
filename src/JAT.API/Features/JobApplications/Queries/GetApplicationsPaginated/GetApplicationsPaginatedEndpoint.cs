using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Domain.Enums;
using JAT.API.Extensions;
using JAT.API.Features.JobApplications.Dtos;
using JAT.API.Filters;

namespace JAT.API.Features.JobApplications.Queries.GetApplicationsPaginated;

public sealed record GetApplicationsPaginatedRequest(
    JobApplicationStatus? Status = null,
    int PageNumber = 1,
    int PageSize = 10);

public sealed class GetApplicationsPaginatedValidator : AbstractValidator<GetApplicationsPaginatedRequest>
{
    public GetApplicationsPaginatedValidator()
    {
        RuleFor(r => r.PageNumber)
            .InclusiveBetween(1, 100_000);

        RuleFor(r => r.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(r => r.Status)
            .IsInEnum()
            .When(r => r.Status.HasValue);
    }
}

public sealed class GetApplicationsPaginatedEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/applications", HandleAsync)
            .WithName("GetApplicationsPaginated")
            .WithSummary("Get applications records by pages")
            .WithDescription("Gets applications records with its tags by pages")
            .WithTags("JobApplications")
            .WithValidation<GetApplicationsPaginatedRequest>()
            .Produces<PaginatedList<JobApplicationDto>>(StatusCodes.Status200OK);
    }

    public static async Task<IResult> HandleAsync(
        [AsParameters] GetApplicationsPaginatedRequest request,
        IHandler<GetApplicationsPaginatedQuery, Result<PaginatedList<JobApplicationDto>>> handler,
        CancellationToken ct)
    {
        var query = new GetApplicationsPaginatedQuery(request.Status, request.PageNumber, request.PageSize);
        var result = await handler.HandleAsync(query, ct);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return TypedResults.Ok(result.Value);
    }
}
