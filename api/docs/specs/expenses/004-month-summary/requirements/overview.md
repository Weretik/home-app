# Month summary — overview and scope

## Goal

As Masha or Vitaliy, I want to open a month and see what came in, what went out, how much is left in each limit and
per day, and where the jar stands, so that the sheet's «Сводка» tab is no longer needed.

## In scope (implemented)

- `GET /api/expenses/summary?year&month` for any month 2000–2100.

## Out of scope (at 8378e4a)

- The sheet's «План / Факт / Прогноз» block (added in part by SDD 005 as the forecast).
- Carry-over of overspend (SDD 005).
- Stored totals.

## Non-functional requirements

| Requirement | Value | Check |
| --- | --- | --- |
| Queries | a fixed number of grouped SQL queries per request (7), none per limit | code review of `GetMonthSummaryHandler` |
| "Today" | the date in `Europe/Kyiv` | handler; falls back to UTC if the time zone is missing |

## Open questions and gaps found while writing this SDD

- [GAP-013] When `Europe/Kyiv` is not found the handler silently uses the UTC date; per-day figures can shift by a
  day between 00:00 and 02:00/03:00 Kyiv time on such a host. No log line or test covers the fallback.
- [GAP-014] The «ИТОГО» row (all expenses against `monthlyLimit`, its percent and per day) is not in the API response:
  the web computes it (`home/expenses-summary/.../limit-rows.ts`). The same rules (rounding, per day never below 0)
  exist twice, in C# for limits and in TypeScript for the total. SDD 005 moves the total's carry-over into the API;
  consider returning the whole total row.
- [GAP-015] Only one integration test (`The_MonthSummary_AddsUp_LikeTheSheet`) covers the summary; quarterly usage,
  `daysLeft`/`perDay` for the current month and `null` outside it, and top-5 ordering have no dedicated test.
  **Resolved 2026-11-26:** the arithmetic moved into `GetMonthSummary/SummaryFigures` and is unit-tested with a fixed
  day (`SummaryFiguresTests`: quarterly usage, `daysLeft`, `perDay` in and outside the period, carry, forecast); invalid
  months in `MonthSummaryValidatorTests` and `ExpensesSummaryApiTests`.
