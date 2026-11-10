using JAT.API.Common.Abstractions;
using JAT.API.Common.Models;
using JAT.API.Extensions;

namespace JAT.API.Features.Analytics.Queries.GetDashboardSummary
{
    public class GetDashboardSummaryEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/analytics/dashboard", HandleAsync)
                .WithName("GetDashboardSummary")
                .WithSummary("Get dashboard for job applications statistic")
                .WithDescription("Returns totals about applications statuses, salaries and interviews")
                .WithTags("Analytics")
                .Produces<GetDashboardSummaryResponse>(StatusCodes.Status200OK);
        }

        private static async Task<IResult> HandleAsync(
            IHandler<GetDashboardSummaryQuery, Result<GetDashboardSummaryResponse>> handler,
            CancellationToken ct)
        {
            var query = new GetDashboardSummaryQuery();
            var result = await handler.HandleAsync(query, ct);

            if (result.IsFailure)
                return result.ToProblem();

            return TypedResults.Ok(result.Value);
        }
    }
}
