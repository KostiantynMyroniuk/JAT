using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Extensions;
using JAT.API.Filters;

namespace JAT.API.Features.Contacts.Commands.UpdateContact;

public sealed record UpdateContactRequest(
    string Name,
    string Email,
    string? LinkedIn);

public sealed class UpdateContactValidator : AbstractValidator<UpdateContactRequest>
{
    public UpdateContactValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(c => c.Email)
            .NotEmpty()
            .MaximumLength(100)
            .EmailAddress();

        RuleFor(c => c.LinkedIn)
            .MaximumLength(200)
            .Must(BeUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.LinkedIn));
    }

    private static bool BeUrl(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https";
}

public sealed class UpdateContactEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/contacts/{contactId:guid}", HandleAsync)
            .WithName("UpdateContact")
            .WithSummary("Update contact record")
            .WithDescription("Updates contact record")
            .WithTags("Contacts")
            .WithValidation<UpdateContactRequest>()
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
