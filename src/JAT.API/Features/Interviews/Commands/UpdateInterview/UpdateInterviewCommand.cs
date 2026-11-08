using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Domain.Enums;
using JAT.API.Extensions;
using JAT.API.Infrastructure.Persistence;

namespace JAT.API.Features.Interviews.Commands.UpdateInterview;

public sealed record UpdateInterviewCommand(
    Guid InterviewId,
    InterviewType InterviewType,
    DateTimeOffset ScheduledAt,
    string? Location,
    string? Notes);

public sealed class UpdateInterviewValidator : AbstractValidator<UpdateInterviewCommand>
{
    public UpdateInterviewValidator()
    {
        RuleFor(c => c.InterviewId)
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

public sealed class UpdateInterviewHandler(
    ApplicationDbContext context,
    IValidator<UpdateInterviewCommand> validator)
    : IHandler<UpdateInterviewCommand, Result>
{
    public async Task<Result> HandleAsync(
        UpdateInterviewCommand command,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return Result.Failure(validationResult.ToValidationError());
        }

        var interview = await context.Interviews
            .FindAsync([command.InterviewId], ct);

        if (interview is null)
        {
            return Result.Failure(
                Error.NotFound($"Interview: {command.InterviewId} not found"));
        }

        interview.Update(
            command.InterviewType,
            command.ScheduledAt,
            command.Location,
            command.Notes);

        await context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
