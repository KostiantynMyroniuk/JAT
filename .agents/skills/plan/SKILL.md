---
name: plan
description: Break a .NET change into ordered, independently testable vertical slices with acceptance criteria and verify commands, and save it as a plan document for sign-off. Use this skill after the research skill, or whenever the user asks to plan, break down, split into tasks or slices, or make an implementation plan for a change in a .NET, ASP.NET Core or EF Core solution that spans more than one file. Never writes production code. Always stop for user approval before the build skill. Accepts an optional ticket ID.
---

# .NET Planning and Task Breakdown

Break the work into small slices that can each be built, tested and reviewed on their own. A good breakdown is the difference between reliable delivery and a tangled mess.

## When to Use

- After `research` produced a research document
- The work spans multiple files, projects or services
- Scope needs to be communicated clearly

**Skip for:** obvious single-step changes already clear from the request.

## Workflow

### Step 1: Read in read-only mode

Before writing the plan, read:

- `.agents/research/{id}-research.md` (ticket ID: from the user's message, else the branch name, else ask once)
- The affected files listed in the research, and the closest existing feature it identified
- `.agents/docs/` checklists for the relevant area, if the folder exists

Do NOT write any code in this step. If there is no research document and the change is not trivial, say so and offer to run `research` first.

### Step 2: Slice vertically

Prefer small complete paths over horizontal layers across the whole solution. When a feature has several independent behaviors (create, list, update), give each behavior its own slice that goes through the layers it needs. Shared schema goes first.

**Good (vertical):**
```
Slice 1: Schema — entity + EF configuration + generated migration (no behavior)
Slice 2: "Create" end to end — handler/service + DTO + validator + endpoint + tests
Slice 3: "List" end to end — query + DTO + endpoint + tests
Slice 4: External integration or sync (only if applicable)
```

**Bad (horizontal):**
```
Phase 1: All entities and DB changes across the solution
Phase 2: All services
Phase 3: All controllers
```

If one end-to-end path needs more than 5 production files, split it at a layer boundary (service first, endpoint second). Both halves must still compile and be testable.

### Step 3: Create the plan document

Create `.agents/plans/{id}-plan.md`:

```markdown
# {ID} Plan — {Short Title}

## Overview
{1–2 sentences: what and why}

## Out of Scope
- {explicit exclusions to prevent scope creep}

## Decisions
- {answers to the research document's Open Questions; link to `.agents/research/{id}-research.md`}

## Slices

### Slice 1: {Name}
**Files:** `src/.../File1.cs`, `src/.../File2.cs`
**What:** {description in prose — no code snippets}
**Tests:** {test project and class; unit or integration}
**Acceptance criteria:**
- [ ] {specific testable outcome}
- [ ] {specific testable outcome}
**Verify:** `dotnet test tests/Foo.Tests --filter "FullyQualifiedName~Bar"`

### Slice 2: {Name}
...

## Dependencies
- Slice 2 requires Slice 1 (schema must exist before the service uses it)
- Slice 3 requires Slice 2 (service must exist before the endpoint)

## Risks & Rollback
- {risk, and how to undo the slice — for migration slices, the reviewed Down migration}

## Files Modified
- `path/to/File.cs` — what changes and why
```

Useful **Verify** commands:

- Build and tests: `dotnet build`, `dotnet test <project> --filter "<expression>"`
- HTTP: `curl -s "https://localhost:{port}/api/..." -H "Authorization: Bearer {token}" | jq` (take `{port}` from `Properties/launchSettings.json`)
- Migrations: `dotnet ef migrations list` and `dotnet ef migrations script` to review the SQL without touching a database

### Step 4: Get sign-off

Show the plan to the user as a short summary of the slices plus anything risky. Ask for explicit approval. Apply requested changes to the document and ask again. Do not start `build` until the user says yes.

## Rules

- Each slice must leave the solution compiling (`dotnet build` green) with the existing tests still passing
- Max 1–5 production files per slice. Tests for the slice ship in the same slice and do not count toward the limit. If it needs more, split it
- **Migrations are always their own slice** and are never mixed with service, handler or endpoint code. The slice contains the entity/EF-configuration change that triggers the migration, and the migration is generated with `dotnet ef migrations add`, never written by hand
- Destructive or breaking schema changes (drop or rename a column, narrow a type, add NOT NULL to a populated table) are split into expand → backfill → contract across separate slices, and flagged under Risks
- Do not plan new NuGet packages or a framework upgrade unless the research approved it. If one is needed, give it its own slice and call it out for approval
- Each slice has testable acceptance criteria and a Verify command
- No time estimates
- No code snippets in the plan — prose and `file:line` references only
