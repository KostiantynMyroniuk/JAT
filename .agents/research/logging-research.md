# logging Research — Application-Wide Structured Logging

## Goal
Establish a comprehensive, high-performance structured logging strategy across the JAT solution using .NET 10 built-in logging (`Microsoft.Extensions.Logging`), covering application lifecycle/database migrations, HTTP request diagnostics, global exception handling, cross-cutting CQRS handler execution, and domain mutation auditing.

## Current Behavior
Currently, almost the entire solution runs without logging:
- Application lifecycle & database migrations: [Program.cs:32-37](file:///D:/WorkProjects/JAT/src/JAT.API/Program.cs#L32-L37) executes `Database.MigrateAsync()` completely silently without any log indicating start, finish, or migration failure.
- Exception handling: [GlobalExceptionHandler.cs:18-20](file:///D:/WorkProjects/JAT/src/JAT.API/Infrastructure/ExceptionHandling/GlobalExceptionHandler.cs#L18-L20) contains the only logger in the codebase (`ILogger<GlobalExceptionHandler>`), but omits HTTP request method, path, and trace identifier.
- CQRS handlers: None of the handlers in `Features/` inject `ILogger` or record request execution, timing, validation failures, or business outcomes.
- HTTP requests: [appsettings.json:5](file:///D:/WorkProjects/JAT/src/JAT.API/appsettings.json#L5) sets `"Microsoft.AspNetCore": "Warning"`, suppressing default ASP.NET Core hosting diagnostics and request execution logs.
- MCP tools: [Mcp/Tools/](file:///D:/WorkProjects/JAT/src/JAT.API/Mcp/Tools/) invoke handlers without logging AI tool calls or results.

## Code Map
| Area | File:line | Notes |
|---|---|---|
| Lifecycle & Startup | [Program.cs:32-37](file:///D:/WorkProjects/JAT/src/JAT.API/Program.cs#L32-L37) | Database migration execution |
| Global Exception Handler | [GlobalExceptionHandler.cs:8-20](file:///D:/WorkProjects/JAT/src/JAT.API/Infrastructure/ExceptionHandling/GlobalExceptionHandler.cs#L8-L20) | Only logger currently in the codebase |
| Handler Registration | [HandlerExtensions.cs:9-26](file:///D:/WorkProjects/JAT/src/JAT.API/Extensions/HandlerExtensions.cs#L9-L26) | Scoped registration of `IHandler<,>` |
| Result & Error mapping | [ResultExtensions.cs:8-41](file:///D:/WorkProjects/JAT/src/JAT.API/Extensions/ResultExtensions.cs#L8-L41) | Converts `Result` to ProblemDetails |
| MCP Result mapping | [McpResultExtensions.cs:15-34](file:///D:/WorkProjects/JAT/src/JAT.API/Mcp/McpResultExtensions.cs#L15-L34) | Converts `Result` to `CallToolResult` |
| JobApplication Commands | [CreateApplicationCommand.cs:53-83](file:///D:/WorkProjects/JAT/src/JAT.API/Features/JobApplications/Commands/CreateApplication/CreateApplicationCommand.cs#L53-L83) | Handler without logging |
| Interview Commands | [CreateInterviewCommand.cs:30-65](file:///D:/WorkProjects/JAT/src/JAT.API/Features/Interviews/Commands/CreateInterview/CreateInterviewCommand.cs#L30-L65) | Handler without logging |
| Contact Commands | [CreateContactCommand.cs:44-98](file:///D:/WorkProjects/JAT/src/JAT.API/Features/Contacts/Commands/CreateContact/CreateContactCommand.cs#L44-L98) | Handler without logging |
| Tag Commands | [CreateTagCommand.cs:22-48](file:///D:/WorkProjects/JAT/src/JAT.API/Features/Tags/Commands/CreateTag/CreateTagCommand.cs#L22-L48) | Handler without logging |
| Unit Tests | [JAT.UnitTests.csproj](file:///D:/WorkProjects/JAT/tests/JAT.UnitTests/JAT.UnitTests.csproj#L1-L18) | Existing unit test project for behavior tests |

## Conventions to Follow
- Use idiomatic .NET 10 structured logging with semantic parameters (`{ParameterName}`), avoiding string concatenation or interpolation.
- Preserve zero-allocation or low-allocation principles.
- Use log levels consistently:
  - `Information`: Normal operational flows (migration applied, entity created/updated/deleted, handler completed successfully).
  - `Warning`: Domain or operational failure (validation failure, entity not found, conflict, handled 4xx client errors).
  - `Error`: Unhandled exceptions, infrastructure failures, database migration crashes.
- Cross-cutting concerns belong in decorators or middleware rather than duplicating boilerplate across every handler.
- Concrete command handlers should record specific domain audit details (entity IDs, key business states).

## Impact Analysis
- **Database:** No schema changes or migrations required.
- **API contract:** No breaking changes to existing REST endpoints or MCP tools. HTTP responses and JSON payloads remain identical.
- **Security:** Ensure logs do not leak sensitive information or raw secrets. Entity IDs, enum statuses, and titles are safe to log.
- **External integrations:** MCP tools automatically gain observability when invoking handlers.
- **Performance / concurrency:** Decorator adds minimal overhead (measuring stopwatch and logging two structured events per handler call).

## Baseline
- `dotnet build src/JAT.API/JAT.API.csproj`: Green (0 errors, 0 warnings).
- `dotnet test tests/JAT.UnitTests/JAT.UnitTests.csproj`: Green (0 tests defined).
- `tests/JAT.IntegrationTests`: Requires running Docker daemon for MS SQL Testcontainers.

## Options
| Option | Pros | Cons |
|---|---|---|
| Option 1: Manual logger injection into every single handler | Explicit control per handler | Massive boilerplate across 18+ handlers; easily forgotten in future handlers; does not measure execution duration uniformly |
| Option 2: Generic `LoggingHandlerDecorator<TRequest, TResponse>` only | Zero boilerplate in handlers; covers 100% of handlers (HTTP + MCP); captures execution latency and failures in one place | Does not log custom domain event details (e.g. "Job application 'Dev' created with ID {Id}") |
| Option 3: Two-Tier Logging (Cross-cutting `LoggingHandlerDecorator` + Domain Audit in Command Handlers + Lifecycle/Exception enrichment) | Best of both worlds: uniform timing & error reporting for all handlers, plus domain audit logs on state mutations, enriched HTTP/exception diagnostics, and zero package dependencies | Slightly more initial setup across slices |

**Recommendation:** Option 3 — implement the cross-cutting `LoggingHandlerDecorator` for uniform observability and timing across all commands and queries, combined with domain audit logging in state-modifying command handlers, lifecycle logging in `Program.cs`, and enriched contextual exception logging in `GlobalExceptionHandler.cs`.

## Risks & Unknowns
- Decorator DI resolution in standard Microsoft.Extensions.DependencyInjection: must cleanly resolve the concrete handler and wrap it with `LoggingHandlerDecorator<,>` without circular dependencies or extra reflection overhead at runtime.
- Log volume: ensure debug-level logs are not flooded into production console by configuring appropriate default levels in `appsettings.json`.

## Open Questions
- 1. Чи бажаєте ви додати ASP.NET Core `UseHttpLogging()` для логування заголовків та статус-кодів HTTP-запитів на рівні конвеєра ASP.NET Core, чи достатньо налаштувати категорію `"Microsoft.AspNetCore.Hosting.Diagnostics": "Information"`?
- 2. Чи потрібно логувати виклики читання (запити `Query`) окремими логами в кожному хендлері, чи для запитів достатньо уніфікованого логування в `LoggingHandlerDecorator`?
