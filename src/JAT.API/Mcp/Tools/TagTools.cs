using System.ComponentModel;
using System.Text.Json;
using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Features.Tags.Commands.CreateTag;
using JAT.API.Features.Tags.Commands.DeleteTag;
using JAT.API.Features.Tags.Dtos;
using JAT.API.Features.Tags.Queries.GetTagsPaginated;
using ModelContextProtocol.Server;

namespace JAT.API.Mcp.Tools;

[McpServerToolType]
public static class TagTools
{
    [McpServerTool(Name = "get_tags", ReadOnly = true, Destructive = false, Idempotent = true)]
    [Description("Gets all tags paginated")]
    public static async Task<string> GetTags(
        IHandler<GetTagsPaginatedQuery, Result<PaginatedList<TagDto>>> handler,
        CancellationToken ct,
        [Description("Page number, default = 1")] int pageNumber = 1,
        [Description("Page size, default = 10")] int pageSize = 10)
    {
        var query = new GetTagsPaginatedQuery(pageNumber, pageSize);

        var result = await handler.HandleAsync(query, ct);

        if (result.IsFailure)
        {
            return JsonSerializer.Serialize(new { error = result.Error?.Description });
        }

        return JsonSerializer.Serialize(result.Value);
    }

    [McpServerTool(Name = "create_tag", ReadOnly = false, Destructive = false, Idempotent = false)]
    [Description("Creates a new tag")]
    public static async Task<string> CreateTag(
        IHandler<CreateTagCommand, Result<Guid>> handler,
        CancellationToken ct,
        [Description("Short tag name")] string name)
    {
        var command = new CreateTagCommand(name);

        var result = await handler.HandleAsync(command, ct);

        if (result.IsFailure)
        {
            return JsonSerializer.Serialize(new { error = result.Error?.Description });
        }

        return JsonSerializer.Serialize(new
        {
            id = result.Value,
            message = "Tag created"
        });
    }

    [McpServerTool(Name = "delete_tag", ReadOnly = false, Destructive = true, Idempotent = true)]
    [Description("Deletes a tag by its ID")]
    public static async Task<string> DeleteTag(
        IHandler<DeleteTagCommand, Result> handler,
        CancellationToken ct,
        [Description("Unique identifier of the tag which should be deleted")] Guid tagId)
    {
        var command = new DeleteTagCommand(tagId);

        var result = await handler.HandleAsync(command, ct);

        if (result.IsFailure)
        {
            return JsonSerializer.Serialize(new { error = result.Error?.Description });
        }

        return JsonSerializer.Serialize(new
        {
            tagId,
            message = "Tag deleted"
        });
    }
}
