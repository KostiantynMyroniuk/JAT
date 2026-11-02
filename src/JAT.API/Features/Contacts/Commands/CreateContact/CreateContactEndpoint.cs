using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Extensions;
using JAT.API.Filters;

namespace JAT.API.Features.Contacts.Commands.CreateContact;

public sealed record CreateContactRequest(
    Guid JobApplicationId,
    string Name,
    string Email,
    string? LinkedIn);

public sealed class CreateContactValidator : AbstractValidator<CreateContactRequest>
{
    public CreateContactValidator()
    {
        RuleFor(c => c.JobApplicationId)
            .NotEmpty();

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

public sealed class CreateContactEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/contacts", HandleAsync)
            .WithName("CreateContact")
            .WithSummary("Create contact record")
            .WithDescription("Creates contact record for a job application")
            .WithTags("Contacts")
            .WithValidation<CreateContactRequest>()
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    public static async Task<IResult> HandleAsync(
        CreateContactRequest request,
        IHandler<CreateContactCommand, Result<Guid>> handler,
        CancellationToken ct)
    {
        var command = new CreateContactCommand(
            request.JobApplicationId,
            request.Name,
            request.Email,
            request.LinkedIn);

        var result = await handler.HandleAsync(command, ct);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return TypedResults.CreatedAtRoute(
            result.Value,
            "GetContact",
            new { contactId = result.Value });
    }
}
