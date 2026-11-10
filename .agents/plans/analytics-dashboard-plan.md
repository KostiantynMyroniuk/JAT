# analytics-dashboard Plan — Analytics & Dashboard

## Overview
Implement a comprehensive Analytics and Dashboard feature for the Job Application Tracker (JAT) adhering to Vertical Slice Architecture, Minimal APIs, CQRS with `IHandler<TRequest, TResponse>`, EF Core 10 SQL aggregations, TimeProvider time abstractions, and Model Context Protocol (MCP) server tools.

## Out of Scope
- Frontend UI implementation (JAT is currently an API-first application; web frontend / dashboard UI is deferred to a separate client project or dedicated frontend task).
- Database migrations (existing schemas for `jat.JobApplications`, `jat.Interviews`, `jat.Tags` fully support all analytics queries).
- Real-time WebSockets / SignalR push (analytics are pulled on demand via HTTP GET and MCP tools).

## Decisions
- Based on research in [.agents/research/analytics-dashboard-research.md](file:///D:/WorkProjects/JAT/.agents/research/analytics-dashboard-research.md):
  1. Follow Option 3: Primary composite endpoint `GET /api/v1/analytics/dashboard` for single-request dashboard hydration + separate `GET /api/v1/analytics/timeline` for historical graphing with time grouping (Day, Week, Month).
  2. Implement database-level EF Core aggregations using `IQueryable` (.GroupBy, .Count, conditional Sums) and `.AsNoTracking()` to avoid loading full entity graphs into memory.
  3. Use injected `TimeProvider` for all relative timestamp operations (e.g., determining upcoming interviews where `ScheduledAt >= now`).
  4. Expose analytics to AI assistants via `AnalyticsTools` in `Mcp/Tools/AnalyticsTools.cs`.

## Slices

### Slice 1: Composite Dashboard Analytics (Overview, Funnel, Statuses, Salaries, Upcoming)
**Files:**
- `src/JAT.API/Features/Analytics/Dtos/DashboardAnalyticsDto.cs`
- `src/JAT.API/Features/Analytics/Queries/GetDashboardAnalytics/GetDashboardAnalyticsQuery.cs`
- `src/JAT.API/Features/Analytics/Queries/GetDashboardAnalytics/GetDashboardAnalyticsEndpoint.cs`
- `tests/JAT.UnitTests/Features/Analytics/GetDashboardAnalyticsTests.cs`

**What:**
Create DTOs in `DashboardAnalyticsDto.cs` modeling overview KPIs (`TotalApplications`, `ActiveApplications`, `TotalInterviews`, `OffersCount`, `RejectionsCount`, `GhostedCount`, `InterviewRate`, `OfferRate`), status breakdown (`StatusCountDto`), funnel stages (`FunnelStageDto`), salary statistics (`SalaryStatisticsDto`), and upcoming interviews (`UpcomingInterviewDto`).
Create `GetDashboardAnalyticsQuery` and `GetDashboardAnalyticsHandler` implementing `IHandler<GetDashboardAnalyticsQuery, Result<DashboardAnalyticsDto>>`. The handler executes optimized EF Core queries against `context.JobApplications` and `context.Interviews` using `.AsNoTracking()`.
Create `GetDashboardAnalyticsEndpoint` implementing `IEndpoint` mapping `GET /analytics/dashboard`.
Add unit tests verifying metric calculations, percentage edge cases (e.g. division by zero when 0 applications), and upcoming interview filtering.

**Tests:**
`tests/JAT.UnitTests/Features/Analytics/GetDashboardAnalyticsTests.cs`

**Acceptance criteria:**
- [ ] `GetDashboardAnalyticsHandler` calculates all overview KPIs correctly without throwing on empty database (safe division by zero).
- [ ] Status breakdown contains counts and percentages for all application statuses present.
- [ ] Recruitment funnel tracks progression across stages (Wishlist, Applied, Screening, Interviewing, Offer, Accepted).
- [ ] Salary stats calculate average min/max and overall min/max for applications with salary defined.
- [ ] Upcoming interviews return up to 5 scheduled interviews strictly where `ScheduledAt >= timeProvider.GetUtcNow()`.
- [ ] `GET /api/v1/analytics/dashboard` returns 200 OK with `DashboardAnalyticsDto`.
- [ ] `dotnet build src/JAT.API/JAT.API.csproj` compiles with 0 errors and 0 warnings.

**Verify:**
`dotnet build src/JAT.API/JAT.API.csproj` and `dotnet test tests/JAT.UnitTests/JAT.UnitTests.csproj`

---

### Slice 2: Application Activity Timeline Query & Endpoint
**Files:**
- `src/JAT.API/Features/Analytics/Dtos/TimelineAnalyticsDto.cs`
- `src/JAT.API/Features/Analytics/Queries/GetApplicationsTimeline/GetApplicationsTimelineQuery.cs`
- `src/JAT.API/Features/Analytics/Queries/GetApplicationsTimeline/GetApplicationsTimelineEndpoint.cs`
- `tests/JAT.UnitTests/Features/Analytics/GetApplicationsTimelineTests.cs`

**What:**
Create `TimelineAnalyticsDto` and `TimelinePointDto` (PeriodStart, AppliedCount, OfferCount).
Create `GetApplicationsTimelineQuery(TimelineGroupBy GroupBy = TimelineGroupBy.Month, DateTimeOffset? From = null, DateTimeOffset? To = null)`.
Create `GetApplicationsTimelineValidator` validating that `From <= To` if both provided and `GroupBy` is a valid enum value (Day, Week, Month).
Create `GetApplicationsTimelineHandler` implementing `IHandler<GetApplicationsTimelineQuery, Result<TimelineAnalyticsDto>>` grouping applications by period using `AppliedAt ?? CreatedAt`.
Create `GetApplicationsTimelineEndpoint` implementing `IEndpoint` mapping `GET /analytics/timeline`.
Add unit/integration tests for filtering and period groupings.

**Tests:**
`tests/JAT.UnitTests/Features/Analytics/GetApplicationsTimelineTests.cs`

**Acceptance criteria:**
- [ ] `GetApplicationsTimelineValidator` enforces valid `GroupBy` enum and valid date boundaries (`From <= To`).
- [ ] Handler groups application counts by requested period (Day, Week, Month) filtered within the date range.
- [ ] `GET /api/v1/analytics/timeline` returns 200 OK with `TimelineAnalyticsDto`.
- [ ] `dotnet build src/JAT.API/JAT.API.csproj` compiles with 0 errors and 0 warnings.

**Verify:**
`dotnet build src/JAT.API/JAT.API.csproj` and `dotnet test tests/JAT.UnitTests/JAT.UnitTests.csproj`

---

### Slice 3: MCP Tools for Analytics
**Files:**
- `src/JAT.API/Mcp/Tools/AnalyticsTools.cs`

**What:**
Create `AnalyticsTools` class marked with `[McpServerToolType]` following the pattern established in `JobApplicationTools.cs` and `InterviewTools.cs`.
Expose AI tools:
- `get_analytics_dashboard`: retrieves complete job search KPI summary, recruitment funnel, status counts, salary metrics, and upcoming interviews.
- `get_analytics_timeline`: retrieves timeline activity points with optional `groupBy` (Day, Week, Month) and date boundaries `from` / `to`.
Inject `IHandler<...>` directly into tool methods and format output using `ToToolResult(...)`.

**Acceptance criteria:**
- [ ] `AnalyticsTools` defines `get_analytics_dashboard` and `get_analytics_timeline` with `[McpServerTool(ReadOnly = true)]` and descriptive parameter attributes.
- [ ] Tool methods delegate execution directly to the corresponding feature handlers and convert `Result` to `CallToolResult`.
- [ ] Solution compiles cleanly with zero warnings or errors.

**Verify:**
`dotnet build src/JAT.API/JAT.API.csproj`

---

## Dependencies
- Slice 1 establishes core DTOs and primary dashboard aggregation query.
- Slice 2 provides the granular timeline time-series query.
- Slice 3 depends on query handlers from Slice 1 and Slice 2 to expose MCP server tools.

## Risks & Rollback
- Risk: In-memory aggregation if EF Core fails to translate complex grouping; mitigated by performing structured EF Core database queries on indexed columns (`Status`) and projecting directly.
- Rollback: Delete files created in `src/JAT.API/Features/Analytics/`, `src/JAT.API/Mcp/Tools/AnalyticsTools.cs`, and `tests/JAT.UnitTests/Features/Analytics/`.

## Files Modified
- None (purely additive feature slice under `Features/Analytics` and `Mcp/Tools`).
