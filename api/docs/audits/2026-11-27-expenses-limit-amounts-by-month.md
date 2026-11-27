# Audit 2026-11-27: Expenses, limit amounts by month (SDD 006)

- **Kind:** final feature audit (`backend-code-audit`) of the uncommitted change under `api/`.
- **Scope:** `api/src/Modules/Expenses` (Domain `Budget/`, `Budget/Amounts/`; Application budget and summary use
  cases, `HouseholdCalendar`; Infrastructure `Persistence/Reference/`, migration `LimitAmountsByMonth`; Api budget
  endpoints), `api/tests` (unit, architecture, integration sources), `api/docs` (module page, ADR 0027, ADR 0041,
  decisions and specs indexes, SDD 006). `web/` is out of scope.
- **Measured against:** [SDD 006](../specs/expenses/006-limit-amounts-by-month/README.md),
  [ADR 0041](../decisions/0041-limit-amounts-kept-by-month.md), [rules](../rules.md),
  [architecture](../architecture/README.md), `AGENTS.md`, the owner's checklists (obsidian
  `Architecture/Checklists/backend/checks/new-feature.md`, tasks T2 endpoint, T3 migration, T10 shared resource,
  T18 aggregate).
- **Evidence:** `dotnet build HomeApp.slnx`, `dotnet test tests/UnitTests`, `dotnet test tests/ArchitectureTests`
  (results at the end). Integration tests were not run in this audit (CI runs them; the SDD records a green run).

## Step 0 — quality targets

[quality-targets.md](../architecture/quality-targets.md) exists; every value is still **proposed, to confirm** and
none is measured (read P95/P99 300/800 ms, write 400/1 000 ms, under 1 request/s, about 1 200 expenses a year,
RPO 24 h / RTO 1 day, 0 extra cost). This change is not on a hot path: the summary still makes one grouped read; the
carry walk now calls `AmountHistory.For` per period (a scan over a handful of steps) in memory. No speed or scaling
verdict is given beyond "no new query, no N+1".

## Findings

Severity: Blocker, Major, Minor. Status: open unless stated (this audit changes no code).

