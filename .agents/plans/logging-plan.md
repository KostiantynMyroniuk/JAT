# logging Plan — Application-Wide Structured Logging

## Overview
Implement a robust, production-grade structured logging architecture for the JAT solution using .NET 10 built-in logging (`Microsoft.Extensions.Logging`), combining cross-cutting CQRS pipeline timing and failure diagnostics via `LoggingHandlerDecorator`, domain audit logging in state-modifying command handlers, lifecycle and migration logging in `Program.cs`, enriched exception telemetry in `GlobalExceptionHandler.cs`, and MCP tool error diagnostics.

## Out of Scope
- Adding external third-party logging packages (e.g. Serilog, NLog) — .NET 10 built-in logging satisfies all requirements without external dependencies.
- Modifying database schemas or generating EF Core migrations (logging touches only code and configuration).
- Adding manual per-handler logging to read-only queries (`GetApplicationQuery`, `GetContactsPaginatedQuery`, etc.) — queries are fully and uniformly covered by `LoggingHandlerDecorator`.

## Decisions
- Based on research in [.agents/research/logging-research.md](file:///D:/WorkProjects/JAT/.agents/research/logging-research.md):
  1. Use a two-tier logging architecture:
     - Tier 1: Generic `LoggingHandlerDecorator<TRequest, TResponse>` for uniform timing, invocation logging, failure/validation warning logging, and unhandled exception logging across all commands and queries (serving both HTTP Minimal APIs and MCP tools).
     - Tier 2: Dedicated domain mutation audit logging in command handlers (`Create*`, `Update*`, `Delete*`, `SetTags`) to record entity IDs and business context upon state change.
  2. Enrich `GlobalExceptionHandler` with HTTP method, request path, and trace identifier to pinpoint unhandled errors in production.
  3. Log database migrations in `Program.cs` before execution and upon completion.
  4. Configure `Microsoft.AspNetCore.Hosting.Diagnostics: Information` in `appsettings.json` for HTTP request/response observability without framework noise.
  5. Provide unit tests for `LoggingHandlerDecorator` in `tests/JAT.UnitTests`.

## Slices

### Slice 1: Infrastructure, Startup, and Exception Logging
**Files:**
- `src/JAT.API/Program.cs`
- `src/JAT.API/Infrastructure/ExceptionHandling/GlobalExceptionHandler.cs`
- `src/JAT.API/appsettings.json`
- `src/JAT.API/appsettings.Development.json`

**What:**
In `Program.cs`, use the application logger (`app.Logger`) to log informational messages when database migrations begin, succeed, or an error log when they fail before rethrowing.
In `GlobalExceptionHandler.cs`, enrich the existing exception logging to include `{Method}`, `{Path}`, and `{TraceIdentifier}` from the `HttpContext` for both 500 unhandled errors and 4xx handled warnings.
In `appsettings.json` and `appsettings.Development.json`, add `"Microsoft.AspNetCore.Hosting.Diagnostics": "Information"` under `Logging:LogLevel` so that incoming HTTP requests, paths, response codes, and durations are visible in the console.

**Tests:**
Build verification and manual smoke test of application startup.

**Acceptance criteria:**
- [ ] Database migration start and completion are logged at `Information` level.
- [ ] `GlobalExceptionHandler` logs HTTP Method, Path, and TraceIdentifier on unhandled exceptions (`Error`) and handled exceptions (`Warning`).
- [ ] `appsettings.json` and `appsettings.Development.json` include `Microsoft.AspNetCore.Hosting.Diagnostics: Information`.
- [ ] `dotnet build src/JAT.API/JAT.API.csproj` succeeds with 0 warnings and 0 errors.

**Verify:**
`Push-Location src/JAT.API; dotnet build; Pop-Location`

---

### Slice 2: Cross-Cutting CQRS Pipeline Logging Decorator
**Files:**
- `src/JAT.API/Common/Behaviors/LoggingHandlerDecorator.cs`
- `src/JAT.API/Extensions/HandlerExtensions.cs`
- `tests/JAT.UnitTests/Common/Behaviors/LoggingHandlerDecoratorTests.cs`

**What:**
Create `LoggingHandlerDecorator<TRequest, TResponse>` implementing `IHandler<TRequest, TResponse>`. It wraps an inner handler and uses `ILogger<LoggingHandlerDecorator<TRequest, TResponse>>` and `Stopwatch` to log:
1. `Information` log upon invocation with `{RequestName}`.
2. `Information` log upon successful completion with `{RequestName}` and `{ElapsedMilliseconds}ms`.
3. `Warning` log when the returned response represents a failure (`Result.IsFailure`), recording `{RequestName}`, `{ElapsedMilliseconds}ms`, `{ErrorCode}`, `{ErrorType}`, `{ErrorDescription}`, and any `{ValidationErrors}`.
4. `Error` log when an unhandled exception is thrown inside the handler, recording `{RequestName}`, `{ElapsedMilliseconds}ms`, and rethrowing the exception.
In `HandlerExtensions.cs`, update handler registration to wrap each registered `IHandler<TRequest, TResponse>` with `LoggingHandlerDecorator<TRequest, TResponse>`.
In `tests/JAT.UnitTests`, add unit tests covering successful execution, failure result logging, and exception rethrowing with logging.

**Tests:**
`tests/JAT.UnitTests/Common/Behaviors/LoggingHandlerDecoratorTests.cs`

**Acceptance criteria:**
- [ ] `LoggingHandlerDecorator` logs start and successful completion with duration in milliseconds.
- [ ] When a handler returns `Result.Failure`, `LoggingHandlerDecorator` logs a warning with error details and duration.
- [ ] When a handler throws an exception, `LoggingHandlerDecorator` logs an error with duration and rethrows.
- [ ] Handlers resolved from DI are wrapped with `LoggingHandlerDecorator`.
- [ ] Unit tests for `LoggingHandlerDecorator` pass.

**Verify:**
`Push-Location src/JAT.API; dotnet build; Pop-Location` and `Push-Location tests/JAT.UnitTests; dotnet test; Pop-Location`

---

### Slice 3: Domain Mutation Audit Logging in JobApplications and Contacts
**Files:**
- `src/JAT.API/Features/JobApplications/Commands/CreateApplication/CreateApplicationCommand.cs`
- `src/JAT.API/Features/JobApplications/Commands/UpdateApplication/UpdateApplicationCommand.cs`
- `src/JAT.API/Features/JobApplications/Commands/DeleteApplication/DeleteApplicationCommand.cs`
- `src/JAT.API/Features/JobApplications/Commands/SetApplicationTags/SetApplicationTagsCommand.cs`
- `src/JAT.API/Features/Contacts/Commands/CreateContact/CreateContactCommand.cs`

**What:**
Inject `ILogger<T>` into command handlers to log explicit domain audit events upon successful state modifications:
1. `CreateApplicationHandler`: Logs `Information` when a job application is created with `{ApplicationId}`, `{Title}`, and `{Status}`.
2. `UpdateApplicationHandler`: Logs `Information` when a job application is updated with `{ApplicationId}`.
3. `DeleteApplicationHandler`: Logs `Information` when a job application is deleted with `{ApplicationId}`.
4. `SetApplicationTagsHandler`: Logs `Information` when tags are assigned with `{ApplicationId}` and `{TagCount}`.
5. `CreateContactHandler`: Logs `Information` when a contact is created with `{ContactId}` for `{JobApplicationId}`.

**Tests:**
Compilation and existing integration test verification.

**Acceptance criteria:**
- [ ] `CreateApplicationHandler` logs job application creation with ID, title, and status.
- [ ] `UpdateApplicationHandler` logs job application update with ID.
- [ ] `DeleteApplicationHandler` logs job application deletion with ID.
- [ ] `SetApplicationTagsHandler` logs tag assignment with application ID and tag count.
- [ ] `CreateContactHandler` logs contact creation with contact ID and application ID.
- [ ] API project compiles cleanly with 0 warnings.

**Verify:**
`Push-Location src/JAT.API; dotnet build; Pop-Location`

---

### Slice 4: Domain Mutation Audit Logging in Contacts and Interviews
**Files:**
- `src/JAT.API/Features/Contacts/Commands/UpdateContact/UpdateContactCommand.cs`
- `src/JAT.API/Features/Contacts/Commands/DeleteContact/DeleteContactCommand.cs`
- `src/JAT.API/Features/Interviews/Commands/CreateInterview/CreateInterviewCommand.cs`
- `src/JAT.API/Features/Interviews/Commands/UpdateInterview/UpdateInterviewCommand.cs`
- `src/JAT.API/Features/Interviews/Commands/DeleteInterview/DeleteInterviewCommand.cs`

**What:**
Inject `ILogger<T>` into command handlers to log explicit domain audit events upon successful state modifications:
1. `UpdateContactHandler`: Logs `Information` when contact is updated with `{ContactId}`.
2. `DeleteContactHandler`: Logs `Information` when contact is deleted with `{ContactId}`.
3. `CreateInterviewHandler`: Logs `Information` when an interview is created/scheduled with `{InterviewId}`, `{JobApplicationId}`, `{InterviewType}`, and `{ScheduledAt}`.
4. `UpdateInterviewHandler`: Logs `Information` when an interview is updated with `{InterviewId}`.
5. `DeleteInterviewHandler`: Logs `Information` when an interview is deleted with `{InterviewId}`.

**Tests:**
Compilation and existing integration test verification.

**Acceptance criteria:**
- [ ] `UpdateContactHandler` logs contact update with ID.
- [ ] `DeleteContactHandler` logs contact deletion with ID.
- [ ] `CreateInterviewHandler` logs interview creation with ID, application ID, interview type, and scheduled time.
- [ ] `UpdateInterviewHandler` logs interview update with ID.
- [ ] `DeleteInterviewHandler` logs interview deletion with ID.
- [ ] API project compiles cleanly with 0 warnings.

**Verify:**
`Push-Location src/JAT.API; dotnet build; Pop-Location`

---

### Slice 5: Domain Mutation Audit Logging in Tags & MCP Diagnostics
**Files:**
- `src/JAT.API/Features/Tags/Commands/CreateTag/CreateTagCommand.cs`
- `src/JAT.API/Features/Tags/Commands/DeleteTag/DeleteTagCommand.cs`
- `src/JAT.API/Mcp/McpResultExtensions.cs`

**What:**
Inject `ILogger<T>` into `CreateTagHandler` and `DeleteTagHandler` to log tag creation with `{TagId}` and `{Name}`, and tag deletion with `{TagId}`.
In `McpResultExtensions.cs`, add diagnostic logging for MCP tool error conversions so that AI tool execution failures are captured with error descriptions.

**Tests:**
Compilation and unit test execution.

**Acceptance criteria:**
- [ ] `CreateTagHandler` logs tag creation with ID and name.
- [ ] `DeleteTagHandler` logs tag deletion with ID.
- [ ] `McpResultExtensions` logs error conversions when an MCP tool fails.
- [ ] Entire solution compiles cleanly with 0 warnings and all tests pass.

**Verify:**
`Push-Location src/JAT.API; dotnet build; Pop-Location` and `Push-Location tests/JAT.UnitTests; dotnet test; Pop-Location`

## Dependencies
- Slice 2 depends on existing `IHandler<,>` infrastructure in `src/JAT.API/Common/Abstractions/IHandler.cs`.
- Slices 3, 4, and 5 can execute independently once Slice 2 is in place.

## Risks & Rollback
- Risk: Decorator registration might interfere with existing handler resolution.
  Rollback: If decorator causes resolution failure, revert `HandlerExtensions.cs` to direct `services.TryAddScoped(handlerInterface, type)`.
- Risk: High log volume in production.
  Rollback: Adjust `Logging:LogLevel:JAT.API` in `appsettings.json` to `Warning`.

## Files Modified
- `src/JAT.API/Program.cs` — add migration and startup logging
- `src/JAT.API/Infrastructure/ExceptionHandling/GlobalExceptionHandler.cs` — enrich exception logs with HTTP context
- `src/JAT.API/appsettings.json` & `appsettings.Development.json` — configure logging levels
- `src/JAT.API/Common/Behaviors/LoggingHandlerDecorator.cs` — cross-cutting CQRS logging decorator
- `src/JAT.API/Extensions/HandlerExtensions.cs` — register decorator for all handlers
- `tests/JAT.UnitTests/Common/Behaviors/LoggingHandlerDecoratorTests.cs` — unit tests for decorator
- `src/JAT.API/Features/JobApplications/Commands/...` — audit logging for JobApplications
- `src/JAT.API/Features/Contacts/Commands/...` — audit logging for Contacts
- `src/JAT.API/Features/Interviews/Commands/...` — audit logging for Interviews
- `src/JAT.API/Features/Tags/Commands/...` — audit logging for Tags
- `src/JAT.API/Mcp/McpResultExtensions.cs` — MCP error logging
