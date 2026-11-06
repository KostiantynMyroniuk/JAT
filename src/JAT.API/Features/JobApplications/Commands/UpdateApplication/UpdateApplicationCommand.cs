using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Domain.Enums;
using JAT.API.Extensions;
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

public sealed class UpdateApplicationValidator : AbstractValidator<UpdateApplicationCommand>
{
    public UpdateApplicationValidator(TimeProvider timeProvider)
    {
        RuleFor(a => a.ApplicationId)
            .NotEmpty();

        RuleFor(a => a.Title)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(a => a.Notes)
            .MaximumLength(2000);

        RuleFor(a => a.Status)
            .IsInEnum();

        RuleFor(a => a.Url)
            .MaximumLength(200)
            .Must(BeUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.Url));

        RuleFor(a => a.AppliedAt)
            .NotEmpty()
            .LessThanOrEqualTo(_ => timeProvider.GetUtcNow().AddDays(1));

        RuleFor(a => a.SalaryMin)
            .GreaterThanOrEqualTo(0);

        RuleFor(a => a.SalaryMax)
            .GreaterThanOrEqualTo(a => a.SalaryMin);
    }

    private static bool BeUrl(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https";
}

public sealed class UpdateApplicationHandler(
    ApplicationDbContext context,
    IValidator<UpdateApplicationCommand> validator)
    : IHandler<UpdateApplicationCommand, Result>
{
    public async Task<Result> HandleAsync(
        UpdateApplicationCommand command,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return Result.Failure(validationResult.ToValidationError());
        }

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
