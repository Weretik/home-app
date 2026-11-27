# Domain and infrastructure

## Domain

- `Domain/Budget/Amounts/AmountStep` — `record AmountStep(DateOnly From, decimal Amount)`.
- `Domain/Budget/Amounts/AmountHistory` — an immutable value object over the ordered steps:
  - `static AmountHistory Start(decimal amount)` — one step from `AmountHistory.Beginning` (2000-01-01) (R-LA-001).
  - `static AmountHistory From(IEnumerable<AmountStep>)` — for EF and tests; orders by `From`, merges steps on the
    same day (the last one given wins), refuses an empty list (a programmer error; storage maps `[]` itself).
  - `decimal For(DateOnly periodStart)` — the amount of a period (Terms).
  - `AmountHistory ChangeFrom(DateOnly from, decimal amount)` — R-LA-002 (the caller passes a period start).
  - `IReadOnlyList<AmountStep> Steps` — a read-only view, so the history stays immutable.
  Pure; no EF or ASP.NET Core types.
- `SpendingLimit`:
  - `Amounts` (`AmountHistory`) replaces `Amount`; `AmountFor(DateOnly periodStart)`.
  - `Create(name, period, amount, categoryIds, sortOrder, now)` starts the history (R-LA-001).
  - `Update(name, period, amount, categoryIds, sortOrder, DateOnly? amountFrom, DateOnly today)`: a changed period
    restarts the history (R-LA-004); an explicit `amountFrom` always applies `ChangeFrom(PeriodStart(period,
    amountFrom), amount)` (R-LA-002); without it the period of `today` is used and an amount already in force there
    leaves the steps alone (R-LA-005). The day outside 2000-01-01…2100-12-31 → `BudgetErrors.AmountFromOutOfRange`.
- `BudgetSettings`: `MonthlyLimits` (`AmountHistory`, amount 0 allowed = no limit) replaces `MonthlyLimit`;
  `MonthlyLimitFor(DateOnly month)`; `Update(monthlyLimit, savingsGoal, limitsFrom, DateOnly? monthlyLimitFrom,
  DateOnly today)` with the same explicit/missing rule as the limit.
- `LimitCarryOver.For(period, Func<DateOnly, decimal> amountOf, limitsFrom, periodStart, spentIn)`: each step of the
  walk asks the amount of that period; amount ≤ 0 resets the carry to 0 (R-LA-006). The asked period with amount ≤ 0
  answers (0, 0) as before.

## Application

- Update commands get `DateOnly? AmountFrom` / `DateOnly? MonthlyLimitFrom`; the handler passes them on as given with
  today in Europe/Kyiv through `TimeProvider` (`HouseholdCalendar`, which also knows the older `Europe/Kiev` and the
  Windows `FLE Standard Time` ids) (R-LA-003) — the same way `GetMonthSummaryHandler` finds today. The
  validators check the range (R-LA-003, SC-LA-008).
- `GetBudget` returns `amount` = `AmountFor(PeriodStart(today))` and `amounts` = steps; `monthlyLimit` for the current
  month and `monthlyLimits` (R-LA-008).
- `SummaryFigures.Usage` uses `limit.AmountFor(start)` for the asked period and `limit.AmountFor` in the carry;
  `TotalCarry` and the forecast use `settings.MonthlyLimitFor(month)` (R-LA-006, R-LA-007).
- The digest builds on the summary and needs no change of its own.

## Infrastructure

- `amounts jsonb not null` on `expenses.spending_limits` and `monthly_limits jsonb not null` on
  `expenses.budget_settings`, mapped with a value converter (System.Text.Json, `[{"from":"yyyy-MM-dd","amount":n}]`)
  through an Infrastructure-private stored record with fixed names, and a value comparer over the steps; `[]` reads as
  one step of 0. The steps are read as a whole, never queried by SQL.
- Migration `LimitAmountsByMonth` (R-LA-009, SC-LA-009), expand only (see the [data model](../data-model.md)): add
  the columns with default `'[]'` (kept), fill them with
  `jsonb_build_array(jsonb_build_object('from','2000-01-01','amount', amount))` (and `monthly_limit`), keep the old
  columns unmapped with `default 0`. `Down` writes back the step in force in the current month (else the first step)
  and drops the new columns. A follow-up migration drops the old columns after the release (EN-002).
- `BudgetSeed` and the starting rows of the Budget migration stay as they are (the new migration converts them).
