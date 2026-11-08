using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.Interviews.Commands.DeleteInterview;

public sealed record DeleteInterviewCommand(Guid InterviewId);

public sealed class DeleteInterviewHandler(ApplicationDbContext context)
    : IHandler<DeleteInterviewCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteInterviewCommand command,
        CancellationToken ct = default)
    {
        var rowsAffected = await context.Interviews
            .Where(i => i.Id == command.InterviewId)
            .ExecuteDeleteAsync(ct);

        if (rowsAffected == 0)
        {
            return Result.Failure(
                Error.NotFound($"Interview: {command.InterviewId} not found"));
        }

        return Result.Success();
    }
}