| ID | Sev. | Finding | Evidence | Suggested fix |
| --- | --- | --- | --- | --- |
| AF-001 | Blocker | The migration is not expand–contract: `Up` adds `amounts` / `monthly_limits` as `NOT NULL` without a default and drops `amount` / `monthly_limit` in the same release. The deploy runs the bundle before the new code (ADR 0016), so the running previous release reads a dropped column (every budget and summary request answers 500) and cannot insert a limit until the new build is live; a failed code deploy leaves production broken until `Down` is run by hand. Breaks rules.md "Migrations … must keep the previous release working (add first, remove later)", ADR 0016 and checklist T3 🔴 item. The SDD states the incompatibility (data-model.md:15) but records no accepted exception. | `Migrations/20261127021929_LimitAmountsByMonth.cs:42-50` (DropColumn in `Up`), lines 27-36 (`DROP DEFAULT`); `specs/expenses/006-limit-amounts-by-month/data-model.md:14-15`; `rules.md:87` | Keep `amount` / `monthly_limit` in this release (unmapped, made nullable or kept with their values) and drop them in a follow-up migration after the release is live; keep the `'[]'`-free default problem in mind (old code inserting a limit needs `amounts` to have a default, e.g. keep the default until the drop migration, and let `AmountHistoryColumn.Read` treat `[]` as needing a fallback). Alternatively, record a deliberate, owner-approved exception (short outage window, two users) in ADR 0041 and the SDD data model, naming the rollback steps. |
| AF-002 | Major | Code and R-LA-002 disagree when a later step exists. R-LA-002: "Steps that start on or after that day are replaced by one step". `SpendingLimit.Update` and `BudgetSettings.Update` skip `ChangeFrom` whenever the amount of the chosen period already equals the new amount, so later steps survive. Example: steps 7 000 (2000-01), 5 000 (2027-01); in December the user sets 7 000 from December (or any earlier month) to cancel the planned decrease: nothing changes, January keeps 5 000, and the request answers 204. The trade-off is deliberate (R-LA-005, the rename test, task evidence note) but the rule text and the module page say otherwise only partially, and there is no way to express "cancel the later step" except choosing that step's own month. | `Expenses.Domain/Budget/SpendingLimit.cs:68`; `Expenses.Domain/Budget/BudgetSettings.cs:64`; `requirements/overview.md:20-23` (R-LA-002) vs `tasks/README.md` notes; `SpendingLimitTests.A_Rename_WithTheAmountInForce_LeavesTheSteps_Alone` | Decide in the SDD and make code follow it. Option A (recommended): pass `DateOnly? amountFrom` to the domain; an explicit `amountFrom` always applies `ChangeFrom` (R-LA-002), a missing one keeps the "amount in force → no change" shortcut (R-LA-005, renames from the current form). Option B: keep the code and rewrite R-LA-002/R-LA-005 plus a scenario stating that an amount already in force keeps later steps. Add a unit test for the chosen case either way. |
| AF-003 | Minor | SDD 006 states the wrong status codes: "refused with 412" and "412 changed meanwhile, 428 no `If-Match`". The code answers `409 expenses.changed_meanwhile` (`EntryErrors.ChangedMeanwhile` is `Error.Conflict`) and `400` for a missing `If-Match` (validator `Version != 0`); the endpoint summaries and module page say 409. | `specs/…/006-limit-amounts-by-month/data-model.md:12`; `contracts/api-contract.md:17`; `Expenses.Domain/Entries/EntryErrors.cs:29`; `BuildingBlocks.Api/Problems/ErrorProblems.cs:19` | Change the SDD to 409 / 400. |
| AF-004 | Minor | The domain record `AmountStep` is at once the `jsonb` storage format (`AmountHistoryColumn` serializes it with web defaults) and the HTTP response schema (`BudgetView.MonthlyLimits`, `SpendingLimitItem.Amounts`). Renaming a domain property silently changes the API contract and, worse, makes stored rows deserialize to `From = 0001-01-01, Amount = 0` (positional-record parameters missing from JSON get default values, no error). The converter has no unit round-trip test; only integration tests touch it. | `Persistence/Reference/AmountHistoryColumn.cs:16,27-30`; `Application/Features/Budget/Queries/GetBudget/BudgetView.cs`, `SpendingLimitItem.cs` | Serialize through an Infrastructure-private storage record with explicit `[JsonPropertyName("from")]` / `("amount")` (or explicit names on `AmountStep`) and add a unit test of `Converter` for `[{"from":"2000-01-01","amount":7000.00}]` both ways; optionally a separate response record in `GetBudget/` (name not used by any other module, rules.md:36). |
| AF-005 | Minor | `AmountHistory` is not fully immutable: `Steps` returns the private array, so `(AmountStep[])history.Steps` can be mutated, which also bypasses the EF value comparer (snapshot is the same instance). `From` accepts duplicate `From` days and does not check amounts (data from the column is trusted). `From` throws (`ThrowIfZero`) on `[]`, so a row holding `[]` would turn every budget read into a 500. | `Expenses.Domain/Budget/Amounts/AmountHistory.cs:21,27-33` | Return `Array.AsReadOnly(_steps)` (or hold an `ImmutableArray`); reject or merge duplicate days in `From`; keep the throw for programmer errors but make sure no `[]` can reach the column (see AF-001). |
| AF-006 | Minor | `Down` restores the amount of the **last** step (`-> -1`), which can be a future step (an amount set from a later month), not the amount in force on the day of the rollback. Documented as "last step", so behavior is known, but it changes the current month's limit on rollback. | `Migrations/20261127021929_LimitAmountsByMonth.cs:77-78` | Pick the last step whose `from` is on or before `date_trunc('month', now())` (fallback: the first step), or document the trade-off in the SDD rollback note. |
| AF-007 | Minor | Kyiv "today" now decides where a write lands (default `amountFrom` / `monthlyLimitFrom`), but `HouseholdCalendar.TodayInKyiv` falls back to UTC silently and does not try the Windows id; the same helper exists four times (`Expenses.Application/HouseholdCalendar.cs`, `Gifts.Application/HouseholdDay.cs`, `Todos.Application/KyivCalendar.cs` with a Windows fallback, `Notifications.Application/KyivTime.cs`). With the UTC fallback a change made on the 1st between 00:00 and 02:00/03:00 Kyiv would start in the previous month. Carries the open 2026-11-26 AF-016. | `Expenses.Application/HouseholdCalendar.cs:9-16` | Rule of three is reached: move one Kyiv clock extension (with `TryFindSystemTimeZoneById` for both ids and a logged fallback) into `BuildingBlocks.Application` and use it from the four modules; at least copy the Todos fallback. |
| AF-008 | Minor | `amountFrom` appears on `createSpendingLimit` in the contract (shared `SpendingLimitRequest`) and is silently ignored. Documented in the module page and SDD, but a client cannot tell from the schema. | `Expenses.Api/Endpoints/Budget/Limits/SpendingLimitRequest.cs:11`; `openapi.json` `SpendingLimitRequest.amountFrom` | Accept as is (documented), or split into `CreateSpendingLimitRequest` / `UpdateSpendingLimitRequest` so create has no `amountFrom`. |
| AF-009 | Minor | SC-LA-007 text says the monthly limit is 0 "from December" and "January takes in nothing", which holds trivially because January has no limit either; the unit test checks the stronger case (limit back to 50 000 in January). | `requirements/overview.md:50-51`; `LimitCarryOverTests.A_Period_WithoutALimit_ResetsTheCarry` | Reword SC-LA-007 to "0 in December, 50 000 again from January". |
| AF-010 | Minor | Status mismatch: the specs index says "in implementation", the SDD header says "accepted for implementation" while its history says "implemented". | `docs/specs/README.md:18`; `specs/…/006…/README.md:5,95-97` | Set one status (e.g. "implemented, audit open") in both. |
| AF-011 | Minor (note) | `Persistence/Reference/` is now at the six-code-file limit (`AmountHistoryColumn` was the sixth). | `ls Persistence/Reference` | The next file there needs a split by meaning (e.g. `Reference/Categories/` and `Reference/Budget/`). |

