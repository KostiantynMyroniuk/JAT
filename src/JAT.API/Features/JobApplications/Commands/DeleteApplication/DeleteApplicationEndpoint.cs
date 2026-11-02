using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Extensions;

namespace JAT.API.Features.JobApplications.Commands.DeleteApplication;

public sealed class DeleteApplicationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/applications/{applicationId:guid}", HandleAsync)
            .WithName("DeleteJobApplication")
            .WithSummary("Delete job application record")
            .WithDescription("Deletes job application record")
            .WithTags("JobApplications")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public static async Task<IResult> HandleAsync(
        Guid applicationId,
        IHandler<DeleteApplicationCommand, Result> handler,
        CancellationToken ct)
    {
        var command = new DeleteApplicationCommand(applicationId);
        var result = await handler.HandleAsync(command, ct);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return TypedResults.NoContent();
    }
}
