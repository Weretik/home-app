# Todos — final backend code audit

- **Date:** 2026-11-26
- **Kind:** final feature audit of a new module (skill `architecture:backend-code-audit`)
- **Branch:** `claude/project-thread-7oue5f`, uncommitted working tree
- **Result:** no open blocker. 3 findings fixed, 9 accepted with a reason.

## Scope

- `api/src/Modules/Todos/**` (five projects, `Initial` migration), the `Host.Api` composition changes
  (`Program.cs`, `MediatorSetup.cs`, `Host.Api.csproj`), `HomeApp.slnx`.
- `api/tests/UnitTests/Todos/**`, `api/tests/IntegrationTests/Api/Todos/**`, the architecture tests as they apply to
  the module.
- `.github/workflows/api.yml` (the `todos` migrations bundle).
- Docs: `api/docs/modules/todos.md`, `api/docs/decisions/0030-todos-module-model.md`, `api/docs/architecture/c4.md`,
  `api/docs/architecture/README.md`, `api/README.md`, this SDD.
- The import script `home-app/import/todos-import.sql` in the project files (R-016, SC-008).
- Not in scope: `web/` (audited separately).

## Method

1. Read the SDD (README, requirements R-001…R-016 and SC-001…SC-009, design, tasks), `api/docs/rules.md`,
   `api/docs/architecture/README.md`, `AGENTS.md` boundaries, and the Expenses module as the reference.
2. Read every source file of the module line by line; compared layout, error handling, version header, idempotency
   and rate limiting with Expenses.
3. Went through the architect checklists in `obsidian/Architecture/Checklists/backend/checks`: T1 new feature
   (`new-feature.md`), T9 new module (`new-module.md`), T3 migration (`tasks/T03-migration.md`).
4. Ran the generated SQL of the migration (`dotnet ef migrations script --idempotent`) twice against a throwaway
   `postgres:17-alpine`, then the import script twice; ran the `pg_trgm` guard as a role without `CREATE` on the
   database.
5. Wrote a failing integration test for each behaviour finding before fixing it; then ran the build and all suites.

## Step 0: quality targets

The repository has no quality targets file (no `quality-targets.md` in `api/docs` or `docs`). The only numbers are the
SDD's non-functional table: hundreds of active and thousands of archived tasks, pages of at most 100, 60 writes a
minute per address. No P95/P99, availability, RPO/RTO or monthly cost target exists for the project, so this audit
gives no speed or scaling verdict beyond "reads page in SQL and are bounded". Recorded as AF-TD-004 (project-wide,
not caused by this module).

## Findings

