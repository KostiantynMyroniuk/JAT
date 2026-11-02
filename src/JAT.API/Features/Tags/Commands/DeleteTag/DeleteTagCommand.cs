using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.Tags.Commands.DeleteTag;

public sealed record DeleteTagCommand(Guid TagId);

public sealed class DeleteTagHandler(ApplicationDbContext context)
    : IHandler<DeleteTagCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteTagCommand command,
        CancellationToken ct = default)
    {
        var rowsAffected = await context.Tags
            .Where(t => t.Id == command.TagId)
            .ExecuteDeleteAsync(ct);

        if (rowsAffected == 0)
        {
            return Result.Failure(
                Error.NotFound($"Tag: {command.TagId} not found"));
        }

        return Result.Success();
    }
}
