using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Domain.Enums;
using JAT.API.Features.JobApplications.Commands.CreateApplication;
using JAT.API.Features.JobApplications.Commands.DeleteApplication;
using JAT.API.Features.JobApplications.Commands.SetApplicationTags;
using JAT.API.Features.JobApplications.Commands.UpdateApplication;
using JAT.API.Features.JobApplications.Dtos;
using JAT.API.Features.JobApplications.Queries.GetApplication;
using JAT.API.Features.JobApplications.Queries.GetApplicationsPaginated;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace JAT.API.Mcp.Tools
{
    [McpServerToolType]
    public static class JobApplicationTools
    {
        [McpServerTool(Name = "get_job_applications", ReadOnly = true)] 
        [Description("Gets paginated list of job applications with current statuses and tags")]
        public static async Task<CallToolResult> GetApplications(
            IHandler<GetApplicationsPaginatedQuery, Result<PaginatedList<JobApplicationDto>>> handler,
            CancellationToken ct,
            [Description("Page number, default = 1")] int pageNumber = 1,
            [Description("Page size, default = 10")] int pageSize = 10,
            [Description("Filter by status")] JobApplicationStatus? status = null)
        {
            var query = new GetApplicationsPaginatedQuery(status, pageNumber, pageSize);

            var result = await handler.HandleAsync(query, ct);

            return result.ToToolResult();
        }

        [McpServerTool(Name = "get_job_application", ReadOnly = true)]
        [Description("Gets details of a specific job application by its ID, including its tags")]
        public static async Task<CallToolResult> GetApplication(
            IHandler<GetApplicationQuery, Result<JobApplicationDto>> handler,
            CancellationToken ct,
            [Description("Unique identifier of the job application")] Guid id)
        {
            var query = new GetApplicationQuery(id);

            var result = await handler.HandleAsync(query, ct);

            return result.ToToolResult();
        }

        [McpServerTool(Name = "create_job_application", ReadOnly = false, Destructive = false, Idempotent = false)] 
        [Description("Creates job application record")]
        public static async Task<CallToolResult> CreateApplication(
            IHandler<CreateApplicationCommand, Result<Guid>> handler,
            CancellationToken ct,
            [Description("Short job title")] string title,
            [Description("Current status of the job application record")] JobApplicationStatus status,
            [Description("Free-form notes about the job")] string? notes = null,
            [Description("Absolute http/https URL of the job posting")] string? url = null,
            [Description("When the application was sent, ISO 8601 with offset, e.g. 2026-10-31T12:00:00+02:00")] DateTimeOffset? appliedAt = null,
            [Description("Start of the salary range")] decimal? salaryMin = null,
            [Description("End of the salary range")] decimal? salaryMax = null)
        {
            var command = new CreateApplicationCommand(title, notes, status, url, appliedAt, salaryMin, salaryMax);

            var result = await handler.HandleAsync(command, ct);

            return result.ToToolResult(id => $"Job application: '{id}' created");
        }

        [McpServerTool(Name = "update_job_application", ReadOnly = false, Destructive = false, Idempotent = true)]
        [Description("Updates an existing job application record")]
        public static async Task<CallToolResult> UpdateApplication(
            IHandler<UpdateApplicationCommand, Result> handler,
            CancellationToken ct,
            [Description("Unique identifier of the job application to update")] Guid id,
            [Description("Short job title")] string title,
            [Description("Current status of the job application record")] JobApplicationStatus status,
            [Description("Free-form notes about the job")] string? notes = null,
            [Description("Absolute http/https URL of the job posting")] string? url = null,
            [Description("When the application was sent, ISO 8601 with offset, e.g. 2026-10-31T12:00:00+02:00")] DateTimeOffset? appliedAt = null,
            [Description("Start of the salary range")] decimal? salaryMin = null,
            [Description("End of the salary range")] decimal? salaryMax = null)
        {
            var command = new UpdateApplicationCommand(id, title, notes, status, url, appliedAt, salaryMin, salaryMax);

            var result = await handler.HandleAsync(command, ct);

            return result.ToToolResult($"Job application: '{id}' updated");
        }

        [McpServerTool(Name = "delete_job_application", ReadOnly = false, Destructive = true, Idempotent = true)]
        [Description("Deletes a job application record by its ID")]
        public static async Task<CallToolResult> DeleteApplication(
            IHandler<DeleteApplicationCommand, Result> handler,
            CancellationToken ct,
            [Description("Unique identifier of the job application to delete")] Guid id)
        {
            var command = new DeleteApplicationCommand(id);

            var result = await handler.HandleAsync(command, ct);

            return result.ToToolResult($"Job application: '{id}' deleted");
        }

        [McpServerTool(Name = "set_job_application_tags", ReadOnly = false, Destructive = false, Idempotent = true)]
        [Description("Sets or replaces the tags associated with a job application")]
        public static async Task<CallToolResult> SetApplicationTags(
            IHandler<SetApplicationTagsCommand, Result> handler,
            CancellationToken ct,
            [Description("Unique identifier of the job application")] Guid id,
            [Description("List of tag IDs to assign to the job application")] Guid[] tagIds)
        {
            var command = new SetApplicationTagsCommand(id, tagIds);

            var result = await handler.HandleAsync(command, ct);

            return result.ToToolResult($"Tags for job application: '{id}' updated");
        }
    }
}
