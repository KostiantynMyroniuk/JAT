using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Domain.Enums;
using JAT.API.Features.JobApplications.Commands.CreateApplication;
using JAT.API.Features.JobApplications.Dtos;
using JAT.API.Features.JobApplications.Queries.GetApplicationsPaginated;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace JAT.API.Mcp.Tools
{
    [McpServerToolType]
    public static class JobApplicationTools
    {
        [McpServerTool(Name = "get_job_applications", ReadOnly = true)] 
        [Description("Gets paginated list of job applications with current statuses and tags")]
        public static async Task<string> GetApplications(
            IHandler<GetApplicationsPaginatedQuery, Result<PaginatedList<JobApplicationDto>>> handler,
            CancellationToken ct,
            [Description("Page number, default = 1")] int pageNumber = 1,
            [Description("Page size, default = 10")] int pageSize = 10,
            [Description("Filter by status")] JobApplicationStatus? status = null)
        {
            var query = new GetApplicationsPaginatedQuery(status, pageNumber, pageSize);

            var result = await handler.HandleAsync(query, ct);

            if (result.IsFailure)
            {
                return JsonSerializer.Serialize(new { error = result.Error?.Description });
            }

            return JsonSerializer.Serialize(result.Value);
        }

        [McpServerTool(Name = "create_job_application", ReadOnly = false, Destructive = false, Idempotent = false)] 
        [Description("Creates job application record")]
        public static async Task<string> CreateApplication(
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

            if (result.IsFailure)
            {
                return JsonSerializer.Serialize(new { error = result.Error?.Description });
            }

            return JsonSerializer.Serialize(new
            {
                id = result.Value,
                message = "Job application created"
            });
        }
    }
}
