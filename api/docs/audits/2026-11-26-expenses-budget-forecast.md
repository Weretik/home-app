# Audit 2026-11-26: Expenses module after SDD 005 (forecast, carry-over, outside limits)

- **Kind:** final feature audit (`backend-code-audit`) of the Expenses module, with the retroactive SDDs 001–004.
- **Scope:** `api/src/Modules/Expenses`, the host composition, `api/tests`, `api/docs` (rules, architecture, module
  page, ADR 0019–0022, SDD 001–005).
- **Measured against:** [SDD 005](../sdd/specs/expenses/005-budget-forecast-carryover/README.md) and 001–004,
  [rules](../rules.md), [architecture](../architecture/README.md), `AGENTS.md`, the owner's checklists
  (obsidian `Architecture/Checklists/backend/checks/new-feature.md` with T2 endpoint, T3 migration, T10 shared
  resource).
- **Evidence:** `dotnet build HomeApp.slnx` (OpenAPI regenerated), unit, architecture and integration tests on
  PostgreSQL 17 (Testcontainers), counts at the end.

## Step 0 — quality targets

There was no quality targets file (AF-001). [quality-targets.md](../architecture/quality-targets.md) now proposes:
read P95/P99 300/800 ms, write 400/1 000 ms (warm), peak under 1 request/s for 2 people, about 1 200 expenses a
year, best-effort availability, RPO 24 h / RTO 1 day, 0 extra cost a month. **All proposed, to confirm; no current
value is measured.** No speed or scaling verdict is given: at the stated volumes the summary's grouped reads (one row
per category and month since `LimitsFrom`) are not a risk.

## Findings

Severity: Critical, High, Medium, Low. Status: fixed (with evidence), open (owner and next step).

