using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Extensions;
using JAT.API.Infrastructure.Persistence;

namespace JAT.API.Features.Contacts.Commands.UpdateContact;

public sealed record UpdateContactCommand(
    Guid ContactId,
    string Name,
    string Email,
    string? LinkedIn);

public sealed class UpdateContactValidator : AbstractValidator<UpdateContactCommand>
{
    public UpdateContactValidator()
    {
        RuleFor(c => c.ContactId)
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

public sealed class UpdateContactHandler(
    ApplicationDbContext context,
    IValidator<UpdateContactCommand> validator)
    : IHandler<UpdateContactCommand, Result>
{
    public async Task<Result> HandleAsync(
        UpdateContactCommand command,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return Result.Failure(validationResult.ToValidationError());
        }

        var contact = await context.Contacts
            .FindAsync([command.ContactId], ct);

        if (contact is null)
        {
            return Result.Failure(
                Error.NotFound($"Contact: {command.ContactId} not found"));
        }

        contact.Name = command.Name;
        contact.Email = command.Email;
        contact.LinkedIn = command.LinkedIn;

        await context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
