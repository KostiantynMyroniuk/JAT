using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Domain.Enums;
using JAT.API.Extensions;

namespace JAT.API.Features.Interviews.Commands.CreateInterview;

public sealed record CreateInterviewRequest(
    Guid JobApplicationId,
    InterviewType InterviewType,
    DateTimeOffset ScheduledAt,
    string? Location,
    string? Notes);

public sealed class CreateInterviewEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/interviews", HandleAsync)
            .WithName("CreateInterview")
            .WithSummary("Create interview record")
            .WithDescription("Creates interview record for a job application")
            .WithTags("Interviews")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    public static async Task<IResult> HandleAsync(
        CreateInterviewRequest request,
        IHandler<CreateInterviewCommand, Result<Guid>> handler,
        CancellationToken ct)
    {
        var command = new CreateInterviewCommand(
            request.JobApplicationId,
            request.InterviewType,
            request.ScheduledAt,
            request.Location,
            request.Notes);

        var result = await handler.HandleAsync(command, ct);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return TypedResults.CreatedAtRoute(
            result.Value,
            "GetInterview",
            new { interviewId = result.Value });
    }
}
