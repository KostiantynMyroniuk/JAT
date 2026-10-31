using JAT.API.Common.Models;
using JAT.API.Domain.Entities;
using JAT.API.Domain.Enums;
using JAT.API.Features.JobApplications.Dtos;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
            ApplicationDbContext context,
            CancellationToken ct,
            [Description("Page number, default = 1")] int pageNumber = 1,
            [Description("Page size, default = 10")] int pageSize = 10,
            [Description("Filter by status")] JobApplicationStatus? status = null)
        {
            var query = context.JobApplications.AsNoTracking();

            if (status.HasValue)
                query = query.Where(a => a.Status == status);

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderBy(a => a.CreatedAt)
                .ThenBy(a => a.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new JobApplicationDto(
                    a.Id,
                    a.Title,
                    a.Notes,
                    a.Status,
                    a.Url,
                    a.AppliedAt,
                    a.SalaryMin,
                    a.SalaryMax,
                    a.Tags.Select(t => new JobApplicationTagDto(
                        t.Id, 
                        t.Name)).ToList()))
                .ToListAsync(ct);

            return JsonSerializer.Serialize(
                new PaginatedList<JobApplicationDto>(items, pageNumber, pageSize, totalCount));
        }

        [McpServerTool(Name = "create_job_application", ReadOnly = false, Destructive = false, Idempotent = false)] 
        [Description("Creates job application record")]
        public static async Task<string> CreateApplication(
            ApplicationDbContext context,
            CancellationToken ct,
            [Description("Short job title")] string title,
            [Description("Current status of the job application record")] JobApplicationStatus status,
            [Description("Free-form notes about the job")] string? notes = null,
            [Description("Absolute http/https URL of the job posting")] string? url = null,
            [Description("When the application was sent, ISO 8601 with offset, e.g. 2026-10-31T12:00:00+02:00")] DateTimeOffset? appliedAt = null,
            [Description("Start of the salary range")] decimal? salaryMin = null,
            [Description("End of the salary range")] decimal? salaryMax = null)
        {
            var application = new JobApplication()
            {
                Title = title,
                Notes = notes,
                Status = status,
                Url = url,
                AppliedAt = appliedAt,
                SalaryMin = salaryMin,
                SalaryMax = salaryMax
            };

            context.JobApplications.Add(application);
            await context.SaveChangesAsync(ct);

            return JsonSerializer.Serialize(new
            {
                application.Id,
                application.Status,
                message = "Job application created"
            });
        }
    }
}
