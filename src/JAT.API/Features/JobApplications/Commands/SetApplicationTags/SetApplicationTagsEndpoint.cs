using FluentValidation;
using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Extensions;
using JAT.API.Filters;

namespace JAT.API.Features.JobApplications.Commands.SetApplicationTags;

public sealed record SetApplicationTagsRequest(Guid[] TagIds);

public sealed class SetApplicationTagsValidator : AbstractValidator<SetApplicationTagsRequest>
{
    public SetApplicationTagsValidator()
    {
        RuleFor(x => x.TagIds)
            .NotNull()
            .Must(x => x.Length <= 20);

        RuleForEach(x => x.TagIds)
            .NotEmpty();
    }
}

public sealed class SetApplicationTagsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/applications/{applicationId:guid}/tags", HandleAsync)
            .WithName("SetApplicationTags")
            .WithSummary("Set tags for job application")
            .WithDescription("Sets chosen tags for job application")
            .WithTags("JobApplications")
            .WithValidation<SetApplicationTagsRequest>()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    public static async Task<IResult> HandleAsync(
        Guid applicationId,
        SetApplicationTagsRequest request,
        IHandler<SetApplicationTagsCommand, Result> handler,
        CancellationToken ct)
    {
        var command = new SetApplicationTagsCommand(applicationId, request.TagIds);
        var result = await handler.HandleAsync(command, ct);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return TypedResults.NoContent();
    }
}
