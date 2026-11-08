using System.ComponentModel;
using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Domain.Enums;
using JAT.API.Features.Interviews.Commands.CreateInterview;
using JAT.API.Features.Interviews.Commands.DeleteInterview;
using JAT.API.Features.Interviews.Commands.UpdateInterview;
using JAT.API.Features.Interviews.Dtos;
using JAT.API.Features.Interviews.Queries.GetInterview;
using JAT.API.Features.Interviews.Queries.GetInterviewsPaginated;
using ModelContextProtocol.Server;

namespace JAT.API.Mcp.Tools;

[McpServerToolType]
public static class InterviewTools
{
    [McpServerTool(Name = "get_interviews", ReadOnly = true, Destructive = false, Idempotent = true)]
    [Description("Gets paginated list of interviews with optional job application filter")]
    public static async Task<CallToolResult> GetInterviews(
        IHandler<GetInterviewsPaginatedQuery, Result<PaginatedList<InterviewDto>>> handler,
        CancellationToken ct,
        [Description("Page number, default = 1")] int pageNumber = 1,
        [Description("Page size, default = 10")] int pageSize = 10,
        [Description("Filter by job application ID")] Guid? jobApplicationId = null)
    {
        var query = new GetInterviewsPaginatedQuery(jobApplicationId, pageNumber, pageSize);

        var result = await handler.HandleAsync(query, ct);

        return result.ToToolResult();
    }

    [McpServerTool(Name = "get_interview", ReadOnly = true, Destructive = false, Idempotent = true)]
    [Description("Gets details of a specific interview by its ID")]
    public static async Task<CallToolResult> GetInterview(
        IHandler<GetInterviewQuery, Result<InterviewDto>> handler,
        CancellationToken ct,
        [Description("Unique identifier of the interview")] Guid id)
    {
        var query = new GetInterviewQuery(id);

        var result = await handler.HandleAsync(query, ct);

        return result.ToToolResult();
    }

    [McpServerTool(Name = "create_interview", ReadOnly = false, Destructive = false, Idempotent = false)]
    [Description("Creates an interview record for a job application")]
    public static async Task<CallToolResult> CreateInterview(
        IHandler<CreateInterviewCommand, Result<Guid>> handler,
        CancellationToken ct,
        [Description("Unique identifier of the job application")] Guid jobApplicationId,
        [Description("Type of interview (HrScreening, Technical, TestTask, Final, Other)")] InterviewType interviewType,
        [Description("Scheduled time of the interview (ISO 8601 with offset, e.g. 2026-11-10T14:00:00+02:00)")] DateTimeOffset scheduledAt,
        [Description("Location or meeting link (e.g. Zoom, Google Meet, office address)")] string? location = null,
        [Description("Notes or instructions for the interview")] string? notes = null)
    {
        var command = new CreateInterviewCommand(jobApplicationId, interviewType, scheduledAt, location, notes);

        var result = await handler.HandleAsync(command, ct);

        return result.ToToolResult(id => $"Interview: '{id}' created");
    }

    [McpServerTool(Name = "update_interview", ReadOnly = false, Destructive = false, Idempotent = true)]
    [Description("Updates an existing interview record")]
    public static async Task<CallToolResult> UpdateInterview(
        IHandler<UpdateInterviewCommand, Result> handler,
        CancellationToken ct,
        [Description("Unique identifier of the interview to update")] Guid id,
        [Description("Type of interview (HrScreening, Technical, TestTask, Final, Other)")] InterviewType interviewType,
        [Description("Scheduled time of the interview (ISO 8601 with offset, e.g. 2026-11-10T14:00:00+02:00)")] DateTimeOffset scheduledAt,
        [Description("Location or meeting link")] string? location = null,
        [Description("Notes or instructions for the interview")] string? notes = null)
    {
        var command = new UpdateInterviewCommand(id, interviewType, scheduledAt, location, notes);

        var result = await handler.HandleAsync(command, ct);

        return result.ToToolResult($"Interview: '{id}' updated");
    }

    [McpServerTool(Name = "delete_interview", ReadOnly = false, Destructive = true, Idempotent = true)]
    [Description("Deletes an interview record by its ID")]
    public static async Task<CallToolResult> DeleteInterview(
        IHandler<DeleteInterviewCommand, Result> handler,
        CancellationToken ct,
        [Description("Unique identifier of the interview to delete")] Guid id)
    {
        var command = new DeleteInterviewCommand(id);

        var result = await handler.HandleAsync(command, ct);

        return result.ToToolResult($"Interview: '{id}' deleted");
    }
}
