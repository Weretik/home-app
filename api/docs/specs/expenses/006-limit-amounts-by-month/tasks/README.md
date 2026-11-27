# Task graph

| Task | Work | Depends on |
| --- | --- | --- |
| TS-001 | `AmountHistory`, `AmountStep` and their tests (SC-LA-002, SC-LA-003) | — |
| TS-002 | `SpendingLimit.Amounts`, `BudgetSettings.MonthlyLimits` (SC-LA-004, SC-LA-005) | TS-001 |
| TS-003 | `LimitCarryOver` with per-period amounts (SC-LA-006, SC-LA-007) | TS-001 |
| EN-001 | EF mapping and the migration `LimitAmountsByMonth` (SC-LA-009) | TS-002 |
| TS-004 | Create/update limit, update settings, get budget; validators (SC-LA-008) | TS-002 |
| TS-005 | Month summary with per-period amounts (SC-LA-001) | TS-002, TS-003 |
| TS-006 | Request records, endpoints, regenerated `openapi.json` | TS-004 |
| TS-007 | Integration tests updated (run by CI) | TS-006, EN-001 |
| TS-008 | Module page, ADR 0041, ADR 0027 note, decisions index | TS-005 |
| EN-002 | **Follow-up, after this release is live (not in this change):** migration `DropLimitSingleAmounts` — fill rows still holding `'[]'` from `amount` / `monthly_limit`, drop the `'[]'` defaults, drop `amount` / `monthly_limit` ([data model](../data-model.md#follow-up-migration-after-this-release-is-live)) | EN-001 released |

Each task: Red → Green → Refactor → Regression → evidence; keep every file to one responsibility and every folder to
at most six code files.

## Evidence

2026-11-27, branch `claude/project-thread-vhmqm0`.

| Check | Command | Result |
| --- | --- | --- |
| Build and contract | `cd api && dotnet build HomeApp.slnx` | succeeded, 0 warnings; `docs/contracts/openapi.json` regenerated (`AmountStep`, `BudgetView.monthlyLimits`, `SpendingLimitItem.amounts`, `SpendingLimitRequest.amountFrom`, `BudgetSettingsRequest.monthlyLimitFrom`) |
| Unit tests (TS-001 … TS-005) | `dotnet test api/tests/UnitTests` | 460 passed (new: `AmountHistoryTests`, SC-LA-004/005/006/007/008 and SC-LA-001 cases in the limit, settings, carry, summary and validator tests) |
| Architecture tests | `dotnet test api/tests/ArchitectureTests` | 93 passed (migrations match the model, folders, one type per file, no `throw` in Domain/Application) |
| Integration tests (TS-007) | `dotnet test api/tests/IntegrationTests` | 200 passed on PostgreSQL 17 in Docker, including `ExpensesLimitAmountsApiTests` (SC-LA-001, SC-LA-008, SC-LA-009) |
| Migration up and down (EN-001) | `dotnet ef database update` then `dotnet ef database update LimitSortOrderUnique` on an empty PostgreSQL 17 | up: every limit and the monthly limit hold one step from 2000-01-01 with the old amount; down: `amount` / `monthly_limit` restored from the last step |

Notes: an explicit `amountFrom` / `monthlyLimitFrom` always runs `ChangeFrom` (R-LA-002); only a missing one keeps
the "amount already in force → no change" shortcut (R-LA-005).

### Remediation of the audit (2026-11-27)

After [the audit](../../../../audits/2026-11-27-expenses-limit-amounts-by-month.md) (AF-001 … AF-011):

| Check | Command | Result |
| --- | --- | --- |
| Build and contract | `cd api && dotnet build HomeApp.slnx` | succeeded, 0 warnings; `openapi.json` unchanged by the remediation |
| Unit tests | `dotnet test api/tests/UnitTests` | 471 passed (new: `Amounts/AmountHistoryColumnTests`, merged duplicate days, read-only steps, explicit day vs no day for the limit and the settings) |
| Architecture tests | `dotnet test api/tests/ArchitectureTests` | 93 passed (migrations match the model) |
| Integration tests (Expenses) | `dotnet test api/tests/IntegrationTests --filter "FullyQualifiedName~Expenses"` | 40 passed on PostgreSQL 17 in Docker |
| Migration up, old-release insert, down | the migration SQL by hand on PostgreSQL 17 | up keeps `amount`; an insert without `amounts` gets `[]`; down restores the step in force in the current month, a `[]` row keeps its amount |
