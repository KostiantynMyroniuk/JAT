using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Infrastructure.Persistence;

namespace JAT.API.Features.Contacts.Commands.UpdateContact;

public sealed record UpdateContactCommand(
    Guid ContactId,
    string Name,
    string Email,
    string? LinkedIn);

public sealed class UpdateContactHandler(ApplicationDbContext context)
    : IHandler<UpdateContactCommand, Result>
{
    public async Task<Result> HandleAsync(
        UpdateContactCommand command,
        CancellationToken ct = default)
    {
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
