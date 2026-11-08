using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Domain.Entities;
using JAT.API.Domain.Enums;
using JAT.API.Extensions;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.Interviews.Commands.CreateInterview;

public sealed record CreateInterviewCommand(
    Guid JobApplicationId,
    InterviewType InterviewType,
    DateTimeOffset ScheduledAt,
    string? Location,
    string? Notes);

public sealed class CreateInterviewValidator : AbstractValidator<CreateInterviewCommand>
{
    public CreateInterviewValidator()
    {
        RuleFor(c => c.JobApplicationId)
            .NotEmpty();

        RuleFor(c => c.InterviewType)
            .IsInEnum();

        RuleFor(c => c.ScheduledAt)
            .NotEmpty();

        RuleFor(c => c.Location)
            .MaximumLength(200);

        RuleFor(c => c.Notes)
            .MaximumLength(500);
    }
}

public sealed class CreateInterviewHandler(
    ApplicationDbContext context,
    IValidator<CreateInterviewCommand> validator)
    : IHandler<CreateInterviewCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(
        CreateInterviewCommand command,
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

        var interview = new Interview
        {
            JobApplicationId = command.JobApplicationId,
            InterviewType = command.InterviewType,
            ScheduledAt = command.ScheduledAt,
            Location = command.Location,
            Notes = command.Notes
        };

        context.Interviews.Add(interview);
        await context.SaveChangesAsync(ct);

        return Result.Success(interview.Id);
    }
}
