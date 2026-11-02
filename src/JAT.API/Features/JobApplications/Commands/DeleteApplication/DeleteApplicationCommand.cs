using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.JobApplications.Commands.DeleteApplication;

public sealed record DeleteApplicationCommand(Guid ApplicationId);

public sealed class DeleteApplicationHandler(ApplicationDbContext context)
    : IHandler<DeleteApplicationCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteApplicationCommand command,
        CancellationToken ct = default)
    {
        var rowsAffected = await context.JobApplications
            .Where(a => a.Id == command.ApplicationId)
            .ExecuteDeleteAsync(ct);

        if (rowsAffected == 0)
        {
            return Result.Failure(
                Error.NotFound($"Application: {command.ApplicationId} not found"));
        }

        return Result.Success();
    }
}
