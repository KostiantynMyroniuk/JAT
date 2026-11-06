using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Domain.Entities;
using JAT.API.Extensions;
using JAT.API.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.Tags.Commands.CreateTag;

public sealed record CreateTagCommand(string Name);

public sealed class CreateTagValidator : AbstractValidator<CreateTagCommand>
{
    public CreateTagValidator()
    {
        RuleFor(t => t.Name)
            .NotEmpty()
            .MaximumLength(50);
    }
}

public sealed class CreateTagHandler(
    ApplicationDbContext context,
    IValidator<CreateTagCommand> validator)
    : IHandler<CreateTagCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(
        CreateTagCommand command,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return Result.Failure<Guid>(validationResult.ToValidationError());
        }

        var tag = Tag.Create(command.Name);

        context.Tags.Add(tag);

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return Result.Failure<Guid>(
                Error.Conflict($"Tag with name: {tag.Name} already exists"));
        }

        return Result.Success(tag.Id);
    }
}