| Id | Severity | Rule | Evidence | Status |
| --- | --- | --- | --- | --- |
| AF-TD-001 | High | R-010 «step changes do not need the version, so ticking steps quickly never conflicts»; ADR 0030 claimed «the web retries it» | Every step change writes the task row (`TodoTask.cs:156,170,181` set `UpdatedAtUtc`), so two step changes of one task sent together race on its `xmin`; the step handlers caught `DbUpdateConcurrencyException` and answered `409 todos.changed_meanwhile`. Reproduced: 8 parallel ticks of one task gave 7 × `409`. The web does not retry step `409`s (`web/src/features/todos/writes/failures-api.ts` only maps them to a message). | **Fixed.** `Todos.Application/Features/Steps/StepChanges.cs` reads the task again after a lost race and reapplies the change (at most 10 attempts; each loss means another change committed, so up to 10 simultaneous changes of one task all land). The three step handlers use it; `ITodosDbContext` exposes `ChangeTracker` to forget the stale read. Test `TodosStepsApiTests.Ticking_Steps_AtTheSameTime_NeverConflicts` (red before, green 4 runs in a row). ADR 0030, module page and design updated. |
| AF-TD-002 | Medium | Rules, Data: «reads … page in SQL (page size at most 100)»; no whole set loaded to filter or page in memory | `GetTodoSummaryHandler.cs` loaded every active overdue, due-today and urgent task (with steps and notes) and ordered and cut it to 8 in memory (`SummaryFocus.Pick`). An overdue backlog grows without bound. | **Fixed.** The query applies the `SummaryFocus` order in SQL and takes `SummaryFocus.MaxTasks` rows (`GetTodoSummaryHandler.cs:40-50`); `Pick` still sorts the ≤ 8 rows (unit tested). Test `TodosSummaryApiTests.The_Focus_HoldsAtMostEight_TheOldestOverdueFirst`. |
| AF-TD-003 | Medium | tasks.md TS-006 covers R-012 at integration level; R-012 «Archive order: completion time, newest first» | Only the active order had an integration test (`TodosTasksApiTests.The_Active_List_IsOrdered_…`); nothing checked the archive order. | **Fixed.** Test `TodosTasksApiTests.The_Archive_IsOrdered_ByCompletion_NewestFirst` (archived in an order that differs from creation, one of them `Cancelled`). |
| AF-TD-004 | High (project) | Audit step 0: a quality targets file with P95/P99, peak, availability, RPO/RTO, monthly cost | No such file in the repository. | **Accepted for this module**: project-wide gap that predates Todos; route to the owner as a separate task (proposed, to confirm: P95 < 300 ms for lists and summary, P95 < 500 ms for writes on the F1 plan after warm-up; RPO 24 h from the provider's daily backup; cost 0 extra per month). |
| AF-TD-005 | Low | R-006 «`Normal` is the default»; input validation | A body without `priority` binds to `TodoPriority.Urgent` (enum value 0) and without `assignee` to `Vitaliy` (`TodoTaskRequest.cs:13`); a status body without `status` becomes `Todo` (`TodoTaskStatusRequest.cs:6`). The OpenAPI document marks these fields required, so the typed web client always sends them. | **Accepted.** Platform-wide behaviour of record binding (Expenses `ExpenseRequest` does the same); the fix (`RespectRequiredConstructorParameters` in `ApiDocumentation.cs`) changes every module and the contract, so it belongs to a platform change, not this module. |
| AF-TD-006 | Low | Design: `notes?`, `status?`, `steps?`, `categoryId?` optional | `openapi.json` lists every `TodoTaskRequest` property as required (nullable ones as required-but-nullable). The server accepts them missing. | **Accepted.** Same generator behaviour as Expenses; changing it is a contract change for both modules and the web client. |
| AF-TD-007 | Low | Validators check shape | `CreateTodoTaskValidator.cs:12` (and update, step, category validators) check `MaximumLength` on the untrimmed text, so a 200-character title padded with spaces is refused although the domain would trim it to a valid one. | **Accepted.** Errs on the safe side, matches Expenses; the web trims before sending. |
| AF-TD-008 | Low | Rules, Data: indexes for filtered lists | `ix_tasks_status_due_date` (`TodoTaskConfiguration.cs:102`): the active list filters `status NOT IN ('Done','Cancelled')`, which a leading `status` column does not serve well; the priority rank is a `CASE`, so the active order is a sort. | **Accepted.** Hundreds of active rows; revisit with `EXPLAIN ANALYZE` if the list ever shows up slow. |
| AF-TD-009 | Low | DDD: root ids typed, not bare `Guid` | `TodoTask.Id`, `TodoCategory.Id`, `CategoryId` are `Guid`. | **Accepted.** One technique per repository: Expenses uses `Guid` ids too. |
| AF-TD-010 | Low | Idempotency on every create | Category and step creates take no `Idempotency-Key`. | **Accepted.** Recorded in ADR 0030: a repeated category hits the unique name (`409`), a duplicated step is visible and removed with one tap; no money, stock or outside message. |
| AF-TD-011 | Info | Concurrency | Deleting a category rewrites `category_id` of its tasks (`ON DELETE SET NULL`), which moves their `xmin`; an open task form then answers `409` once. | **Accepted.** Recorded in ADR 0030, Consequences. |
| AF-TD-012 | Info | Security checklist (permissions, owner checks, personal data) | Every endpoint is anonymous; task titles and notes are household data. | **Accepted.** ADR 0015 (open until Identity) covers the module; every write is rate limited. The logging behaviour logs only the message name and error code, never titles or notes. |

## Checked and in order

- **Layers and boundaries:** five projects; Domain references only `BuildingBlocks.Domain`; Application has no
  Infrastructure or Api reference; `Todos.Contracts` empty; no reference to Expenses; Host only calls
  `AddTodosModule` / `MapTodosModule` and adds the Application assembly to Mediator (architecture tests green).
- **Folders and files:** largest folders have 6 files (`Features/Tasks/Reading/`, the three migration files are
  exempt); no single-file or wrapper folder; one type per file, file named after the type.
- **Use cases:** every command and query with input has a validator (14 use cases, `ListTodoCategories` and
  `GetTodoSummary` take none); every handler returns `Result`; HTTP request records live in `Todos.Api` and map to
  commands.
- **One `SaveChanges` per command:** create, update, status, category create/update save once; task and category
  deletes are one conditional `DELETE … WHERE id AND xmin` (`ExecuteDeleteAsync`); step commands commit once (a lost
  race retries, AF-TD-001).
- **Reads:** `AsNoTracking` projections to `TodoTaskItem`, left join to categories in SQL, steps ordered in SQL,
  `ToPagedListAsync` with `pageSize` 1…100; category list counts active tasks in one correlated query; no query or
  save in a loop.
- **Search:** `SearchPattern.ContainsEachWord` (escaped, lower-cased per word) on `lower(title)`, matching the GIN
  `lower(title) gin_trgm_ops` index.
- **Unique violation → 409:** category name (pre-check plus caught `UniqueConstraintViolationException` on create
  and update); idempotency key race on task create answers like a repeat.
- **If-Match / `xmin`:** update and status compare the sent version, then EF checks the loaded `xmin` on save
  (`DbUpdateConcurrencyException` → `409`); a missing or unreadable header becomes 0 and fails validation (`400`);
  every step change moves the task version (integration test).
- **Idempotency (R-014):** key, kind and SHA-256 fingerprint stored in `todos.create_requests` in the same
  transaction; repeat returns the first id, other data `409 todos.idempotency_key_reused`; tested.
- **R-005:** entering the archive stamps `CompletedAtUtc`, leaving clears it, archived → archived keeps the first
  time, create as `Done` stamps it (unit tests).
- **R-012 active order:** priority rank, due date with nulls last, creation, id (integration test).
- **R-013:** `KyivCalendar` (`Europe/Kyiv`, fallback ids), unit tests around midnight UTC; overdue / due today
  counted only for active tasks; three assignees always present in order Maria, Vitaliy, Shared.
- **Summary focus:** overdue by due date, then today, then other `Urgent`; archived never; at most 8 (unit and
  integration tests).
- **Rate limiting (R-015):** all 10 writes have `RequireRateLimiting(PublicWrite)` and list `429`; integration test
  enumerates the endpoints.
- **OpenAPI:** every endpoint has a unique camelCase `WithName`, `WithSummary`, `Produces<T>` and `ProducesProblem`
  for its codes (architecture test on the generated document).
- **Migration (T3):** only adds the `todos` schema, so the previous release keeps working; generated SQL reviewed and
  applied twice with the idempotent script; `pg_trgm` creation is guarded (verified: a role without `CREATE` gets a
  notice and no failure) and the trigram index is created only when the extension exists; FK only inside the schema;
  seed with fixed ids; the deploy builds a `todos` bundle and runs it before the app (`api.yml`). Rollback: the
  previous release ignores the new schema.
- **Import (R-016, SC-008):** the script run twice leaves 13 rows, 4 of them archived with a completion time, 12 with
  a matched category.
- **Docs:** module page, ADR 0030, C4 (context and containers), architecture module table and README match the code
  after the fixes above.

## Verification (2026-11-26, after the fixes)

| Check | Result |
| --- | --- |
| `cd api && dotnet build HomeApp.slnx` | succeeded, 0 warnings, 0 errors |
| `dotnet test tests/UnitTests` | 142 passed |
| `dotnet test tests/ArchitectureTests` | 22 passed |
| `dotnet test tests/IntegrationTests` (Docker, `postgres:17-alpine`) | 42 passed (Todos: 16) |
| `api/docs/contracts/openapi.json` | unchanged by the fixes (no web regeneration needed for them) |
| Migration SQL and import script, twice each on a throwaway PostgreSQL 17 | as expected (see above) |
