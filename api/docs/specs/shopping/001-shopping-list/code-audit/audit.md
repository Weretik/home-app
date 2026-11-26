# Shopping list — final code audit

- **Scope:** `src/Modules/Shopping/**`, the host composition change, `tests/UnitTests/Shopping/**`,
  `tests/IntegrationTests/Api/Shopping/**`, the architecture test change, and the docs of this feature, against this
  SDD, [API rules](../../../../rules.md), the [architecture](../../../../architecture/README.md) and the ADRs.
- **Kind:** final feature audit (new module), 2026-11-26. Findings marked *fixed* were fixed in the same change.

## Step 0: quality targets

The project has no quality targets file (no `quality-targets.md` anywhere in the repository). No P95/P99, peak,
availability, RPO/RTO or monthly cost targets exist to check against, so no speed or scaling verdict is given.
Expected load of this feature, from the brief: two users, list of tens of items, history growing by a few hundred
rows a month, keyword dictionary of about 400 rows. See AF-001.

## Commands and results

| Check | Command | Result |
| --- | --- | --- |
| Build (regenerates `openapi.json`) | `cd api && dotnet build HomeApp.slnx` | 0 warnings, 0 errors |
| Unit tests | `dotnet test api/tests/UnitTests` | 198 passed, 0 failed |
| Architecture tests | `dotnet test api/tests/ArchitectureTests` | 23 passed, 0 failed (includes the new `Every_Module_IsComposedByTheHost`) |
| Integration tests build | `dotnet build api/tests/IntegrationTests` | builds |
| Integration tests run | all 40 (Expenses and Shopping) against a temporary local PostgreSQL 16, by a local-only `ApiFactory` switch that was reverted afterwards; CI runs them with Testcontainers | 40 passed, also on a rerun against the same database |
| Model matches migration | `dotnet ef migrations has-pending-model-changes` for `ShoppingDbContext` and `ExpensesDbContext` | "No changes have been made to the model since the last migration." for both |
| Seed against the source | live sheet «Справочник» read through the Sheets tool | 398 keyword rows (last at row 399), 13 departments; 2 duplicate keywords dropped (396 seeded), boundaries spot-checked |
| Expenses contract unchanged | diff of the Expenses paths and schemas in `openapi.json` | identical |

## Findings

| ID | Severity | Status | Area | Finding |
| --- | --- | --- | --- | --- |
| AF-001 | High | open (project-wide) | quality targets | No quality targets file |
| AF-002 | High | fixed | deployment | No migration bundle for Shopping in `.github/workflows/api.yml` |
| AF-003 | Medium | fixed | domain model | Re-add rule lived in the handler |
| AF-004 | Low | fixed | application | Keyword length helper duplicated a domain rule |
| AF-005 | Low | fixed | persistence | Two indexes on `keyword` overwrote each other |
| AF-006 | Medium | fixed | persistence | Times kept with 100 ns ticks, stored with microseconds |
| AF-007 | Low | fixed | folders | Integration `Api/` folder would hold more than six files |
| AF-008 | Low | fixed | architecture tests | No test that every module is composed by the host |
| AF-009 | Low | accepted | contract | History paged by `page`/`pageSize`, not `before`/`limit` |
| AF-010 | Low | accepted | contract | `Person` OpenAPI schema shared with Expenses |
| AF-011 | Low | accepted | queries | Keyword list and item list are not paged |
| AF-012 | Low | accepted | application port | `IShoppingDbContext` exposes `Database` |
| AF-013 | Low | accepted | queries | The matcher loads the whole dictionary per request |
| AF-014 | High | fixed | deployment | `CREATE EXTENSION pg_trgm` failed in production; trigram index dropped |
| AF-015 | Low | fixed | docs | Design named a wrong index and left out the trigram index |
| AF-016 | Info | open (merge) | docs | `decisions/README.md` will conflict with `main` |

### AF-001 — no quality targets file

