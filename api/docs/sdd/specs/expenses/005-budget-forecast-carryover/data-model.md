# Budget forecast and carry-over — data model

## Context

All data belongs to the Expenses module, schema `expenses`. Two existing tables gain one column each, one table is
new. The summary's new figures are computed on read and never stored.

## Model

```text
expenses.expenses (existing)
└── outside_limits  boolean not null default false   — R-OL-001

expenses.budget_settings (existing, one row)
└── limits_from     date not null                      — R-CO-007, seeded 2026-11-01

expenses.planned_expenses (new)                         — R-FC-001
├── id              uuid, primary key (v7)
├── month           date, first day of the month, index ix_planned_expenses_month
├── name            varchar(100), trimmed, not empty
├── category_id     uuid → expenses.categories (restrict)
├── payer           text enum (Vitaliy, Maria, Shared)
├── amount          numeric(12,2) > 0
├── created_at_utc  timestamptz
└── xmin            row version (no column of its own)

expenses.create_requests (existing)
└── kind 'planned' added for plan creates

MonthSummary (read model, extended)
├── carriedIn, carriesOut                 — monthly total, R-CO-006
├── outsideLimitsCount, outsideLimitsAmount — R-OL-003
├── forecast { planned, expectedExpenses, expectedResult, expectedTotalLeft } — R-FC-005
└── limits[] + carriedIn, carriesOut, planned — R-CO-001, R-FC-006
```

## Identifiers

| Object | ID / key | Where created | Public id needed? | Reason |
| --- | --- | --- | --- | --- |
| `PlannedExpense` | `uuid` v7 | domain `Create` (`Guid.CreateVersion7`) | yes, in routes | same strategy as every Expenses aggregate ([ADR 0013](../../../../decisions/0013-expenses-module-model.md)) |
| plan create request | `Idempotency-Key` string ≤ 100 | client | no | `create_requests.key` primary key |
| expense from a plan | `uuid` v7 | `Expense.Create` in the record handler | yes | ordinary expense |

## Invariants and integrity

- Plan name, month and amount rules: in `PlannedExpense` (domain).
- Category exists: checked in the handlers (404) and by the foreign key (same schema).
- Recording creates exactly one expense per plan: the plan's delete and the expense insert share one transaction and
  the plan's row version.
- `LimitsFrom` within 2000–2100: in `BudgetSettings.Update`.
- Outside-limits expenses never enter limit spending: one filter in the summary's limit query, tested by SC-OL-003.
