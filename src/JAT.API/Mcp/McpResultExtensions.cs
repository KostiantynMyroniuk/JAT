using JAT.API.Common.Models;
using ModelContextProtocol.Protocol;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JAT.API.Mcp
{
    public static class McpResultExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            Converters = { new JsonStringEnumConverter() }
        };

        public static CallToolResult ToToolResult<T>(this Result<T> result)
            => result.IsSuccess ? Text(JsonSerializer.Serialize(result.Value, _jsonOptions)) : Error(result.Error!);

        public static CallToolResult ToToolResult<T>(this Result<T> result, Func<T, string> successFormatter)
            => result.IsSuccess ? Text(successFormatter(result.Value)) : Error(result.Error!);

        public static CallToolResult ToToolResult(this Result result, string? successMessage = null)
            => result.IsSuccess ? Text(successMessage ?? "Operation succeded") : Error(result.Error!);

        public static CallToolResult Text(string text)
            => new() { Content = [new TextContentBlock() { Text = text }] };

        public static CallToolResult Error(Error error)
            => new() { IsError = true, Content = [new TextContentBlock() { Text = error.Description }] };
    }
}
