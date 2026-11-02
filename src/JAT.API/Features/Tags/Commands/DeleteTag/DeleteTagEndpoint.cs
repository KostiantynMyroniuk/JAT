using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Extensions;

namespace JAT.API.Features.Tags.Commands.DeleteTag;

public sealed class DeleteTagEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/tags/{tagId:guid}", HandleAsync)
            .WithName("DeleteTag")
            .WithSummary("Delete tag")
            .WithTags("Tags")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public static async Task<IResult> HandleAsync(
        Guid tagId,
        IHandler<DeleteTagCommand, Result> handler,
        CancellationToken ct)
    {
        var command = new DeleteTagCommand(tagId);
        var result = await handler.HandleAsync(command, ct);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return TypedResults.NoContent();
    }
}
