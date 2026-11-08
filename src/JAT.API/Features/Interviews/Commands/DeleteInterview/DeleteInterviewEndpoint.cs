using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Extensions;

namespace JAT.API.Features.Interviews.Commands.DeleteInterview;

public sealed class DeleteInterviewEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/interviews/{interviewId:guid}", HandleAsync)
            .WithName("DeleteInterview")
            .WithSummary("Delete interview record")
            .WithDescription("Deletes interview record by its ID")
            .WithTags("Interviews")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public static async Task<IResult> HandleAsync(
        Guid interviewId,
        IHandler<DeleteInterviewCommand, Result> handler,
        CancellationToken ct)
    {
        var command = new DeleteInterviewCommand(interviewId);
        var result = await handler.HandleAsync(command, ct);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return TypedResults.NoContent();
    }
}
