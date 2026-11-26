# 003 — Budget settings and spending limits

- **Module:** Expenses
- **Type:** feature — **retroactive, describes implemented behavior**
- **Status:** implemented (PR #9 `b6fb5d7`); SDD written 2026-11-26 from the code at `8378e4a`
- **Owner:** Vitaliy
- **Created:** 2026-11-26
- **Frontend counterpart:** [web 003](../../../../../../web/docs/specs/web/expenses/003-budget-settings-page/README.md)
- **Changed by:** [005 forecast and carry-over](../005-budget-forecast-carryover/README.md) (adds `limitsFrom` and carry-over)

The budget of the sheet's «Бюджет» tab: named spending limits over sets of categories for a month or a quarter
(limits may overlap), the monthly limit on all expenses and the savings goal
([ADR 0019](../../../../decisions/0019-spending-limits-as-category-sets.md)). The `Budget` migration seeded the sheet's
seven limits, a monthly limit of 50 000 and a savings goal of 100 000.

Retroactive: no behavior change; gaps are in [overview](requirements/overview.md).

## Requirements

- [Overview and scope](requirements/overview.md)
- [Budget and limits](requirements/budget-and-limits.md)

## Technical design

- [Domain](design/domain.md)
- [Infrastructure](design/infrastructure.md)
- [Data model](data-model.md)
- [API contract](contracts/api-contract.md)

## File map (implemented)

Paths relative to `api/src/Modules/Expenses/`.

| Path | State | One responsibility |
| --- | --- | --- |
| `Expenses.Domain/Budget/{SpendingLimit,LimitPeriod,BudgetSettings,BudgetErrors}.cs` | existing | limits, period arithmetic (`PeriodAround`), the singleton settings |
| `Expenses.Application/Features/Budget/Commands/{Create,Update,Delete}SpendingLimit/`, `UpdateBudgetSettings/` | existing | writes |
| `Expenses.Application/Features/Budget/Queries/GetBudget/` | existing | settings + limits in order |
| `Expenses.Application/BudgetCategories.cs` | existing | checks a limit's categories exist |
| `Expenses.Infrastructure/Persistence/Reference/{SpendingLimitConfiguration,BudgetSettingsConfiguration,BudgetSeed}.cs` | existing | mapping and seed values |
| `Expenses.Api/Endpoints/Budget/Limits/`, `Endpoints/Budget/Overview/` | existing | endpoints (Overview also holds the month summary of 004) |

## Planning and delivery

- [Scenario traceability](traceability.md)
- [Delivery readiness](checklist/delivery-readiness.md)

## Change history

- 2026-11-26 — retroactive SDD created from the code at `8378e4a`.
