---
name: research
description: Read-only investigation of a .NET, ASP.NET Core or EF Core codebase before any change is made. Produces a research document with current behavior, a code map with file and line references, conventions to follow, database and API impact, risks and open questions. Use this skill whenever the user brings a ticket, feature request, bug report or vague idea for a .NET solution and no approach has been agreed yet, or says research, investigate, spec this, how does X work here, or what would it take to. Run it before the plan skill even when the task looks simple. Accepts an optional ticket ID.
---

# .NET Research

Understand the problem and the existing code well enough that the plan can be written without guessing. The only output is a research document. No production code is written in this step.

## When to Use

- New feature, or a bug whose cause is not obvious
- The change spans several files or projects
- The change touches DB schema, public API contracts, auth, or an external integration

**Skip for:** one-line fixes where cause and fix are already obvious from the request.

## Ground Rules

- **Read-only.** Do not create, edit or delete anything except the research document.
- Allowed commands: `git log`, `git blame`, `git grep`, `dotnet --info`, `dotnet sln list`, `dotnet list package`, and a baseline `dotnet build` / `dotnet test` (they only write to `bin/`, `obj/`, `TestResults/`).
- Never run `dotnet ef database update` or anything else that connects to a database or an external service.
- Every statement about the code cites `path/to/File.cs:line`. If you could not verify something, mark it **Unverified** instead of presenting it as fact.
- Missing information goes under Open Questions. Do not fill gaps with assumptions.

## Workflow

### Step 0: Ticket ID

Take it from the user's message. If absent, try the branch name (`git branch --show-current`). If still unknown, ask once; if the user has none, use a short kebab-case slug. Below this is `{id}`.

### Step 1: Orient in the solution

Read what exists, in this order:

1. `*.sln` / `*.slnx` and every `*.csproj` — target framework, project references (their direction shows the layering), package references
2. `global.json`, `Directory.Build.props`, `Directory.Packages.props` (central package management), `.editorconfig`, nullable and warnings-as-errors settings
3. `Program.cs` (or `Startup.cs`) and the DI extension methods (`AddXxx`) — middleware order, authentication, service lifetimes
4. `appsettings*.json`, `Properties/launchSettings.json` (ports), Options classes
5. `.agents/docs/` checklists, `README.md`, `AGENTS.md` / `GEMINI.md` if present

Name the architecture style (Clean/Onion, N-tier, vertical slices, modular monolith) and the local choices: controllers vs minimal APIs, MediatR/handlers vs services, FluentValidation, AutoMapper/Mapster/manual mapping, Result pattern vs exceptions.

### Step 2: Trace the closest existing feature

Find the most similar feature that already exists and follow it end to end: route → validation → service/handler → repository or DbContext → entity and EF configuration → DTO/mapping → response. Record `file:line` at each hop. This is the pattern the new work must follow.

### Step 3: Investigate the specifics

For a **bug**: the exact code path, a minimal failing scenario, and a root-cause hypothesis backed by evidence (logs, `git blame`, an existing test).
For a **feature**: what already exists, what is missing, and where the new code plugs in.

In both cases check the areas that apply:

- **Data** — entities, `IEntityTypeConfiguration` classes, DbContext, latest migration and model snapshot, indexes, nullability, size of existing data
- **API contract** — routes, DTOs, status codes, versioning, authorization policies; is the change breaking for current clients?
- **Async and concurrency** — CancellationToken flow, transactions, `SaveChanges` boundaries, hosted/background services, queues
- **External calls** — typed `HttpClient`s, retries and resilience (Polly or `Microsoft.Extensions.Http.Resilience`), webhooks, message bus, secrets and config keys
- **Cross-cutting** — logging, exception handling / ProblemDetails, caching, feature flags
- **Tests** — which test projects exist, framework (xUnit/NUnit/MSTest), mocking library, integration style (`WebApplicationFactory`, Testcontainers), and the nearest existing tests to copy style from

### Step 4: Baseline

Run `dotnet build` and `dotnet test`. Record whether they are green and list any failures or warnings that already exist, so that later problems are not blamed on the change.

### Step 5: Write the research document

Create `.agents/research/{id}-research.md`:

```markdown
# {ID} Research — {Short Title}

## Goal
{1–3 sentences: the problem and the desired outcome}

## Current Behavior
{how it works today, with `file:line` references}

## Code Map
| Area | File:line | Notes |
|---|---|---|
| Entry point (route) | | |
| Validation | | |
| Service / handler | | |
| Data access | | |
| Entity + EF config | | |
| DTO / mapping | | |
| Tests | | |

## Conventions to Follow
- {pattern seen in the closest feature, with file:line}

## Impact Analysis
- **Database:** {tables/columns/indexes; migration needed; backward compatible; backfill}
- **API contract:** {new or changed endpoints; breaking or not; versioning}
- **Security:** {authn/authz, PII, secrets}
- **External integrations:** {…}
- **Performance / concurrency:** {…}

## Baseline
{build and test status before any change; known pre-existing failures}

## Options
| Option | Pros | Cons |
|---|---|---|

**Recommendation:** {one option and why}

## Risks & Unknowns
- {…}

## Open Questions
- {questions that need a human decision}
```

Show the user a short summary (not the whole file), point out the Open Questions, and wait for answers. Then suggest `plan`.

## Rules

- No solution code or pseudo-code in the document — prose and `file:line` references only
- No time estimates
- Keep it skimmable (about two pages); link to code instead of quoting it
- Do not slice the work into tasks here — that is `plan`
