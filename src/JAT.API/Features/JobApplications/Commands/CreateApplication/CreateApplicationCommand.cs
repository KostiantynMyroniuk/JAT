using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Domain.Entities;
using JAT.API.Domain.Enums;
using JAT.API.Extensions;
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

public sealed class CreateApplicationValidator : AbstractValidator<CreateApplicationCommand>
{
    public CreateApplicationValidator(TimeProvider timeProvider)
    {
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
            .LessThanOrEqualTo(_ => timeProvider.GetUtcNow().AddDays(1));

        RuleFor(a => a.SalaryMin)
            .GreaterThanOrEqualTo(0);

        RuleFor(a => a.SalaryMax)
            .GreaterThanOrEqualTo(a => a.SalaryMin);
    }

    private static bool BeUrl(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https";
}

public sealed class CreateApplicationHandler(
    ApplicationDbContext context,
    IValidator<CreateApplicationCommand> validator)
    : IHandler<CreateApplicationCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(
        CreateApplicationCommand command,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return Result.Failure<Guid>(validationResult.ToValidationError());
        }

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
