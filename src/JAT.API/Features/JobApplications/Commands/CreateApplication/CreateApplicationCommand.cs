using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Domain.Entities;
using JAT.API.Domain.Enums;
using JAT.API.Infrastructure.Persistence;

namespace JAT.API.Features.JobApplications.Commands.CreateApplication;

public sealed record CreateApplicationCommand(
    string Title,
    string? Notes,
    JobApplicationStatus Status,
    string? Url,
    DateTimeOffset? AppliedAt,
    decimal? SalaryMin,
    decimal? SalaryMax);

public sealed class CreateApplicationHandler(ApplicationDbContext context)
    : IHandler<CreateApplicationCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(
        CreateApplicationCommand command,
        CancellationToken ct = default)
    {
        var application = new JobApplication
        {
            Title = command.Title,
            Notes = command.Notes,
            Status = command.Status,
            Url = command.Url,
            AppliedAt = command.AppliedAt,
            SalaryMin = command.SalaryMin,
            SalaryMax = command.SalaryMax
        };

        context.JobApplications.Add(application);
        await context.SaveChangesAsync(ct);

        return Result.Success(application.Id);
    }
}