## What was checked and holds

- ✅ **Layers.** `Domain/Budget/Amounts/` (`AmountHistory`, `AmountStep`) uses no EF Core or ASP.NET Core; the
  converter and comparer sit in Infrastructure (`AmountHistoryColumn`). Application only orders steps: load, version
  check, aggregate method, one save. Architecture tests (layers, one type per file, no `throw`, folders) are green.
- ✅ **Folders.** `Domain/Budget` 5 files + `Amounts/` (2), `Expenses.Application` root 4, `Persistence/Reference` 6
  (AF-011). No empty, wrapper or one-file folder added. SDD file map matches the created files.
- ✅ **Contract.** Every changed endpoint keeps `WithName`, `WithSummary`, `Produces…`/`ProducesProblem`; a fresh
  build regenerated `openapi.json` identical to the working-tree copy (`git diff --stat api/docs/contracts`: the same
  51+/5- lines as before the build). Changes are additive for readers (`AmountStep`, `monthlyLimits`, `amounts`,
  optional `amountFrom`, `monthlyLimitFrom`).
- ✅ **Rules R-LA-001 … R-LA-009 / SC-LA-001 … SC-LA-009** (except AF-002, AF-009): `AmountHistory.Start`
  (R-LA-001); `ChangeFrom` replaces later-or-equal steps and merges an equal step (R-LA-002, SC-LA-002/003);
  defaults to today in Kyiv, range 2000-01-01…2100-12-31 checked by validators and domain (R-LA-003, SC-LA-008);
  month ↔ quarter restarts (R-LA-004, SC-LA-005); quarterly start (SC-LA-004); `LimitCarryOver.For` asks each
  period's amount, `≤ 0` resets the carry and the asked period answers (0, 0) (R-LA-006, SC-LA-006/007);
  `SummaryFigures.Usage` / `TotalCarry` / forecast use the period's amounts (R-LA-007, SC-LA-001); `GetBudget`
  returns current amounts plus steps (R-LA-008); migration SQL builds one step from 2000-01-01 per row (R-LA-009).
- ✅ **Data.** `jsonb` columns with a value converter (camelCase `from`/`amount`, matching the migration SQL) and a
  value comparer over the steps (safe snapshot by reference for an immutable value, see AF-005). `xmin`
  (`IsRowVersion`) unchanged on both tables; a changed history is a new instance, the comparer sees it, the row is
  updated and its `xmin` moves, so an edit on an older version still answers 409. The seed (`HasData`) now holds the
  serialized step; the migration converts the existing row by SQL, so a changed production value is kept.
- ✅ **Idempotency and races.** `createSpendingLimit` keeps its `Idempotency-Key` and sort-order unique index;
  `amountFrom` is not used on create. Updates stay guarded by `If-Match` + `xmin`. No outbox, external call or
  background work in this change.
- ✅ **Docs.** Module page (aggregates, rules, tables, endpoints, migrations, layer table), ADR 0041 added and
  indexed, ADR 0027 marked "superseded in part", specs index row added. No C4 change needed (no new module,
  container or external system).
- ➖ Permissions, personal data, rate limits: unchanged (endpoints closed by default behind Google sign-in, ADR 0037;
  writes keep `PublicWrite`). Error messages are Ukrainian like the rest of the module (web ADR 0014).

## Verification

- `cd api && dotnet build HomeApp.slnx`: build succeeded, 0 warnings, 0 errors; `openapi.json` regenerated, unchanged
  against the working tree.
- `dotnet test tests/UnitTests`: 460 passed, 0 failed.
- `dotnet test tests/ArchitectureTests`: 93 passed, 0 failed.
- Integration tests: not run here (Docker; CI). Sources reviewed: `ExpensesLimitAmountsApiTests` (SC-LA-001, 008,
  009 and a limit changed from a month), budget and summary tests updated to the new shapes.

