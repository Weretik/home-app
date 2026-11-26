# Savings page — frontend design (as implemented)

## Boundaries

`web/src/features/expenses/{savings-page,savings-editing,savings-goal}`, data in `data/savings-api.ts` and
`budget-data/budget-api.ts` (the goal). Rules as in web 001 ([web rules](../../../../../rules.md)).

## File map (implemented, under `web/src/features/expenses/`)

| Path                                                                                                   | One responsibility                                                                 |
| ------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------- |
| `savings-page/{SavingsPage,BalanceCard,MovementList,MovementTable,SavingsMonthsCard,SignedAmount}.tsx` | the tab                                                                            |
| `savings-editing/{SavingsDialog,SavingsEditor}.tsx`, `savings-form.ts`                                 | direction + positive amount → signed request                                       |
| `savings-goal/{GoalCard,GoalProgressView}.tsx`, `goal-progress.ts`                                     | «План копилки»                                                                     |
| `data/savings-api.ts`                                                                                  | `useSavingsMovements(month)`, `useSavingsOverview()`, key `['expenses','savings']` |

## State and data

- Movements: `useInfiniteQuery` `['expenses','savings','movements',month]`; months `['expenses','savings','months']`;
  goal from `useBudget()` (`['expenses','budget']`).
- Savings writes invalidate `['expenses','savings']`; the home summary is not invalidated (web 001 GAP-W01). Since 2026-11-26 every write invalidates the module prefix `['expenses']`.

## Navigation

`/expenses/savings?year=&month=`; a month in the overview sets `?year=&month=`.

## Test design

`SavingsPage.test.tsx`, `savings-form.test.ts`, `goal-progress.test.ts`.
