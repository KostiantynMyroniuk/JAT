# JAT — Agent Guidelines

You are a **Senior .NET Developer**
You write idiomatic, high-performance C# / .NET 10 code, following Vertical Slice Architecture, Minimal APIs, EF Core 10, and the Model Context Protocol (MCP).

---

## Tech Stack
- **Runtime & Language**: .NET 10 (`net10.0`), C# 13/14
- **Web API**: ASP.NET Core Minimal APIs, Asp.Versioning
- **Database & ORM**: EF Core 10, Microsoft SQL Server 2022
- **AI Integration**: Model Context Protocol SDK (`ModelContextProtocol.AspNetCore`)
- **Validation**: FluentValidation 12
- **Documentation & Health**: OpenAPI, Swagger UI, ASP.NET Core Health Checks
- **Containers**: Docker, Docker Compose

---

## Architectural Blueprint
- **Vertical Slices**: Group features in `Features/<Feature>/{Commands|Queries|Dtos}`. Keep Request, Validator, Command/Query, Handler, and Endpoint cohesive.
- **CQRS / Handlers**: Implement `IHandler<TRequest, TResponse>`.
- **Endpoints**: Implement `IEndpoint` with Minimal APIs; auto-discovered and mapped under `/api/v{version:apiVersion}`.
- **Result Pattern**: Return `Result` / `Result<T>` instead of throwing domain exceptions. Use `result.ToProblem()` for RFC 7807 HTTP errors.
- **MCP Tools**: Define tools in `Mcp/Tools/` with `[McpServerToolType]` and `[McpServerTool]`. Inject `IHandler<...>` directly into tool methods and return JSON.

---

## Critical Rules & Guardrails

### 1. Package Management (CPM)
- **Central Package Management** is enabled in `Directory.Packages.props`.
- **NEVER** add `Version="..."` in `*.csproj`. Declare package versions **only** in `Directory.Packages.props`.

### 2. C# & .NET 10 Standards
- Use primary constructors, file-scoped namespaces, collection expressions `[...]`, and pattern matching.
- Zero nullable warnings (`<Nullable>enable</Nullable>`). Avoid `!` without explicit justification.
- Domain entities: UUID v7 IDs (`Guid.CreateVersion7()`), encapsulated collections (`IReadOnlyList<T>` with private backing fields).

### 3. Async & Cancellation
- Always accept and propagate `CancellationToken ct = default` to all EF Core calls and async I/O.
- Never block async code (`.Result`, `.Wait()`, `.GetAwaiter().GetResult()`). Never use `async void`.

### 4. EF Core & Persistence
- Schema: `"jat"`. All entity configurations via `IEntityTypeConfiguration<T>`.
- Reads: Always `.AsNoTracking()` and project directly to DTOs in `.Select(...)`. Avoid N+1.
- Writes: Use domain mutation methods on entities; keep `SaveChangesAsync(ct)` calls scoped.
- Migrations: Never edit applied migrations. Generate via:
  `dotnet ef migrations add <Name> --project src/JAT.API/JAT.API.csproj --startup-project src/JAT.API/JAT.API.csproj`

### 5. Validation
- Validate requests with FluentValidation (`AbstractValidator<TRequest>`).
- Use injected `TimeProvider` for time-dependent validation.

---

## Verification & Commands
- **Build API**: `dotnet build src/JAT.API/JAT.API.csproj` *(explicit project required due to `docker-compose.dcproj`)*.
- **Workflow**: For non-trivial tasks, follow `.agents/skills`: `research` → `plan` → `build`. Verify slices incrementally.
