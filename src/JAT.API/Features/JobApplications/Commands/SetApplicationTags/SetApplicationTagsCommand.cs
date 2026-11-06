using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Extensions;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.JobApplications.Commands.SetApplicationTags;

public sealed record SetApplicationTagsCommand(Guid ApplicationId, Guid[] TagIds);

public sealed class SetApplicationTagsValidator : AbstractValidator<SetApplicationTagsCommand>
{
    public SetApplicationTagsValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty();

        RuleFor(x => x.TagIds)
            .NotNull()
            .Must(x => x.Length <= 20);

        RuleForEach(x => x.TagIds)
            .NotEmpty();
    }
}

public sealed class SetApplicationTagsHandler(
    ApplicationDbContext context,
    IValidator<SetApplicationTagsCommand> validator)
    : IHandler<SetApplicationTagsCommand, Result>
{
    public async Task<Result> HandleAsync(
        SetApplicationTagsCommand command,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return Result.Failure(validationResult.ToValidationError());
        }

        var application = await context.JobApplications
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.Id == command.ApplicationId, ct);

        if (application is null)
        {
            return Result.Failure(
                Error.NotFound($"Application: {command.ApplicationId} not found"));
        }

        var distinctTagIds = command.TagIds.Distinct().ToArray();

        var tags = await context.Tags
            .Where(t => distinctTagIds.Contains(t.Id))
            .ToListAsync(ct);

        if (tags.Count != distinctTagIds.Length)
        {
            var missingIds = distinctTagIds.Except(tags.Select(t => t.Id));
            return Result.Failure(
                Error.Validation($"Tags not found: {string.Join(", ", missingIds)}", "TagIds"));
        }

        application.SetTags(tags);
        await context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