## Remediation

2026-11-27, same branch, uncommitted. Outcome per finding:

| ID | Outcome | What changed |
| --- | --- | --- |
| AF-001 | Fixed | `LimitAmountsByMonth` is expand only: `Up` adds `amounts` / `monthly_limits` with default `'[]'` (kept) and fills them, keeps `amount` / `monthly_limit` (dropped from the EF model only; snapshot and Designer unchanged, "migrations match the model" green) and gives them `default 0` so the new code can insert. The previous release keeps reading and writing the old columns; a limit it inserts gets `[]`, which `AmountHistoryColumn` reads as one step of 0 ("no limit") instead of a 500. Chosen over a sync trigger (no database code, a window of minutes for two users). `Down` works on the expanded schema (writes the old columns back, drops their defaults and the new columns). Follow-up migration recorded as task EN-002 (fill `[]` rows from the old amount, drop the defaults and the old columns) in the SDD data model and task graph, not written. Up, an old-release insert and Down were run by hand on PostgreSQL 17. |
| AF-002 | Fixed (option A) | `SpendingLimit.Update(…, DateOnly? amountFrom, DateOnly today)` and `BudgetSettings.Update(…, DateOnly? monthlyLimitFrom, DateOnly today)`: an explicit day always applies `ChangeFrom` (R-LA-002); a missing one uses today's period and keeps the "amount in force → no change" shortcut (R-LA-005). Handlers pass the request value through. R-LA-002 / R-LA-005 reworded, SC-LA-010 added; unit tests for both cases on the limit and on the settings. **Web impact:** the web forms send `amountFrom` / `monthlyLimitFrom` on every edit (default: the current month), so a rename or a savings-goal save from the web now cancels a planned later step; the web should send the field only when the amount changed or a month was chosen (handed to the web side, `web/` not touched here). |
| AF-003 | Fixed | SDD data model and API contract now say `409 expenses.changed_meanwhile` and `400` for a missing `If-Match`. |
| AF-004 | Fixed | `AmountHistoryColumn` serializes a private nested `StoredStep` record with `[JsonPropertyName("from"/"amount")]` and `[JsonRequired]` (a step without those names fails the read instead of becoming `0001-01-01, 0`). New `tests/UnitTests/Expenses/Amounts/AmountHistoryColumnTests` (write shape, read back, `[]`, missing names, comparer); `Expenses.Infrastructure` exposes internals to `UnitTests`. The response schema still uses the domain `AmountStep` (`from`, `amount`); a separate response record is left as optional (contract unchanged). |
| AF-005 | Fixed | `Steps` returns `Array.AsReadOnly`; `From` merges steps on the same day (the last one given wins) and still refuses an empty list (programmer error); an empty stored list is mapped by the converter (AF-001). `AmountHistoryTests` moved with the converter test to `tests/UnitTests/Expenses/Amounts/` (two files, folder rules hold). |
| AF-006 | Fixed | `Down` restores the last step whose `from` is on or before `date_trunc('month', now())`, else the first step; a `[]` row keeps its old amount. |
| AF-007 | Partly fixed, rest deferred | `HouseholdCalendar` resolves the zone once with `TryFindSystemTimeZoneById` over `Europe/Kyiv`, `Europe/Kiev`, `FLE Standard Time` (as Todos' `KyivCalendar`), UTC only when none exists. **Deferred:** one shared Kyiv clock in `BuildingBlocks.Application` for Expenses, Gifts, Todos and Notifications (cross-module, out of this change; carries 2026-11-26 AF-016). |
| AF-008 | Accepted | `amountFrom` stays on the shared `SpendingLimitRequest` and is ignored on create; documented in the module page and the SDD contract. |
| AF-009 | Fixed | SC-LA-007 reads "0 in December, 50 000 again from January", matching `LimitCarryOverTests.A_Period_WithoutALimit_ResetsTheCarry`. |
| AF-010 | Fixed | Status "implemented, audited" in `docs/specs/README.md` and the SDD header. |
| AF-011 | Note | `Persistence/Reference/` stays at six code files (the stored record is nested in `AmountHistoryColumn`); the next file there splits it by meaning. |

### Verification after remediation

- `cd api && dotnet build HomeApp.slnx`: succeeded, 0 warnings, 0 errors; `openapi.json` unchanged by the remediation
  (still the SDD's 51+/5- lines against `HEAD`).
- `dotnet test api/tests/UnitTests`: 471 passed, 0 failed.
- `dotnet test api/tests/ArchitectureTests`: 93 passed, 0 failed.
- `dotnet test api/tests/IntegrationTests --filter "FullyQualifiedName~Expenses"`: 40 passed, 0 failed (PostgreSQL 17
  in Docker).
