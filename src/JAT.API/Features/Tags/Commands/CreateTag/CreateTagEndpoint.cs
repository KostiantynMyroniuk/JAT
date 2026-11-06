using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Extensions;

namespace JAT.API.Features.Tags.Commands.CreateTag;

public sealed record CreateTagRequest(string Name);

public sealed class CreateTagEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/tags", HandleAsync)
            .WithName("CreateTag")
            .WithSummary("Create tag")
            .WithTags("Tags")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    public static async Task<IResult> HandleAsync(
        CreateTagRequest request,
        IHandler<CreateTagCommand, Result<Guid>> handler,
        CancellationToken ct)
    {
        var command = new CreateTagCommand(request.Name);
        var result = await handler.HandleAsync(command, ct);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return TypedResults.CreatedAtRoute(
            result.Value,
            "GetTagsPaginated",
            new { tagId = result.Value });
    }
}
