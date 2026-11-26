# Budget settings page — frontend design (as implemented)

## File map (implemented, under `web/src/features/expenses/`)

| Path                                                                                        | One responsibility                                                 |
| ------------------------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| `settings-page/{SettingsPage,BudgetSettingsCard,LimitsCard}.tsx`, `budget-settings-form.ts` | the page, settings form, limits list                               |
| `limit-editing/{LimitDialog,LimitEditor}.tsx`, `limit-form.ts`                              | limit form and dialogs                                             |
| `budget-data/{budget-api,budget-writes-api}.ts`                                             | `useBudget()` (`['expenses','budget']`), settings and limit writes |

## State and data

- One query `['expenses','budget']`; categories from `useExpenseCategories()`.
- Writes reuse `useSave`/`useRemove` from `data/writes-api.ts`; every budget write invalidates `['expenses','budget']` (since 2026-11-26: the module prefix `['expenses']`; the settings use `useChange` in `budget-data/budget-writes-api.ts`).

## Navigation

`/expenses/settings` (`settingsPath`), reached from the gear; not in `expensesPages`.

## Test design

`SettingsPage.test.tsx`, `limit-form.test.ts`.
