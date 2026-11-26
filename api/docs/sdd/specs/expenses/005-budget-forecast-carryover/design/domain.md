# Budget forecast and carry-over — domain design

## Responsibility

- `Expense` (existing, `Domain/Entries/`) gains `OutsideLimits`; `Create` and `Update` take it, default false.
  Existing rules (`EntryRules`) stay unchanged.
- `BudgetSettings` (existing, `Domain/Budget/`) gains `LimitsFrom` (`DateOnly`). `Update(monthlyLimit, savingsGoal,
  limitsFrom)` checks the date range 2000-01-01 … 2100-12-31 and returns `expenses.budget_settings_invalid`
  otherwise; the old values stay on failure.
- `LimitCarryOver` (new static, `Domain/Budget/`) holds the carry arithmetic of R-CO-001 … R-CO-005: the first counted
  period for a `LimitPeriod` and a `LimitsFrom`, the period start, and `(CarriedIn, CarriesOut)` for a period given a
  function that returns spending between two dates. Pure, no EF, unit-tested with the numbers of SC-CO-001 … 006.
- `PlannedExpense` (new aggregate, `Domain/Planning/`): `Id` (UUID v7), `Month` (first day of month), `Name`,
  `CategoryId`, `Payer`, `Amount`, `CreatedAtUtc`, `Version`. `Create` and `Update` check R-FC-001 (name, month,
  money via the existing money rule). Errors in `PlannedExpenseErrors` with stable codes
  (`expenses.planned_name_required`, `expenses.planned_name_too_long`, `expenses.planned_month_invalid`,
  amount codes reused from `EntryErrors`), and a not-found error for unknown plans.
- Recording a plan is an Application use case: it creates an `Expense` through `Expense.Create` and removes the plan;
  no domain event is needed.

The month summary stays a read model in Application (`Features/Summary/Report/`), computed on read
([ADR 0020](../../../../../decisions/0020-month-summary-computed-on-read.md)); it uses `LimitCarryOver` for every limit
and for the monthly total.

## Cross-module interaction

None. Expenses owns all data involved; `Expenses.Contracts` stays empty.

## Risks

- **Concurrency.** Two people editing one plan: guarded by the row version (`xmin`, `If-Match`)
  ([ADR 0017](../../../../../decisions/0017-row-version-via-xmin-for-edits.md)). Recording and editing the same plan at
  once: the record's delete is version-checked inside the same `SaveChanges`, so one of them gets 409 and the record
  creates no expense.
- **Repeat of record.** A lost response followed by a retry answers 404 (the plan is gone). The web must treat 404 on
  record as "already recorded or deleted" and reload both lists. No idempotency key is needed because the plan's
  existence is the state check (rules: "a state check for transitions").
- **Historic data.** Changing `LimitsFrom` or a limit amount re-computes all carries (nothing stored), which is the
  sheet's behavior (R-CO-005).
- **Rule change of ADR 0019.** "Overspending is not carried into the next period" in ADR 0019 and the module page
  becomes false; a new ADR records the change (TS-015).
