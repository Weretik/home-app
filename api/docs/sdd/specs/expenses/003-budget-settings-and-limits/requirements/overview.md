# Budget settings and spending limits — overview and scope

## Goal

As Masha or Vitaliy, I want to set how much we may spend on groups of categories per month or quarter, the limit on
the whole month and the jar's goal, so that the summary can show where we stand.

## In scope (implemented)

- Read the budget: settings (`version`, `monthlyLimit`, `savingsGoal`) and every limit in order.
- Change the monthly limit and the savings goal together (`If-Match`).
- Add (`Idempotency-Key`), change (`If-Match`) and delete spending limits.

## Out of scope (at 8378e4a)

- Carry-over of overspend — "overspending is not carried into the next period" (changed by SDD 005).
- History of limit amounts; reordering limits (a new limit goes last, an edit keeps its place).

## Open questions and gaps found while writing this SDD

- [GAP-009] A new limit's `SortOrder` is `max + 10`, read then written without a race guard: two parallel creates get
  the same order. `api/docs/rules.md` asks for a named race guard on shared state. Low impact (ties are broken by name).
  **Resolved 2026-11-26:** unique index `ux_spending_limits_sort_order` (migration `LimitSortOrderUnique`, which
  first renumbers any shared places); the second of two racing creates answers `409` `expenses.limit_added_meanwhile`
  (`ExpensesRaceApiTests.Two_Limits_AddedAtOnce_DoNotShareAPlace_TheSecondIsAConflict`).
- [GAP-010] A limit delete takes no version (`If-Match`), as for entries (001 GAP-001); documented on the module page.
- [GAP-011] A limit's `categoryIds` is a `uuid[]` without a foreign key; categories cannot be deleted today, but a
  future category delete or merge must clean limits (no test or rule guards it).
- [GAP-012] `GetBudget` falls back to empty settings (version 0) when the row is missing, while `UpdateBudgetSettings`
  answers 404 then; with version 0 the web cannot save. Only reachable if the seeded row is deleted.
  **Resolved 2026-11-26:** `GetBudget` answers `404` `expenses.budget_settings_not_found` like the edit
  (`ExpensesSummaryApiTests.Without_TheSettingsRow_TheBudget_IsNotFound_ForReadAndEdit_LikeEachOther`); the summary
  still reads the numbers as 0.
