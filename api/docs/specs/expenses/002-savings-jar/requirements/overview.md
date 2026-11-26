# Savings jar — overview and scope

## Goal

As Masha or Vitaliy, I want to record what we put into and take out of the jar and see its balance by month and
against our goal, so that the «Копилка» tab of the sheet is no longer needed.

## In scope (implemented)

- Create (`Idempotency-Key`), edit (`If-Match`), delete a movement with a signed amount.
- A month's movements, newest first, paged, with the month's deposits, withdrawals, balance at start and end, and the
  balance now.
- All months with movements, newest first, with deposits, withdrawals and closing balance, plus the balance now.
- The savings goal: stored in budget settings (`savingsGoal`, 0 = none) and returned by `GET /api/expenses/budget`
  and in the month summary.

## Out of scope

- Interest, several jars, card balances.
- A stored balance (always computed).

## Actors

- Masha, Vitaliy via the web app. The import (001) may load the sheet's «Копилка» tab.

## Non-functional requirements

| Requirement | Value | Check |
| --- | --- | --- |
| Volume | a few movements a month | months overview reads one grouped row per month |

## Open questions and gaps found while writing this SDD

- [GAP-006] `listSavingsMonths` returns every month without paging. `api/docs/rules.md` ("Reads … page in SQL, page
  size at most 100") is not followed; harmless at today's size (one row per month).
- [GAP-007] Only one API test covers the jar (`Savings_Movements_AddUp_ByMonth`: creates, zero amount, month sums,
  months overview, one edit). Delete, repeated create, stale version and 404 are not tested for movements.
  **Resolved 2026-11-26:** `ExpensesSavingsApiTests.A_Movement_IsRepeatedOnce_RefusedWhenStale_AndDeleted`.
- [GAP-008] The goal is edited on «Настройки» (feature 003) and shown on «Копилка»; there is no endpoint that returns
  the goal with the jar. Not a defect, but the dependency is only in the web.