- **Evidence:** `find . -name "quality-targets*"` finds nothing; `api/docs/` has `architecture`, `contracts`,
  `decisions`, `modules`, `rules.md`, `specs`.
- **Impact:** no number to judge latency, availability or cost against; this audit gives no speed verdict.
- **Target change (proposed, to confirm):** `api/docs/architecture/quality-targets.md` with: P95 < 300 ms and
  P99 < 800 ms for list reads, P95 < 500 ms for writes (measure from App Service request logs); peak 2 users,
  < 5 req/s; availability as App Service free/basic tier gives, no SLA; RPO 24 h and RTO 1 day from the managed
  database backups; monthly cost: the current hosting bill, no new paid service from this feature. Current values
  must be measured, not invented.
- **Owner:** Vitaliy. Systemic, outside this feature.

### AF-002 — no migration bundle for Shopping

- **Evidence:** `.github/workflows/api.yml:61-67` builds one bundle per module but lists only
  `--context ExpensesDbContext`. The apply step (`:113`) runs every bundle in `migrations/`, so Shopping's would
  run once it is built.
- **Impact:** in production the host's `ShoppingDatabaseInitializer` does not migrate (migrations run before
  deploy), so the `shopping` schema would not exist and every Shopping endpoint would fail.
- **Target change:** add next to the Expenses line:
  `dotnet ef migrations bundle --configuration Release --self-contained --target-runtime linux-x64 --project src/Modules/Shopping/Shopping.Infrastructure --startup-project src/Host/Host.Api --context ShoppingDbContext --output ${{ runner.temp }}/migrations/shopping`
  (same flags as the Expenses line). Outside `api/`, so not done by this change.
- **Verification:** the CI deploy log shows "Migrating shopping" and `shopping.__ef_migrations_history` holds
  `20261126220437_Initial`.

### AF-003 — re-add rule in the handler (fixed)

- **Evidence (before):** `AddItemsHandler` decided "needed → refresh, in cart → restore" and called two public
  methods. **Fix:** `ShoppingItem.AddAgain(quantity, urgent, addedBy, now)` returns `ReAddOutcome`;
  `Refresh`/`Restore` are private. Covered by `ShoppingItemTests`.

### AF-004 — keyword length helper (fixed)

- **Evidence (before):** `Features/Keywords/KeywordInput.cs` repeated the 120-character input limit.
  **Fix:** removed; validators use `DepartmentKeyword.MaxInputLength`.

### AF-005 — keyword indexes overwrote each other (fixed)

- **Evidence:** two unnamed `HasIndex(k => k.Keyword)` calls map to one EF index. **Fix:** named overloads
  `ux_department_keywords_keyword` (unique) in the migration (the trigram index was later dropped, AF-014).

### AF-006 — time precision (fixed)

- **Evidence:** an integration test compared `boughtAtUtc` from the write response with the read and they differed
  (.NET 100 ns ticks, PostgreSQL microseconds). **Fix:** `ItemWindow` truncates "now" to microseconds; the test passes.

### AF-007 — integration folder size (fixed)

- **Evidence:** `tests/IntegrationTests/Api/` held six files before Shopping. **Fix:** split by area into
  `Api/Expenses/` (4), `Api/Platform/` (2) and `Api/Shopping/` (4). Folder test passes.

### AF-008 — host composition test (fixed)

- **Fix:** `ModuleBoundaryTests.Every_Module_IsComposedByTheHost` fails when a module folder has no `Add<Module>Module`
  and `Map<Module>Module` call in `Program.cs`.

### AF-009 — history paging (accepted deviation)

- The brief asks for `before`/`limit`; the endpoint follows the Expenses pattern (`page`, `pageSize`, total) so the
  web client reuses its pager. Recorded in [the contract](../contracts/api-contract.md). Two users and a few hundred
  rows a month make offset paging cheap.

### AF-010 — shared `Person` schema (accepted)

