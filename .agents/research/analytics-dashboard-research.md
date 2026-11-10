# analytics-dashboard Research — Analytics & Dashboard

## Goal
Design and implement an Analytics and Dashboard feature for the Job Application Tracker (JAT) that provides job applicants with comprehensive insights into their job search progress, recruitment funnel conversion, interview timeline, and salary metrics. The solution must follow Vertical Slice Architecture, Minimal APIs, EF Core 10, Result pattern, and MCP server tools.

## Current Behavior
The solution currently tracks domain entities ([JobApplication.cs](file:///D:/WorkProjects/JAT/src/JAT.API/Domain/Entities/JobApplication.cs), [Interview.cs](file:///D:/WorkProjects/JAT/src/JAT.API/Domain/Entities/Interview.cs), [Contact.cs](file:///D:/WorkProjects/JAT/src/JAT.API/Domain/Entities/Contact.cs), [Tag.cs](file:///D:/WorkProjects/JAT/src/JAT.API/Domain/Entities/Tag.cs)), but has no aggregation endpoints, reporting queries, or analytics tools:
- [JobApplications](file:///D:/WorkProjects/JAT/src/JAT.API/Features/JobApplications/): supports CRUD, status updates, tag associations, and paginated listing by status.
- [Interviews](file:///D:/WorkProjects/JAT/src/JAT.API/Features/Interviews/): supports CRUD and paginated listing by `jobApplicationId`.
- [Tags](file:///D:/WorkProjects/JAT/src/JAT.API/Features/Tags/): supports creation, deletion, and paginated listing.
- [Contacts](file:///D:/WorkProjects/JAT/src/JAT.API/Features/Contacts/): supports CRUD and paginated listing.
- Schema: table `jat.JobApplications` has an index on `Status` ([JobApplicationConfiguration.cs:36](file:///D:/WorkProjects/JAT/src/JAT.API/Infrastructure/Persistence/Configurations/JobApplicationConfiguration.cs#L36)).

## Code Map
Closest reference feature is `JobApplications` queries ([Features/JobApplications/Queries](file:///D:/WorkProjects/JAT/src/JAT.API/Features/JobApplications/Queries/)):

| Area | File:line | Notes |
|---|---|---|
| Query pattern | [GetApplicationsPaginatedQuery.cs](file:///D:/WorkProjects/JAT/src/JAT.API/Features/JobApplications/Queries/GetApplicationsPaginated/GetApplicationsPaginatedQuery.cs#L12-L80) | `IHandler<TQuery, Result<TDto>>` with EF Core query |
| Minimal API endpoint | [GetApplicationsPaginatedEndpoint.cs](file:///D:/WorkProjects/JAT/src/JAT.API/Features/JobApplications/Queries/GetApplicationsPaginated/GetApplicationsPaginatedEndpoint.cs#L14-L41) | `IEndpoint` mapping `/applications` |
| MCP tool pattern | [JobApplicationTools.cs](file:///D:/WorkProjects/JAT/src/JAT.API/Mcp/Tools/JobApplicationTools.cs#L16-L47) | `[McpServerToolType]` delegating to handlers |
| TimeProvider registration | [Program.cs:11](file:///D:/WorkProjects/JAT/src/JAT.API/Program.cs#L11) | `builder.Services.AddSingleton(TimeProvider.System)` |
| Schema & index | [JobApplicationConfiguration.cs:36-39](file:///D:/WorkProjects/JAT/src/JAT.API/Infrastructure/Persistence/Configurations/JobApplicationConfiguration.cs#L36-L39) | `builder.HasIndex(a => a.Status);` |

## Conventions to Follow
- Follow Vertical Slice Architecture: place feature in `src/JAT.API/Features/Analytics/{Queries|Dtos}`.
- Handlers implement `IHandler<TRequest, Result<TResponse>>`.
- Endpoints implement `IEndpoint` and register Minimal APIs under `/api/v{version:apiVersion}/analytics/...`.
- Database access: EF Core `GroupBy` and SQL aggregations via `.AsNoTracking()` directly projected to DTOs without loading full entity graphs into memory.
- Time management: inject `TimeProvider` for current timestamp calculations (e.g., upcoming interviews, date filtering).
- MCP Tools: implement `AnalyticsTools` with `[McpServerToolType]` in `src/JAT.API/Mcp/Tools/AnalyticsTools.cs`.
- Return RFC 7807 problem details on failure (`result.ToProblem()`).

## Impact Analysis
- **Database:** No schema modifications or migrations required. Aggregation queries run on existing tables (`jat.JobApplications`, `jat.Interviews`, `jat.Tags`). Optional future migration for index on `jat.Interviews(ScheduledAt)` if interview dataset grows large, but not required for current needs.
- **API contract:**
  - `GET /api/v1/analytics/dashboard` -> 200 OK with `DashboardAnalyticsDto` (overview KPIs, status distribution, recruitment funnel, interview summary, salary stats, upcoming interviews).
  - Optional `GET /api/v1/analytics/timeline?from={date}&to={date}` -> 200 OK with time-series application counts.
- **Security:** Public API conforming to existing endpoints (no authn/authz currently configured).
- **External integrations:** Expose MCP tool `get_analytics_dashboard` for AI assistants.
- **Performance / concurrency:** High-performance database-side aggregations using SQL `COUNT`, `GROUP BY`, `MIN`, `MAX`, `AVG`.

## Baseline
- `dotnet build src/JAT.API/JAT.API.csproj`: Green (0 errors, 0 warnings).
- `dotnet test tests/JAT.UnitTests/JAT.UnitTests.csproj`: Green.

## Options
| Option | Pros | Cons |
|---|---|---|
| Option 1: Monolithic Dashboard Endpoint (`GET /api/v1/analytics/dashboard`) | 1 network request hydrates entire dashboard; cohesive DTO; fastest development; ideal for mobile/web dashboards and MCP tools | Less flexible if UI wants to reload only a single widget |
| Option 2: Fine-grained Endpoints (`/overview`, `/funnel`, `/interviews`, `/salaries`) | Independent widget updates; can query only what is visible | Multiple network round-trips; more endpoints and boilerplate |
| Option 3: Primary Dashboard Endpoint + Specialized Timeline Endpoint (Recommended) | Single call for main dashboard views (`/dashboard`), plus separate endpoint for historical activity charts (`/timeline`) with date filtering | Optimal balance of ergonomics, performance, and flexibility |

**Recommendation:** Option 3. Provide `GET /api/v1/analytics/dashboard` for immediate holistic dashboard hydration + MCP tool integration, and `GET /api/v1/analytics/timeline` for historical graphing.

## Domain Metrics Definition
1. **Overview KPIs**:
   - `TotalApplications`: count of all applications
   - `ActiveApplications`: count where Status is `Applied`, `Screening`, or `Interviewing`
   - `TotalInterviews`: count of all interviews
   - `OffersCount`: count where Status is `Offer` or `Accepted`
   - `RejectionsCount`: count where Status is `Rejected`
   - `GhostedCount`: count where Status is `Ghosted`
   - `InterviewRate`: percentage of applications reaching at least screening/interview stage
   - `OfferRate`: percentage of applications resulting in an offer/accepted status
2. **Recruitment Funnel**:
   - Progression stages: `Wishlist` -> `Applied` -> `Screening` -> `Interviewing` -> `Offer` -> `Accepted`
   - Stage counts and drop-off / conversion percentages between consecutive stages.
3. **Status Distribution**:
   - Count and percentage for each `JobApplicationStatus` enum value.
4. **Interview Insights**:
   - Count by `InterviewType` (`HrScreening`, `Technical`, `TestTask`, `Final`, `Other`)
   - Next upcoming interviews (title, company/contact, scheduled time, interview type, location)
5. **Salary Statistics**:
   - Minimum, maximum, and average expected salary across applications with salary info.
6. **Top Tags / Skills**:
   - Most frequent tags with application counts.

## Risks & Unknowns
- Are there UI/frontend requirements (e.g. React/Blazor dashboard) or is the scope purely backend REST Minimal API + MCP tools?
- Should ghosted / rejected applications be excluded from active salary statistics? (Recommended: calculate both overall and active).
- Should timeline support grouping by day, week, or month?

## Open Questions
- 1. Чи потрібен окремий фронтенд (наприклад, Blazor або зовнішній SPA/React), чи наразі реалізуємо виключно REST API (Minimal APIs + Swagger) та MCP Tools для AI-асистента?
- 2. Чи погоджуєтеся ви з форматом Option 3: головний ендпоінт `GET /api/v1/analytics/dashboard` (зведені KPI, воронка, розподіл за статусами, зарплати, найближчі інтерв'ю) + додатковий `GET /api/v1/analytics/timeline` для графіків динаміки подачі за періодами?
- 3. Які саме метрики та графіки є для вас найбільш пріоритетними на першому етапі?
