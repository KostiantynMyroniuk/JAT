using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.Contacts.Commands.DeleteContact;

public sealed record DeleteContactCommand(Guid ContactId);

public sealed class DeleteContactHandler(ApplicationDbContext context)
    : IHandler<DeleteContactCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteContactCommand command,
        CancellationToken ct = default)
    {
        var rowsAffected = await context.Contacts
            .Where(c => c.Id == command.ContactId)
            .ExecuteDeleteAsync(ct);

        if (rowsAffected == 0)
        {
            return Result.Failure(
                Error.NotFound($"Contact: {command.ContactId} not found"));
        }

        return Result.Success();
    }
}
