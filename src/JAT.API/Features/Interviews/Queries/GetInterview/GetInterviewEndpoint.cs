using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Extensions;
using JAT.API.Features.Interviews.Dtos;

namespace JAT.API.Features.Interviews.Queries.GetInterview;

public sealed class GetInterviewEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/interviews/{interviewId:guid}", HandleAsync)
            .WithName("GetInterview")
            .WithSummary("Get interview by id")
            .WithDescription("Returns the interview details")
            .WithTags("Interviews")
            .Produces<InterviewDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public static async Task<IResult> HandleAsync(
        Guid interviewId,
        IHandler<GetInterviewQuery, Result<InterviewDto>> handler,
        CancellationToken ct)
    {
        var query = new GetInterviewQuery(interviewId);
        var result = await handler.HandleAsync(query, ct);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return TypedResults.Ok(result.Value);
    }
}
