---
name: build
description: Implement an approved plan for a .NET, ASP.NET Core or EF Core project one slice at a time, running dotnet build and dotnet test after every slice and ticking acceptance criteria only with evidence. Use this skill whenever the user says build, implement, execute the plan, start coding, or do slice N and a plan exists in .agents/plans. Stops and asks instead of expanding scope when something unexpected comes up. Requires an approved plan, so run the plan skill first if none exists. Accepts an optional ticket ID and an optional slice number.
---

# .NET Build

Implement the approved plan one slice at a time. The plan is the contract: do what it says, verify it, and stop when reality disagrees with it.

## Preconditions

1. `.agents/plans/{id}-plan.md` exists and the user approved it (ticket ID: from the user's message, else the branch name, else ask once). If not, stop and suggest `plan`. The only exception is a trivial single-step change the user has already fully described.
2. Run `git status`. If there are unrelated uncommitted changes, tell the user before starting and never revert them.
3. Read the whole plan, and the "Conventions to Follow" and "Baseline" sections of the research document.
4. If the user named a slice, do only that one, after checking that the slices it depends on are done.

## Per-Slice Loop

1. **Re-read the slice** — Files, What, Tests, Acceptance criteria, Verify. Open each listed file and its nearest sibling so the new code matches local style.
2. **Tests first when behavior is testable.** Write or update tests that encode the acceptance criteria, and see them fail for the expected reason before implementing.
3. **Implement the minimum** that satisfies the slice. Touch only the files the slice lists.
4. **`dotnet build`** — 0 errors and no new warnings.
5. **`dotnet test`** — first with `--filter` for this slice's tests, then the whole test project(s) covering the touched code. Run the full solution suite after the last slice.
6. **Run the slice's Verify command.** For HTTP checks start the app (`dotnet run --project <Api> --launch-profile <profile>`) in the background and stop it afterwards.
7. **Tick the acceptance criteria** in the plan (`- [x]`) only when you have evidence (test output, a real response). Unverified means unticked.
8. **Report in 2–3 lines**: what changed, the evidence, any deviation. Then continue to the next slice, unless the user asked for one slice at a time.

## .NET Guardrails

**Code**
- Conventions in the research document win over personal preference. Match naming, folder layout, namespace style and mapping approach already used
- Async end to end. Accept and pass `CancellationToken`. Never `.Result`, `.Wait()` or `.GetAwaiter().GetResult()`; no `async void` outside event handlers
- Constructor injection only, no service locator. Register services where their siblings are registered. Never inject a Scoped service into a Singleton; use `IServiceScopeFactory` in hosted services
- Respect nullable reference types. No `!` suppression without a comment saying why
- Never expose EF entities from the API — map to DTOs. Validate at the boundary with the validation approach already in the project. Return errors through the existing mechanism (ProblemDetails or exception middleware) and never leak exception details
- Log with `ILogger<T>` and message templates, not string interpolation. No secrets, tokens or personal data in logs
- No secrets or connection strings in code, committed `appsettings*.json` or tests. Use User Secrets or environment variables and the Options pattern
- Outgoing HTTP only through `IHttpClientFactory` or typed clients, with the resilience setup the project already uses. Never `new HttpClient()` per request
- Do not add NuGet packages, change the target framework, or edit `Directory.*.props` or `global.json` unless the plan says so. With central package management, versions go in `Directory.Packages.props`

**EF Core and database**
- Schema changes only through `dotnet ef migrations add <Name> --project <Infra> --startup-project <Api>`. Never hand-write a migration or edit one that is already committed or applied. Review the generated `Up`, `Down` and snapshot diff, and review the SQL with `dotnet ef migrations script`
- Apply migrations only to a local development database, after checking the connection string. Never run `dotnet ef database update`, `database drop` or destructive SQL against a shared, staging or production database
- Read queries: `AsNoTracking()`, project to DTOs with `Select`, avoid N+1 (`Include` or projection), filter in the database before `ToListAsync`, never call `SaveChangesAsync` inside a loop

**Tests**
- Never delete, skip (`Skip = ...`) or weaken an existing test to get green. If a test is wrong, explain why and ask
- Use the framework and mocking library the repo already uses. Test endpoints through `WebApplicationFactory` if the repo does that elsewhere

**Hygiene**
- If `.editorconfig` exists, run `dotnet format --verify-no-changes --include <changed files>` and fix only your own files
- No commented-out code, no TODO without a ticket, no unrelated refactoring

## When to Stop and Ask

Stop instead of improvising when:

- The slice needs a file that is not listed in it, or would exceed 5 production files
- A plan assumption is wrong (the entity does not exist, DI is done differently, and so on)
- The same build or test failure remains after 3 targeted fix attempts. Report what you tried and your diagnosis
- The fix needs a breaking API change, a destructive migration, a new package, or touching secrets, infrastructure or real data

Describe what you found, propose an amendment to the plan, and wait. Update the plan document after the user agrees.

## Finish

After the last slice:

1. Run the full `dotnet build` and `dotnet test` and compare with the Baseline in the research document
2. Compare `git diff --stat` with the plan's "Files Modified" and mention any extra file
3. Give a final summary: slices done with evidence, deviations, follow-ups, and manual steps (applying the migration in each environment, config keys to set)
4. Do not commit or push unless asked. If asked, make one commit per slice with the message `{ID}: slice N — name`
