# 0020. The month summary is computed on read, with no stored totals

Status: accepted, 2026-11-26; extended by [0027](0027-limit-carry-over-like-the-sheet.md) (carry-over, which reads
back to the limits start day) and [0028](0028-planned-expenses-for-the-forecast.md) (the forecast)

## Context

The sheet's «Сводка» tab shows one month at a glance: incomes by person, expenses by payer, the result after the
savings jar, the jar's balance and goal, each limit with what is left and what may be spent per day, and the top
categories. The site's home page needs the same. The figures depend on every expense, income and jar movement of the
month (and of the quarter for quarterly limits), all of which can be added, edited or deleted at any time.

## Decision

- `GetMonthSummary` (`GET /api/expenses/summary?year&month`) computes the summary on every request: grouped sums in
  SQL over expenses, incomes and jar movements, then the limit usage, per-day allowance and top five categories in
  memory. No totals, snapshots or caches are stored.
- `result` = incomes − expenses − the jar's net movement of the month, as in the sheet.
- A monthly limit is measured over the month, a quarterly one over the calendar quarter that holds the month.
- "Today", used for the days left and the per-day allowance, is the current date in `Europe/Kyiv` (from the injected
  `TimeProvider`), so the figures change at the household's midnight, not at UTC midnight. The per-day allowance is
  `null` for a period that is not running now.
- The sheet's «План / Факт / Прогноз» block is not reproduced; a plan would arrive with the «План» tab as its own
  decision. The result was compared with the sheet for November 2026.

## Consequences

- The summary can never disagree with the entries: an edit of an old expense shows at once.
- Each request runs a handful of grouped queries over at most a quarter of rows (the jar balance sums the whole jar
  table, see [0018](0018-savings-jar-as-signed-movements.md)); for two people that stays small for years. If it ever
  does not, monthly totals can be added behind the same endpoint.
- The answer depends on the day it is asked; a test of the handler sets the day through a fake `TimeProvider`.
