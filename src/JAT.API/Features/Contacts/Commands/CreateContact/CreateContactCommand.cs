using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Domain.Entities;
using JAT.API.Extensions;
using JAT.API.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.Contacts.Commands.CreateContact;

public sealed record CreateContactCommand(
    Guid JobApplicationId,
    string Name,
    string Email,
    string? LinkedIn);

public sealed class CreateContactValidator : AbstractValidator<CreateContactCommand>
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

public sealed class CreateContactHandler(
    ApplicationDbContext context,
    IValidator<CreateContactCommand> validator)
    : IHandler<CreateContactCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(
        CreateContactCommand command,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return Result.Failure<Guid>(validationResult.ToValidationError());
        }

        var applicationExists = await context.JobApplications
            .AnyAsync(a => a.Id == command.JobApplicationId, ct);

        if (!applicationExists)
        {
            return Result.Failure<Guid>(
                Error.NotFound($"Application: {command.JobApplicationId} not found"));
        }

        var contactExists = await context.Contacts
            .AnyAsync(c => c.JobApplicationId == command.JobApplicationId, ct);

        if (contactExists)
        {
            return Result.Failure<Guid>(
                Error.Conflict($"Contact for application: {command.JobApplicationId} already exists"));
        }

        var contact = new Contact
        {
            JobApplicationId = command.JobApplicationId,
            Name = command.Name,
            Email = command.Email,
            LinkedIn = command.LinkedIn
        };

        context.Contacts.Add(contact);

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return Result.Failure<Guid>(
                Error.Conflict($"Contact for application: {command.JobApplicationId} already exists"));
        }

        return Result.Success(contact.Id);
    }
}