| ID | Sev. | Finding | Evidence | Status |
| --- | --- | --- | --- | --- |
| AF-001 | High | No quality targets file; no numbers to measure against. | `docs/architecture/` had none; SDD 005 overview "the project has no quality targets file" | fixed: proposed targets, to confirm by Vitaliy |
| AF-002 | High | SDD 005 contradicts the code on plan delete: R-FC-003/SC-FC-004, the contract and TS-010 require `If-Match` on `deletePlannedExpense`; the code, ADR 0022 and the module page delete without a version (like every delete of the module, 001 GAP-001, 003 GAP-010). | `DeletePlannedExpenseCommand.cs` (only `Id`), `DeletePlannedExpenseEndpoint.cs`, `contracts/api-contract.md:32` | open — product question 1; recorded in SDD 005 open questions |
| AF-003 | Medium | `GetMonthSummaryHandler` (208 lines) mixed reading with per-limit usage, carry and forecast arithmetic; the arithmetic had no unit test (quarter usage, `daysLeft`, `perDay`, SDD 004 GAP-015; SC-CO-009 needed a fixed clock). | handler before the change | fixed: `GetMonthSummary/SummaryFigures.cs` (pure) + `CategoryMonthAmount.cs`; handler 142 lines; `UnitTests/Expenses/Summary/SummaryFiguresTests.cs` (7 tests), `MonthSummaryValidatorTests` |
| AF-004 | Medium | The version race path (`DbUpdateConcurrencyException` → 409 between read and save) was never exercised (001 GAP-004), neither for entries nor for «record». | no test | fixed: `IntegrationTests/TestHost/RacingExpensesDb.cs` lets a second request in before the save; `ExpensesRaceApiTests` (edit race, record race) |
| AF-005 | Medium | `SortOrder` of a new limit is read (`max + 10`) then written without a race guard (003 GAP-009); a racing copy would also have been answered `409 idempotency_key_reused`, a wrong message. | `CreateSpendingLimitHandler.cs` | fixed: unique index `ux_spending_limits_sort_order` (migration `LimitSortOrderUnique`, renumbers duplicates first), handler maps that violation to `409 expenses.limit_added_meanwhile`; `ExpensesRaceApiTests.Two_Limits_AddedAtOnce…` |
| AF-006 | Low | `GetBudget` showed empty settings (version 0) when the row is missing while the edit answered 404, so the page could never save (003 GAP-012). | `GetBudgetHandler.cs` | fixed: both answer `404 expenses.budget_settings_not_found` (`BudgetErrors.SettingsNotFound`); contract: `getBudget` lists 404; web client regenerated; `ExpensesSummaryApiTests.Without_TheSettingsRow…` |
| AF-007 | Medium | The host validated scopes and the container build only in Development (ASP.NET default); a singleton capturing a `DbContext` would reach production. | `Program.cs` | fixed: `UseDefaultServiceProvider(ValidateScopes, ValidateOnBuild)` in every environment; `HostRulesTests`; every integration test boots it |
| AF-008 | Medium | Many documented rules had no automated check (one type per file, empty and wrapper folders, solution layout, central package versions, problem responses, `Idempotency-Key`/`If-Match`, rate limits, request ≠ command, queries never write, one save per command, page size, schema ownership, migrations vs model, host composition, secrets, ADR/SDD indexes). | `rules.md` before; 19 architecture tests | fixed: 55 architecture tests, [enforcement table](../rules.md#enforcement), [ADR 0023](../decisions/0023-every-rule-checked-by-a-test.md) |
| AF-009 | Low | Two files declared two types each (`ImportReport.cs` + `ImportMonth`, `RateLimitOptions.cs` + `FixedWindowLimit`), against "one type per file". | found by the new `SourceFileRulesTests` | fixed: split into own files |
| AF-010 | Medium | Missing tests: savings delete/repeat/stale/404 (002 GAP-007), income edit/delete and expense repeated create (001 GAP-003), entry and plan validators. | traceability of 001/002 | fixed: `ExpensesSavingsApiTests`, `ExpensesEditingApiTests` (+2), `EntryInputValidatorTests`, `PlannedInputValidatorTests` |
| AF-011 | Low | ADR 0019 said overspend is not carried; ADR 0021 changed that without a status note. | ADR 0019 | fixed: 0019 status "carry-over part superseded by 0021", sentence struck through, index updated; 0021 refers back |
| AF-012 | Low | API README, architecture README and the module page did not link the SDDs; ADR 0021/0022 did not link SDD 005. | the files | fixed |
| AF-013 | Low | `rules.md` asked for "handler tests in UnitTests" (001 GAP-002) while handlers are, rightly, tested on PostgreSQL (`xmin`, unique indexes, `ExecuteDelete` cannot be faked in memory). | `rules.md` Process | fixed: rule says what goes where; arithmetic is pulled out of handlers to be unit-tested |
| AF-014 | Medium | SDD 005 delivery evidence is not filled in: every `traceability.md` row is `planned`, task Evidence sections empty, delivery readiness unchecked, although the integration tests (`ExpensesCarryOverApiTests`, `ExpensesForecastApiTests`) exist and pass. | `005…/traceability.md`, `tasks/*` | open — owner: the SDD 005 delivery (TS-016); fill evidence from the green runs below |
| AF-015 | Low | An anonymous read endpoint has no rate-limit decision on record (ADR 0012/0015 cover writes); the summary reads history since `LimitsFrom`. | `GetMonthSummaryEndpoint.cs`, ADR 0015 | open — record "reads unlimited until sign-in" in ADR 0015, or a global soft limit |
| AF-016 | Low | The Kyiv time zone fallback to UTC is silent (004 GAP-013). | `GetMonthSummaryHandler.Today` | open — log a warning once; low impact (Linux App Service has tzdata) |
| AF-017 | Low | `listSavingsMonths` is not paged (002 GAP-006); bounded by one row per month. | `ListSavingsMonthsHandler` | open, accepted: name it as an exception in the module page |
| AF-018 | Low | `categoryIds` of a limit has no foreign key (003 GAP-011); no category delete exists. | `SpendingLimitConfiguration` | open — a future category delete must clean limits, with a test |
| AF-019 | Low | No record of personal data and retention, no action journal (checklist §5, §8). Data: amounts, comments, two first names. | — | open — one line in ADR 0015 or the module page; journal not needed for two users |

## Checklist new-feature (T1) and T2/T3/T10, items not covered by the table

- ✅ Business task and scenarios: SDD 005 requirements with `SC-*` (given/when/then).
- ✅ Module: Expenses owns the data; no other module touched; `Contracts` empty.
- ✅ Commands through aggregates (`Expense`, `BudgetSettings`, `PlannedExpense`), queries straight to DTOs; carry-over
  is a pure domain function (`LimitCarryOver`).
- ✅ One `SaveChanges` per command (now tested); «record» changes two roots (adds `Expense`, removes `PlannedExpense`)
  in one save, justified in SDD 005 R-FC-004 and ADR 0022.
- ✅ Errors as `Result` with codes; validators for every input (tested).
- ✅ Endpoints: request records, 201 + `Location`, problem responses, `If-Match`/`Idempotency-Key` (tested on OpenAPI).
- ✅ Migration `ForecastAndCarryOver`: add-only (`outside_limits` default false, `limits_from` default 2026-11-01, new
  table), compatible with the previous release; `LimitSortOrderUnique`: renumber + index, compatible; applied as a
  bundle before the deploy (ADR 0016). Rollback: both `Down` methods drop what they added.
- ✅ Race guards (T10): row version on every edit and «record», unique index on create keys and on `sort_order`;
  repeats: `Idempotency-Key` on creates, «record» repeat answers 404.
- ➖ Permissions and owner checks: no authentication yet (ADR 0015, temporary); both users see everything.
- ➖ Outbox, external calls, background work: none in this feature.

## Product questions (behavior not changed)

1. **Deletes without `If-Match`.** Expenses, incomes, savings movements, limits and plans are deleted without a
   version; SDD 005 asks for one on plan deletes. One rule for all deletes? (AF-002, 001 GAP-001, 003 GAP-010.)
2. **Quarter before `LimitsFrom`.** A quarterly limit starts at the quarter that holds `LimitsFrom` (Q4 2026 from
   1 October), so October 2026 spending counts toward the first quarter's carry, as the sheet's formula does; the
   note also says periods before `LimitsFrom` are ignored. Keep the sheet's behavior?
3. **Plans and `left`.** A plan shows as `planned` next to a limit but does not reduce its `left`/`perDay`. Should it?
4. **Plans of a quarter.** `LimitUsage.planned` of a quarterly limit sums the plans of all three months of the
   quarter. Intended?

## Rule → test summary

See [rules.md, Enforcement](../rules.md#enforcement). New in this audit: `Structure/` (folders: empty and wrapper;
one type per file and namespace; solution layout and central versions; ADR/SDD/module page indexes), `UseCases/`
(naming beside the folder; queries never write and read untracked; one save per command; no `throw`; page size ≤ 100),
`Http/` (problem responses, schemas, 400 on input, 429 and rate limit on writes, `Idempotency-Key`/`If-Match`, request
records, one endpoint per file), `Hosting/` (scope/build validation, one entry point per module, no background
services, no secrets, no migrate on startup), `Boundaries/DataOwnershipTests` (schema ownership, migrations match the
model). Review only, with reasons: no query in a loop, migration compatibility, outbox handlers (none yet), commands
return little, rule of three, test first, whether a change needs an ADR.

## Verification

- `dotnet build HomeApp.slnx`: 0 warnings, 0 errors; `openapi.json` differs only by `404` on `getBudget` (intended).
- `dotnet test tests/UnitTests`: 139 passed (was 121).
- `dotnet test tests/ArchitectureTests`: 55 passed (was 19).
- `dotnet test tests/IntegrationTests`: 41 passed, 0 skipped, on PostgreSQL 17 (was 30).
- Each new architecture rule was seen failing on a deliberate break (empty folder, wrapper folder, endpoint without
  rate limit, model change without migration), then restored.
- `web`: `npm run contracts:generate`, `npm run typecheck` green.
