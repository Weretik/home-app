# 001 — Expenses and incomes: lists, period filter, editing, one-time sheet import

- **Module:** Expenses
- **Type:** feature — **retroactive, describes implemented behavior**
- **Status:** implemented (PR #7 `824799b`, PR #8 `12ba66c`, period filter in PR #9 `b6fb5d7`); this SDD written 2026-11-26 from the code at `8378e4a`
- **Owner:** Vitaliy
- **Created:** 2026-11-26
- **Frontend counterpart:** [web 001](../../../../../../web/docs/specs/web/expenses/001-entries-pages/README.md)

The household's expenses and incomes, moved once from the Google Sheet «Учёт расходов» and since then added, changed
and deleted on the site. Expenses are listed by month or by any period up to 366 days, filtered by category and
payer; incomes by month and person. Creates are idempotent, edits are guarded by a row version.

This SDD was written after the code. It records what the code does at `8378e4a`, so that later changes and audits
have stable `R-*`/`SC-*` ids. It does not change behavior. Gaps between the code and the project rules are listed as
open questions in [overview](requirements/overview.md); none is fixed here.

## Requirements

- [Overview and scope](requirements/overview.md)
- [Entries and import](requirements/entries-and-import.md)

## Technical design

- [Domain](design/domain.md)
- [Infrastructure](design/infrastructure.md)
- [Data model](data-model.md)
- [API contract](contracts/api-contract.md)

## File map (implemented)

Paths relative to `api/src/Modules/Expenses/`.

| Path | State | One responsibility |
| --- | --- | --- |
| `Expenses.Domain/Entries/{Expense,Income,EntryRules,EntryErrors,PaymentMethod}.cs` | existing | the two entry aggregates and their shared rules |
| `Expenses.Domain/Parties/{Payer,Person}.cs`, `Expenses.Domain/Categories/` | existing | who and what for |
| `Expenses.Application/Features/Expenses/Commands/{Create,Update,Delete}Expense/`, `Queries/ListExpenses/` | existing | expense use cases |
| `Expenses.Application/Features/Incomes/Commands/{Create,Update,Delete}Income/`, `Queries/ListIncomes/` | existing | income use cases |
| `Expenses.Application/Features/Categories/Queries/ListCategories/` | existing | category list |
| `Expenses.Application/Idempotency/` | existing | `CreateRequest`, fingerprint and repeat rules (ADR 0007) |
| `Expenses.Application/Features/Imports/Commands/ImportSheet/`, `Imports/SheetFormat/` | existing | the one-time import and the sheet cell reader |
| `Expenses.Infrastructure/Persistence/Entries/`, `Persistence/Reference/CategoryConfiguration.cs`, `CategorySeed.cs` | existing | mapping, seed of 14 categories |
| `Expenses.Api/Endpoints/Expenses/`, `Endpoints/Incomes/`, `Endpoints/Import/`, `EntryVersionHeader.cs` | existing | endpoints, `If-Match` parsing |

## Planning and delivery

- [Scenario traceability](traceability.md)
- [Delivery readiness](checklist/delivery-readiness.md)

No task graph: the work is done. A future change to this area adds `TS-*` tasks here by the incremental migration rules of the backend SDD
template: keep these ids, add new ones.

## Change history

- 2026-11-26 — retroactive SDD created from the code at `8378e4a`.
