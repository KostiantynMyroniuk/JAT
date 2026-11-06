using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Extensions;

namespace JAT.API.Features.Contacts.Commands.UpdateContact;

public sealed record UpdateContactRequest(
    string Name,
    string Email,
    string? LinkedIn);

public sealed class UpdateContactEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/contacts/{contactId:guid}", HandleAsync)
            .WithName("UpdateContact")
            .WithSummary("Update contact record")
            .WithDescription("Updates contact record by its ID")
            .WithTags("Contacts")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    public static async Task<IResult> HandleAsync(
        Guid contactId,
        UpdateContactRequest request,
        IHandler<UpdateContactCommand, Result> handler,
        CancellationToken ct)
    {
        var command = new UpdateContactCommand(
            contactId,
            request.Name,
            request.Email,
            request.LinkedIn);

        var result = await handler.HandleAsync(command, ct);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return TypedResults.NoContent();
    }
}
