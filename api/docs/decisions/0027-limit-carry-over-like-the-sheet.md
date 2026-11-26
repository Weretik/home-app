# 0027. Overspend carries over like the sheet, computed on read from a start day

Status: accepted, 2026-11-26

Specification: [SDD 005 — budget forecast, carry-over, expenses outside limits](../specs/expenses/005-budget-forecast-carryover/README.md).

## Context

The old «Бюджет» sheet does not forget overspend: its column «Перенос с прошлого мес.» takes what a limit went over
in one month from the next month, starting at the day in «Справочник!H2» (2026-11-01). Since phase 3 the site
measured every period on its own ([0020](0020-month-summary-computed-on-read.md); [0019](0019-spending-limits-as-category-sets.md)
said overspend is not carried, which this record supersedes), so a big month looked fine again
on the first of the next one. The family also makes rare, deliberate one-off spends (a fridge, a trip) that should
show in the month's totals but must not eat the limits or roll into the following months.

## Decision

- A limit's carry follows the sheet's formula exactly. Starting with the first counted period,
  `carry(next) = max(0, carry + spent − limit)`. For the period asked about, `carriedIn` is the carry,
  `left = amount − spent − carriedIn`, `percent = round((spent + carriedIn) / amount × 100)` and
  `carriesOut = max(0, spent + carriedIn − amount)`. Unspent money never carries forward; it only pays off earlier
  overspend.
- Monthly limits step by month, quarterly limits by quarter. The first counted period is the first month that starts
  on or after `BudgetSettings.LimitsFrom`, or the quarter that holds it; earlier periods carry nothing (both figures
  are 0 there). The current amount of a limit is used for past periods too, as in the sheet.
- The monthly limit on all expenses («ИТОГО») carries the same way over months, using every expense the limits count.
  With no monthly limit (0) both figures are 0.
- `LimitsFrom` is a new column of the one settings row, seeded with 2026-11-01 and editable through
  `PUT /api/expenses/budget/settings` (left out: unchanged).
- An expense can be marked `OutsideLimits` («Не в лимиты»). It counts in the month's expenses, by payer, in the top
  categories and in the result, but in no limit, not in the monthly limit and not in any carry. The summary reports
  how many such expenses the month has and their sum.
- Nothing new is stored. `GetMonthSummary` reads, in one grouped query, the limit-counted spending per category and
  month from the quarter that holds `LimitsFrom` up to the end of the asked month's quarter, and walks the carry in
  memory (`LimitCarryOver` in `Domain/Budget`, a pure function tested on its own).

## Consequences

- The summary still cannot disagree with the entries: editing an expense months ago changes the carry of every later
  month at once, as it does in the sheet.
- The grouped read now spans the months since `LimitsFrom` instead of one quarter: one row per category and month,
  about 14 × 12 rows a year. Fine for years; monthly totals can be stored behind the same endpoint if it ever is not.
- Changing a limit's amount rewrites its past carry too (the sheet does the same). Keeping history of limit amounts
  would be its own decision.
- Moving `LimitsFrom` forward is the way to start over, for example after a period the family agrees to forget.
