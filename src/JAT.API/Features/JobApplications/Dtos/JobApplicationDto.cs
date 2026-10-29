using JAT.API.Domain.Enums;
using static JAT.API.Features.JobApplications.GetApplication;

namespace JAT.API.Features.JobApplications.Dtos
{
    public sealed record JobApplicationDto(
        Guid Id,
        string Title,
        string? Notes,
        JobApplicationStatus Status,
        string? Url,
        DateTimeOffset? AppliedAt,
        decimal? SalaryMin,
        decimal? SalaryMax,
        IReadOnlyList<JobApplicationTagDto> Tags);
}