- Both modules declare `Person { Vitaliy, Maria }`; the OpenAPI generator names both `Person`. Values are equal and
  the Expenses contract is byte-identical, so the shared schema is harmless. If a module ever changes its values,
  rename one enum (e.g. `ShoppingPerson`).

### AF-011 — unpaged lists (accepted)

- `GET /shopping/items` (bounded by the cart window, tens of rows) and `GET /shopping/keywords` (about 400 rows,
  filtered by department and search in SQL) are returned whole. Revisit when the dictionary passes a few thousand
  rows.

### AF-012 — `Database` on the port (accepted)

- `IShoppingDbContext.Database` is used only by `DeleteDepartmentHandler` for its one explicit transaction (two
  set-based `UPDATE`s and a delete). Documented in [infrastructure design](../design/infrastructure.md).

### AF-013 — matcher loads the whole dictionary (accepted)

- `DepartmentDictionary` reads departments and keywords (about 400 short rows, no tracking) per add or match
  request. Not measured (no targets, AF-001); at this size it is one small indexed read. A cache would need
  invalidation on every keyword edit.

### AF-014 — `pg_trgm` at deploy (fixed)

- The first production deploy failed: `CREATE EXTENSION IF NOT EXISTS pg_trgm` gave `3F000: no schema has been
  selected to create in` on the hosting provider's server. **Fix:** the extension and the gin trigram index on
  `department_keywords.keyword` were removed (the search scans about 400 rows); the unapplied `Initial` migration was
  regenerated. Exception to the rules.md search rule recorded in ADR 0031.

### AF-015 — design index names (fixed)

- `design/infrastructure.md` named `ix_shopping_items_status_bought_at`; the migration has
  `ix_shopping_items_status_bought_at_utc`, and the trigram index was missing from the list. Fixed. The same file
  now also notes that department create and reorder change several rows in one commit.

### AF-016 — decisions index merge

- `main` has API ADRs 0021–0026 (Gifts), Budget takes 0027–0029 and Todos 0030; Shopping uses **0031** and **0032**.
  This tree's `docs/decisions/README.md` lacks the rows 0021–0030, so merging will conflict there: keep all rows.

## Database checklist

| Use case | Commits | Race protection | If it stops half-way |
| --- | --- | --- | --- |
| Add items | 1 | `Idempotency-Key` in `create_requests` (unique key, caught violation answers with the winner) | nothing saved |
| Update item, set status | 1 | `xmin` version from `If-Match` | nothing saved |
| Delete item | 1 | `If-Match` version | nothing saved |
| Clear cart | 1 conditional `UPDATE` | the condition itself (only rows still in the cart) | nothing saved |
| Create / update / reorder department | 1 | unique name index; version on update | nothing saved |
| Delete department | 1 explicit transaction (2 `UPDATE`s + delete) | transaction; fallback cannot be deleted | rolled back |
| Create / update / delete keyword | 1 | unique keyword index (caught → 409) | nothing saved |
| Update settings | 1 | version | nothing saved |

Reads use `AsNoTracking` projections; no query or save in a loop; history is paged in SQL; frequent items are
grouped in SQL; keyword search is an escaped `ILIKE` on the trigram index. No external side effects, so no outbox.

## Rate limits and access

13 write endpoints use `RateLimitPolicies.PublicWrite`; 8 reads use the global soft limit, as in Expenses. Access is
open until Google sign-in ([ADR 0015](../../../../decisions/0015-expenses-open-until-identity.md)); no personal data
beyond the two fixed names.

## Folder axiom

No folder in `src/Modules/Shopping`, `tests/UnitTests/Shopping` or `tests/IntegrationTests/Api` holds more than six
code files; no empty, one-file or wrapper folders apart from the layout levels `Commands/`/`Queries/` and the
`Shopping.Contracts` project, which holds only its project file exactly like `Expenses.Contracts`. The folder
architecture test passes.

## Verdict

The module is complete against the SDD. It must not deploy before AF-002 is fixed (and AF-014 checked). AF-001 is a
project-wide gap.
