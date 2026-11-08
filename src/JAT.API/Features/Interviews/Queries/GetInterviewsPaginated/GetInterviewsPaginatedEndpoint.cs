using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Extensions;
using JAT.API.Features.Interviews.Dtos;

namespace JAT.API.Features.Interviews.Queries.GetInterviewsPaginated;

public sealed record GetInterviewsPaginatedRequest(
    Guid? JobApplicationId = null,
    int PageNumber = 1,
    int PageSize = 10);

public sealed class GetInterviewsPaginatedEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/interviews", HandleAsync)
            .WithName("GetInterviewsPaginated")
            .WithSummary("Get interview records by pages")
            .WithDescription("Gets interview records by pages with optional job application filter")
            .WithTags("Interviews")
            .Produces<PaginatedList<InterviewDto>>(StatusCodes.Status200OK);
    }

    public static async Task<IResult> HandleAsync(
        [AsParameters] GetInterviewsPaginatedRequest request,
        IHandler<GetInterviewsPaginatedQuery, Result<PaginatedList<InterviewDto>>> handler,
        CancellationToken ct)
    {
        var query = new GetInterviewsPaginatedQuery(request.JobApplicationId, request.PageNumber, request.PageSize);
        var result = await handler.HandleAsync(query, ct);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return TypedResults.Ok(result.Value);
    }
}
