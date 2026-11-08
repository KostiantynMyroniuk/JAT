using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Domain.Enums;
using JAT.API.Extensions;

namespace JAT.API.Features.Interviews.Commands.UpdateInterview;

public sealed record UpdateInterviewRequest(
    InterviewType InterviewType,
    DateTimeOffset ScheduledAt,
    string? Location,
    string? Notes);

public sealed class UpdateInterviewEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/interviews/{interviewId:guid}", HandleAsync)
            .WithName("UpdateInterview")
            .WithSummary("Update interview record")
            .WithDescription("Updates interview record by its ID")
            .WithTags("Interviews")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    public static async Task<IResult> HandleAsync(
        Guid interviewId,
        UpdateInterviewRequest request,
        IHandler<UpdateInterviewCommand, Result> handler,
        CancellationToken ct)
    {
        var command = new UpdateInterviewCommand(
            interviewId,
            request.InterviewType,
            request.ScheduledAt,
            request.Location,
            request.Notes);

        var result = await handler.HandleAsync(command, ct);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return TypedResults.NoContent();
    }
}
