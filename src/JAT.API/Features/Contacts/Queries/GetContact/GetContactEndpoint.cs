using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Extensions;
using JAT.API.Features.Contacts.Dtos;

namespace JAT.API.Features.Contacts.Queries.GetContact;

public sealed class GetContactEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/contacts/{contactId:guid}", HandleAsync)
            .WithName("GetContact")
            .WithSummary("Get contact by id")
            .WithDescription("Returns the contact details")
            .WithTags("Contacts")
            .Produces<ContactDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public static async Task<IResult> HandleAsync(
        Guid contactId,
        IHandler<GetContactQuery, Result<ContactDto>> handler,
        CancellationToken ct)
    {
        var query = new GetContactQuery(contactId);
        var result = await handler.HandleAsync(query, ct);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return TypedResults.Ok(result.Value);
    }
}
