using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Domain.Enums;
using JAT.API.Infrastructure.Persistence;

namespace JAT.API.Features.JobApplications.Commands.UpdateApplication;

public sealed record UpdateApplicationCommand(
    Guid ApplicationId,
    string Title,
    string? Notes,
    JobApplicationStatus Status,
    string? Url,
    DateTimeOffset? AppliedAt,
    decimal? SalaryMin,
    decimal? SalaryMax);

public sealed class UpdateApplicationHandler(ApplicationDbContext context)
    : IHandler<UpdateApplicationCommand, Result>
{
    public async Task<Result> HandleAsync(
        UpdateApplicationCommand command,
        CancellationToken ct = default)
    {
        var application = await context.JobApplications
            .FindAsync([command.ApplicationId], ct);

        if (application is null)
        {
            return Result.Failure(
                Error.NotFound($"Application: {command.ApplicationId} not found"));
        }

        application.Update(
            command.Title,
            command.Notes,
            command.Status,
            command.Url,
            command.AppliedAt,
            command.SalaryMin,
            command.SalaryMax);

        await context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
