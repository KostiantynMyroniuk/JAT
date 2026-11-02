using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Extensions;

namespace JAT.API.Features.Contacts.Commands.DeleteContact;

public sealed class DeleteContactEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/contacts/{contactId:guid}", HandleAsync)
            .WithName("DeleteContact")
            .WithSummary("Delete contact record")
            .WithDescription("Deletes contact record")
            .WithTags("Contacts")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public static async Task<IResult> HandleAsync(
        Guid contactId,
        IHandler<DeleteContactCommand, Result> handler,
        CancellationToken ct)
    {
        var command = new DeleteContactCommand(contactId);
        var result = await handler.HandleAsync(command, ct);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return TypedResults.NoContent();
    }
}
