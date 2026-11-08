# interviews-crud Plan — Interviews CRUD

## Overview
Implement complete CRUD functionality for the `Interview` domain entity adhering to Vertical Slice Architecture, Minimal APIs, CQRS with `IHandler<TRequest, TResponse>`, FluentValidation, RFC 7807 ProblemDetails, Model Context Protocol (MCP) server tools, and integration tests with `WebApplicationFactory` & Testcontainers.

## Out of Scope
- Adding interview lists directly into `JobApplicationDto` or modifying `GetApplicationEndpoint` (deferred by user decision).
- Modifying database migrations (database schema already has the `jat.Interviews` table with foreign keys and cascade delete).
- Authentication or authorization restrictions on interview endpoints.

## Decisions
- Based on research in [.agents/research/interviews-crud-research.md](file:///D:/WorkProjects/JAT/.agents/research/interviews-crud-research.md) and user input:
  1. `ScheduledAt` allows past timestamps to support logging past interviews; validator checks `NotEmpty()` / non-default without enforcing future dates.
  2. Implement MCP tools in `src/JAT.API/Mcp/Tools/InterviewTools.cs` matching `JobApplicationTools` and `TagTools`.
  3. `JobApplicationDto` remains unchanged for now.
  4. Use Integration Tests with `CustomWebApplicationFactory` and Testcontainers in `tests/JAT.IntegrationTests` instead of unit tests.

## Slices

### Slice 1: DTO and Create Interview Vertical Slice
**Files:**
- `src/JAT.API/Domain/Entities/Interview.cs`
- `src/JAT.API/Features/Interviews/Dtos/InterviewDto.cs`
- `src/JAT.API/Features/Interviews/Commands/CreateInterview/CreateInterviewCommand.cs`
- `src/JAT.API/Features/Interviews/Commands/CreateInterview/CreateInterviewEndpoint.cs`
- `tests/JAT.IntegrationTests/Features/Interviews/Commands/CreateInterviewTests.cs`

**What:**
Add an `Update` domain mutation method to `Interview.cs` to encapsulate property changes. Create `InterviewDto` representing interview data. Create `CreateInterviewCommand`, `CreateInterviewValidator`, and `CreateInterviewHandler` implementing `IHandler<CreateInterviewCommand, Result<Guid>>`. The handler validates the command, verifies the target `JobApplication` exists in the database, instantiates the entity, saves changes, and returns the generated UUID v7 identifier. Create `CreateInterviewEndpoint` implementing `IEndpoint` mapping `POST /interviews` to return HTTP 201 Created with route reference to `GetInterview`. Create integration tests for valid interview creation, 404 when JobApplication does not exist, and 400 when validation fails.

**Tests:**
`tests/JAT.IntegrationTests/Features/Interviews/Commands/CreateInterviewTests.cs`

**Acceptance criteria:**
- [x] `CreateInterviewValidator` enforces `JobApplicationId` is not empty, `ScheduledAt` is not default, `InterviewType` is a valid enum value, `Location` max length 200, and `Notes` max length 500.
- [x] `CreateInterviewHandler` returns `Error.NotFound` if the specified `JobApplicationId` does not exist in `JobApplications`.
- [x] Valid request successfully persists an `Interview` and returns `Result.Success(interview.Id)`.
- [x] `POST /interviews` returns HTTP 201 Created with `Location` header pointing to `GetInterview`.
- [x] API project and integration tests compile cleanly.

**Verify:**
`dotnet build src/JAT.API/JAT.API.csproj` and `dotnet build tests/JAT.IntegrationTests/JAT.IntegrationTests.csproj`

---

### Slice 2: Read Interviews Vertical Slice (Single & Paginated)
**Files:**
- `src/JAT.API/Features/Interviews/Queries/GetInterview/GetInterviewQuery.cs`
- `src/JAT.API/Features/Interviews/Queries/GetInterview/GetInterviewEndpoint.cs`
- `src/JAT.API/Features/Interviews/Queries/GetInterviewsPaginated/GetInterviewsPaginatedQuery.cs`
- `src/JAT.API/Features/Interviews/Queries/GetInterviewsPaginated/GetInterviewsPaginatedEndpoint.cs`
- `tests/JAT.IntegrationTests/Features/Interviews/Queries/GetInterviewTests.cs`

**What:**
Create `GetInterviewQuery` and `GetInterviewHandler` implementing `IHandler<GetInterviewQuery, Result<InterviewDto>>`, projecting directly from `context.Interviews.AsNoTracking()` to `InterviewDto`, returning `Error.NotFound` if not found. Create `GetInterviewEndpoint` implementing `IEndpoint` mapping `GET /interviews/{interviewId:guid}` returning HTTP 200 OK or 404 NotFound. Create `GetInterviewsPaginatedQuery`, `GetInterviewsPaginatedValidator`, and `GetInterviewsPaginatedHandler` implementing `IHandler<GetInterviewsPaginatedQuery, Result<PaginatedList<InterviewDto>>>`. Supports optional filtering by `jobApplicationId`, validates `PageNumber` (1..100,000) and `PageSize` (1..100), orders interviews by `ScheduledAt` descending, and returns paginated results. Create `GetInterviewsPaginatedEndpoint` mapping `GET /interviews`. Add integration tests for retrieving an existing interview by ID, 404 for missing ID, and paginated listing with filtering.

**Tests:**
`tests/JAT.IntegrationTests/Features/Interviews/Queries/GetInterviewTests.cs`

**Acceptance criteria:**
- [x] `GetInterviewHandler` returns `InterviewDto` when found, or `Error.NotFound` when given an unknown ID.
- [x] `GetInterviewsPaginatedValidator` validates page numbers and page size bounds.
- [x] `GetInterviewsPaginatedHandler` correctly filters by `JobApplicationId` when supplied and orders by `ScheduledAt` descending.
- [x] `GET /interviews/{interviewId:guid}` returns 200 OK or 404 NotFound ProblemDetails.
- [x] `GET /interviews` returns 200 OK with `PaginatedList<InterviewDto>`.

**Verify:**
`dotnet build src/JAT.API/JAT.API.csproj` and `dotnet build tests/JAT.IntegrationTests/JAT.IntegrationTests.csproj`

---

### Slice 3: Update & Delete Interview Vertical Slice
**Files:**
- `src/JAT.API/Features/Interviews/Commands/UpdateInterview/UpdateInterviewCommand.cs`
- `src/JAT.API/Features/Interviews/Commands/UpdateInterview/UpdateInterviewEndpoint.cs`
- `src/JAT.API/Features/Interviews/Commands/DeleteInterview/DeleteInterviewCommand.cs`
- `src/JAT.API/Features/Interviews/Commands/DeleteInterview/DeleteInterviewEndpoint.cs`
- `tests/JAT.IntegrationTests/Features/Interviews/Commands/UpdateInterviewTests.cs`
- `tests/JAT.IntegrationTests/Features/Interviews/Commands/DeleteInterviewTests.cs`

**What:**
Create `UpdateInterviewCommand`, `UpdateInterviewValidator`, and `UpdateInterviewHandler` implementing `IHandler<UpdateInterviewCommand, Result>`. The handler finds the interview by ID, validates the inputs, calls the domain `Update` method on `Interview`, saves changes, and returns `Result.Success()` or `Error.NotFound`. Create `UpdateInterviewEndpoint` mapping `PUT /interviews/{interviewId:guid}` returning HTTP 204 NoContent. Create `DeleteInterviewCommand` and `DeleteInterviewHandler` implementing `IHandler<DeleteInterviewCommand, Result>` using `ExecuteDeleteAsync(ct)` to delete the record, returning `Error.NotFound` if 0 rows were affected. Create `DeleteInterviewEndpoint` mapping `DELETE /interviews/{interviewId:guid}` returning HTTP 204 NoContent. Add integration tests for updating and deleting interviews.

**Tests:**
- `tests/JAT.IntegrationTests/Features/Interviews/Commands/UpdateInterviewTests.cs`
- `tests/JAT.IntegrationTests/Features/Interviews/Commands/DeleteInterviewTests.cs`

**Acceptance criteria:**
- [x] `UpdateInterviewValidator` validates `InterviewId`, `ScheduledAt`, `InterviewType`, `Location`, and `Notes`.
- [x] `UpdateInterviewHandler` returns `Error.NotFound` if the interview ID does not exist, and successfully updates all fields when found.
- [x] `PUT /interviews/{interviewId:guid}` returns HTTP 204 NoContent on success.
- [x] `DeleteInterviewHandler` deletes the record via `ExecuteDeleteAsync` and returns `Error.NotFound` if no row was deleted.
- [x] `DELETE /interviews/{interviewId:guid}` returns HTTP 204 NoContent on success or 404 NotFound if absent.

**Verify:**
`dotnet build src/JAT.API/JAT.API.csproj` and `dotnet build tests/JAT.IntegrationTests/JAT.IntegrationTests.csproj`

---

### Slice 4: MCP Tools for Interviews
**Files:**
- `src/JAT.API/Mcp/Tools/InterviewTools.cs`

**What:**
Create `InterviewTools` class marked with `[McpServerToolType]` following the pattern established in `JobApplicationTools.cs` and `TagTools.cs`. Expose AI tools:
- `get_interviews`: retrieves paginated interviews with optional `jobApplicationId`, `pageNumber`, and `pageSize`.
- `get_interview`: retrieves a single interview by its UUID.
- `create_interview`: creates an interview for a given application ID with type, scheduled time, optional location, and notes.
- `update_interview`: updates an interview's type, scheduled time, location, and notes.
- `delete_interview`: deletes an interview by ID.
Inject `IHandler<...>` directly into tool methods and format output using `ToToolResult(...)`.

**Acceptance criteria:**
- [x] `InterviewTools` defines `get_interviews`, `get_interview`, `create_interview`, `update_interview`, and `delete_interview` with `[McpServerTool]` attributes and descriptive tool parameter annotations.
- [x] Tool methods delegate execution directly to the corresponding feature handlers and convert `Result` to `CallToolResult`.
- [x] Solution compiles cleanly with zero warnings or errors.

**Verify:**
`dotnet build src/JAT.API/JAT.API.csproj`

---

## Dependencies
- Slice 1 introduces `InterviewDto` and `Interview.Update(...)`, which Slices 2, 3, and 4 depend upon.
- Slice 2 provides the `GetInterview` endpoint required for the `CreatedAtRoute` response in Slice 1.
- Slice 4 requires all command and query handlers from Slices 1, 2, and 3.

## Risks & Rollback
- Risk: Running integration tests requires Docker daemon for Testcontainers.
- Rollback: Revert files added in `src/JAT.API/Features/Interviews/`, `src/JAT.API/Mcp/Tools/InterviewTools.cs`, and `tests/JAT.IntegrationTests/Features/Interviews/`.

## Files Modified
- `src/JAT.API/Domain/Entities/Interview.cs` — add domain mutation method `Update(...)`
