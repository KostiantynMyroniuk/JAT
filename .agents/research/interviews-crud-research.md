# interviews-crud Research — Interviews CRUD

## Goal
Implement full CRUD operations for the `Interview` domain entity following Vertical Slice Architecture, Minimal APIs, CQRS with `IHandler<TRequest, TResponse>`, FluentValidation, and the Result pattern.

## Current Behavior
The codebase already has the foundational domain and persistence definitions for interviews, but no HTTP endpoints, commands, queries, or DTOs exist yet:
- Domain entity: [Interview.cs](file:///D:/WorkProjects/JAT/src/JAT.API/Domain/Entities/Interview.cs#L5-L20) defines `Id`, `JobApplicationId`, `InterviewType`, `ScheduledAt`, `Location`, and `Notes`.
- Enum: [InterviewType.cs](file:///D:/WorkProjects/JAT/src/JAT.API/Domain/Enums/InterviewType.cs#L3-L6) defines `HrScreening`, `Technical`, `TestTask`, `Final`, and `Other`.
- EF Core entity configuration: [InterviewConfiguration.cs](file:///D:/WorkProjects/JAT/src/JAT.API/Infrastructure/Persistence/Configurations/InterviewConfiguration.cs#L7-L20) configures `Location` (max 200), `Notes` (max 500), and string conversion for `InterviewType`.
- Relationship: [JobApplicationConfiguration.cs](file:///D:/WorkProjects/JAT/src/JAT.API/Infrastructure/Persistence/Configurations/JobApplicationConfiguration.cs#L11-L15) configures `HasMany(a => a.Interviews).WithOne(i => i.JobApplication).HasForeignKey(i => i.JobApplicationId).OnDelete(DeleteBehavior.Cascade)`.
- DbContext: [ApplicationDbContext.cs](file:///D:/WorkProjects/JAT/src/JAT.API/Infrastructure/Persistence/ApplicationDbContext.cs#L9) exposes `DbSet<Interview> Interviews`.
- Initial migration: [20261031114739_InitialMigration.cs](file:///D:/WorkProjects/JAT/src/JAT.API/Infrastructure/Persistence/Migrations/20261031114739_InitialMigration.cs#L73-L95) already creates the `jat.Interviews` table with foreign key and cascade deletion.

## Code Map
Closest reference feature is `Contacts` ([Features/Contacts](file:///D:/WorkProjects/JAT/src/JAT.API/Features/Contacts/)):

| Area | File:line | Notes |
|---|---|---|
| Entry point (route) | [CreateContactEndpoint.cs](file:///D:/WorkProjects/JAT/src/JAT.API/Features/Contacts/Commands/CreateContact/CreateContactEndpoint.cs#L13-L50) | `IEndpoint` mapping Minimal API `/contacts` |
| Entry point (route) | [UpdateContactEndpoint.cs](file:///D:/WorkProjects/JAT/src/JAT.API/Features/Contacts/Commands/UpdateContact/UpdateContactEndpoint.cs#L12-L46) | Minimal API `PUT /contacts/{contactId:guid}` |
| Entry point (route) | [DeleteContactEndpoint.cs](file:///D:/WorkProjects/JAT/src/JAT.API/Features/Contacts/Commands/DeleteContact/DeleteContactEndpoint.cs#L11-L37) | Minimal API `DELETE /contacts/{contactId:guid}` |
| Entry point (route) | [GetContactEndpoint.cs](file:///D:/WorkProjects/JAT/src/JAT.API/Features/Contacts/Queries/GetContact/GetContactEndpoint.cs#L8-L35) | Minimal API `GET /contacts/{contactId:guid}` |
| Entry point (route) | [GetContactsPaginatedEndpoint.cs](file:///D:/WorkProjects/JAT/src/JAT.API/Features/Contacts/Queries/GetContactsPaginated/GetContactsPaginatedEndpoint.cs#L13-L39) | Minimal API `GET /contacts` with pagination and filter |
| Validation | [CreateContactCommand.cs](file:///D:/WorkProjects/JAT/src/JAT.API/Features/Contacts/Commands/CreateContact/CreateContactCommand.cs#L18-L42) | FluentValidation `AbstractValidator<T>` |
| Service / handler | [CreateContactCommand.cs](file:///D:/WorkProjects/JAT/src/JAT.API/Features/Contacts/Commands/CreateContact/CreateContactCommand.cs#L44-L98) | `IHandler<CreateContactCommand, Result<Guid>>` |
| Service / handler | [DeleteContactCommand.cs](file:///D:/WorkProjects/JAT/src/JAT.API/Features/Contacts/Commands/DeleteContact/DeleteContactCommand.cs#L10-L28) | Uses `ExecuteDeleteAsync` |
| Data access | [ApplicationDbContext.cs](file:///D:/WorkProjects/JAT/src/JAT.API/Infrastructure/Persistence/ApplicationDbContext.cs#L9) | `DbSet<Interview> Interviews` |
| Entity + EF config | [Interview.cs](file:///D:/WorkProjects/JAT/src/JAT.API/Domain/Entities/Interview.cs#L5-L20) / [InterviewConfiguration.cs](file:///D:/WorkProjects/JAT/src/JAT.API/Infrastructure/Persistence/Configurations/InterviewConfiguration.cs#L7-L20) | Existing entity & schema |
| DTO / mapping | [ContactDto.cs](file:///D:/WorkProjects/JAT/src/JAT.API/Features/Contacts/Dtos/ContactDto.cs#L3-L8) | Record DTO |
| Tests | [CreateApplicationTests.cs](file:///D:/WorkProjects/JAT/tests/JAT.IntegrationTests/Features/JobApplications/Commands/CreateApplicationTests.cs#L12-L70) | Integration tests with WebApplicationFactory & Testcontainers |

## Conventions to Follow
- Follow Vertical Slice Architecture: place everything in `src/JAT.API/Features/Interviews/{Commands|Queries|Dtos}`.
- Handlers implement `IHandler<TRequest, Result<TResponse>>` or `IHandler<TRequest, Result>`.
- Endpoints implement `IEndpoint` and register via Minimal APIs under `/api/v{version:apiVersion}/interviews`.
- Errors returned as RFC 7807 problem details via `result.ToProblem()`.
- Primary constructors and file-scoped namespaces throughout.
- Queries always `.AsNoTracking()` and directly projected using `.Select(...)`.
- Verify `JobApplication` exists before creating an interview for it (`Error.NotFound`).
- Optional domain mutation method `Update(...)` on `Interview` entity to encapsulate state changes.

## Impact Analysis
- **Database:** No database migrations or schema modifications are required; table `jat.Interviews` is already defined in EF Core configuration and generated in migration `20261031114739_InitialMigration`.
- **API contract:**
  - `POST /api/v1/interviews` -> 201 Created with `Location: /api/v1/interviews/{id}`
  - `GET /api/v1/interviews/{interviewId}` -> 200 OK (`InterviewDto`) or 404 Not Found
  - `GET /api/v1/interviews?jobApplicationId={guid}&pageNumber=1&pageSize=10` -> 200 OK (`PaginatedList<InterviewDto>`)
  - `PUT /api/v1/interviews/{interviewId}` -> 204 NoContent or 400/404
  - `DELETE /api/v1/interviews/{interviewId}` -> 204 NoContent or 404
- **Security:** No authentication/authorization configured in the solution at this stage.
- **External integrations:** None directly affected. Optional MCP tool registration in `Mcp/Tools/InterviewTools.cs`.
- **Performance / concurrency:** Paginated reads with max page size validation (100 items), `.AsNoTracking()`.

## Baseline
- `dotnet build src/JAT.API/JAT.API.csproj`: Green (0 errors, 0 warnings).
- `dotnet test tests/JAT.UnitTests/JAT.UnitTests.csproj`: Green (0 tests defined).
- `tests/JAT.IntegrationTests`: Requires running Docker daemon for MS SQL Testcontainers (Docker currently offline on local machine).

## Options
| Option | Pros | Cons |
|---|---|---|
| Option 1: REST Minimal API CRUD (`Features/Interviews/`) | Follows established vertical slices exactly (like `Contacts`); zero schema changes needed; clean and standard | Doesn't include MCP tools unless explicitly added |
| Option 2: REST Minimal API CRUD + MCP Tools (`InterviewTools.cs`) | Exposes full CRUD to both HTTP clients and MCP/AI agents; matches `JobApplications` & `Tags` pattern | Slightly more code to maintain |

**Recommendation:** Option 2 — implement the complete REST CRUD feature slices first, and include `InterviewTools` for AI assistant integration as specified in project blueprint guidelines.

## Risks & Unknowns
- Validation rules for `ScheduledAt`: should scheduled time allow past dates (e.g. logging previously completed interviews), or should it enforce future dates? In recruiting trackers, users often record past interviews after the fact.
- Multiple interviews per application: unlike `Contact` (1-to-1), `Interview` is 1-to-many with `JobApplication`, so no uniqueness conflict check is needed.

## Open Questions
- 1. Чи дозволяти створювати інтерв'ю з датою у минулому (наприклад, для логування вже проведених співбесід), чи дата обов'язково має бути в майбутньому?
- 2. Чи реалізовувати MCP tools (`InterviewTools.cs` у `Mcp/Tools/`) разом із REST ендпоінтами?
- 3. Чи потрібно оновлювати `JobApplicationDto` / `GetApplicationEndpoint`, щоб повертати список співбесід разом із заявкою, чи залишити це окремим ендпоінтом?
