using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Domain.Enums;
using JAT.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JAT.API.Features.Analytics.Queries.GetDashboardSummary
{
    public sealed record GetDashboardSummaryQuery();

    public sealed record GetDashboardSummaryResponse(
        OverviewKpiDto OverviewKpi,
        SalaryStatisticsDto SalaryStatistics,
        IReadOnlyList<UpcomingInterviewDto> UpcomingInterviews);

    public sealed record OverviewKpiDto(
        int Total,
        int Active,
        int Applied,
        int Rejected,
        int Ghosted,
        int Offers,
        int WithInterviews,
        double? InterviewRate,
        double? OfferRate);

    public sealed record SalaryStatisticsDto(
        decimal? Min,
        decimal? Max,
        decimal? Average);

    public sealed record UpcomingInterviewDto(
        Guid Id,
        string Title,
        InterviewType InterviewType,
        DateTimeOffset ScheduledAt,
        string? Location);

    public class GetDashboardSummaryHandler(
        ApplicationDbContext context,
        TimeProvider timeProvider) : IHandler<GetDashboardSummaryQuery, Result<GetDashboardSummaryResponse>>
    {
        public async Task<Result<GetDashboardSummaryResponse>> HandleAsync(GetDashboardSummaryQuery request, CancellationToken ct = default)
        {
            var statusCounts = await GetStatusCountsAsync(ct);
            var salaryStatistics = await GetSalaryStatisticAsync(ct);
            var upcomingInterviews = await GetUpcomingInterviewAsync(ct);

            var overviewKpiCalculations = CalculateOverviewKpi(statusCounts);

            return Result.Success(new GetDashboardSummaryResponse(
                overviewKpiCalculations,
                salaryStatistics,
                upcomingInterviews));
        }

        private async Task<Dictionary<JobApplicationStatus, int>> GetStatusCountsAsync(CancellationToken ct)
        {
            return await context.JobApplications
                .AsNoTracking()
                .GroupBy(a => a.Status)
                .Select(a => new
                {
                    Status = a.Key,
                    Count = a.Count()
                })
                .ToDictionaryAsync(x => x.Status, x => x.Count, ct);
        }

        private async Task<SalaryStatisticsDto> GetSalaryStatisticAsync(CancellationToken ct)
        {
            var salaryStats = await context.JobApplications
                .AsNoTracking()
                .Where(a => a.SalaryMin != null && a.SalaryMax != null)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Min = g.Min(a => a.SalaryMin),
                    Max = g.Max(a => a.SalaryMax),
                    Avg = g.Average(a => (a.SalaryMin + a.SalaryMax) / 2)
                })
                .FirstOrDefaultAsync(ct);

            return salaryStats == null 
                ? new SalaryStatisticsDto(null, null, null)
                : new SalaryStatisticsDto(salaryStats.Min, salaryStats.Max, salaryStats.Avg);
        }

        private async Task<IReadOnlyList<UpcomingInterviewDto>> GetUpcomingInterviewAsync(CancellationToken ct)
        {
            var utcNow = timeProvider.GetUtcNow();

            return await context.Interviews
                .AsNoTracking()
                .Where(i => i.ScheduledAt <= utcNow)
                .OrderBy(i => i.ScheduledAt)
                .Take(5)
                .Select(i => new UpcomingInterviewDto(
                    i.JobApplicationId,
                    i.JobApplication.Title,
                    i.InterviewType,
                    i.ScheduledAt,
                    i.Location))
                .ToListAsync(ct);
        }

        private OverviewKpiDto CalculateOverviewKpi(Dictionary<JobApplicationStatus, int> counts)
        {
            var total = counts.Values.Sum();

            var totalAttempted = total - counts.GetValueOrDefault(JobApplicationStatus.Wishlist);

            var active = 
                counts.GetValueOrDefault(JobApplicationStatus.Applied) + 
                counts.GetValueOrDefault(JobApplicationStatus.Interviewing) + 
                counts.GetValueOrDefault(JobApplicationStatus.Screening);

            var applied = 
                counts.GetValueOrDefault(JobApplicationStatus.Applied);

            var ghosted =
                counts.GetValueOrDefault(JobApplicationStatus.Ghosted);

            var offers = 
                counts.GetValueOrDefault(JobApplicationStatus.Offer) + 
                counts.GetValueOrDefault(JobApplicationStatus.Accepted);

            var rejected =
                counts.GetValueOrDefault(JobApplicationStatus.Rejected);

            var withInterviews =
                counts.GetValueOrDefault(JobApplicationStatus.Interviewing) +
                counts.GetValueOrDefault(JobApplicationStatus.Screening) +
                offers;

            return new OverviewKpiDto(
                total,
                active,
                applied,
                rejected,
                ghosted,
                offers,
                withInterviews,
                SafeRate(withInterviews, totalAttempted),
                SafeRate(offers, totalAttempted));
        }

        private static double? SafeRate(int numerator, int denominator) =>
            denominator == 0 ? null : Math.Round((double)numerator / denominator, 3);
    }
}
