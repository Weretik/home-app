# 0028. Planned expenses are their own table and are recorded into real expenses

Status: accepted, 2026-11-26

Specification: [SDD 005 — budget forecast, carry-over, expenses outside limits](../specs/expenses/005-budget-forecast-carryover/README.md).

## Context

The family wants to write down expenses they expect this month or later (a gift, an annual insurance) to see where
the month and its limits are heading («Прогноз»). The sheet's «План / Факт / Прогноз» block did this with loose cells.
A plan is not money spent: it must not show in the expense list or the month's totals, but it should count against
the limits of its category in the forecast. When the money is spent, the plan becomes a real expense.

## Decision

- A new aggregate `PlannedExpense` (table `expenses.planned_expenses`): month (first day), name, category (foreign
  key, same schema), payer, amount with the usual money rules, creation time and the `xmin` row version. It has no
  «outside the limits» flag: a plan always counts toward its limits in the forecast; the flag exists only on real
  expenses.
- CRUD under `/api/expenses/planned` with the module's usual conventions: `Idempotency-Key` on create (kind
  `planned`), `If-Match` on edit, delete without a version, `PublicWrite` rate limit.
- `POST /api/expenses/planned/{id}/record` turns a plan into an expense with the plan's payer, category and amount
  on the given date (counted in the limits) and deletes the plan in the same save, guarded by the plan's version.
  It takes no `Idempotency-Key`: a repeat finds the plan gone and answers `404`, so nothing is recorded twice.
- The month summary gets a `forecast` (planned, expected expenses, expected result, expected room left in the monthly
  limit) and each limit a `planned` sum over the months of its period. Plans never change the carry-over
  ([0027](0027-limit-carry-over-like-the-sheet.md)), which is about money actually spent.

## Consequences

- Real figures and expectations stay apart; a plan can be edited or dropped without touching the history.
- A retried «record» after a lost answer gets `404` and the client reloads; the expense exists once.
- Recurring plans (the sheet's «Платежи») are not modelled; they can come later on top of this table.
