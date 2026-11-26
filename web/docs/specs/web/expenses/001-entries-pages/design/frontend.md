# Expenses and incomes pages — frontend design (as implemented)

## Clients and boundaries

- `web/src/features/expenses` (one feature folder per backend module); routes in `src/app/root/App.tsx`.
- API only through `src/platform/api.ts`, called from `data/*-api.ts`.

## Architecture baseline

| Area                           | Evidence                                     | Rule                                                                                                                                                   | State |
| ------------------------------ | -------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ | ----- |
| Feature isolation, folder size | `npm run architecture:structure`             | [web rules, Structure](../../../../../rules.md#structure)                                                                                              | holds |
| API access                     | `data/expenses-api.ts`, `data/writes-api.ts` | [web rules, API](../../../../../rules.md#api)                                                                                                          | holds |
| Writes                         | `useIdempotencyKey`, `useSave`, `useRemove`  | [web rules, Writes](../../../../../rules.md#writes), [web ADR 0007](../../../../../decisions/0007-editing-in-dialogs-with-idempotency-and-versions.md) | holds |
| Tabs and URL state             | `section/`, `url-state/`                     | [web ADR 0006](../../../../../decisions/0006-section-pages-as-tabs-with-state-in-the-url.md)                                                           | holds |

## File map (implemented, under `web/src/features/expenses/`)

| Path                                                                                   | One responsibility                                             |
| -------------------------------------------------------------------------------------- | -------------------------------------------------------------- |
| `section/{ExpensesSection,MonthPicker,SectionTabs}.tsx`, `expenses-pages.ts`           | section frame, tabs list                                       |
| `url-state/{use-month-param,use-period-param,use-filter-param,period}.ts`              | month, period and filters in the URL; period checks            |
| `data/{expenses-api,writes-api,formatting,labels,month}.ts`                            | reads, writes and key hook, formatting, Russian labels, months |
| `month-view/{FilterSelect,ListStatus,LoadMoreButton}.tsx`                              | parts shared by month pages                                    |
| `entry-editing/{entry-fields,use-entry-form,use-entry-editor}.ts`                      | field checks, form state, open-dialog state                    |
| `entry-dialogs/{EntryDialog,DeleteEntryDialog,RowActions,AmountField,PersonField}.tsx` | dialog frame, confirmation, row menu, fields                   |
| `expense-list/{ExpensesPage,ExpenseFilters,PeriodFilter,ExpenseList,ExpenseTable}.tsx` | «Расходы»                                                      |
| `expense-editing/{ExpenseDialog,ExpenseEditor}.tsx`, `expense-form.ts`                 | expense form                                                   |
| `income-list/{IncomesPage,IncomeList,IncomeTable}.tsx`                                 | «Доходы»                                                       |
| `income-editing/{IncomeDialog,IncomeEditor}.tsx`, `income-form.ts`                     | income form                                                    |

(The working tree is moving the generic write hooks to `write-data/` as part of web 005.)

## State and data

- Keys: `['expenses','list',days,filter]`, `['expenses','incomes',month,filter]`, `['expenses','categories']`
  (`staleTime` 5 min). `useInfiniteQuery`, 50 per page.
- Invalidation after a write: the kind's key only (GAP-W01); since 2026-11-26 the module prefix `['expenses']` ([web ADR 0009](../../../../../decisions/0009-writes-refresh-their-backend-module.md)).

## Navigation and deep links

`/expenses?year&month&category&payer[&from&to]`, `/expenses/incomes?year&month&person`. Invalid values fall back to
the current month; an invalid period shows the reason.

## Visual system

Theme tokens only, both schemes; list on phones, table on computers ([design rules](../../../../../design.md)).

## Test design

Vitest for pure logic (`entry-fields`, `period`, `month`, `formatting`); Testing Library page tests with a mocked
client (`ExpensesPage.test.tsx`, `IncomesPage.test.tsx`). No E2E (GAP-W04).

## Chosen approach and significant tools

No feature-specific packages beyond the app stack ([web ADR 0002](../../../../../decisions/0002-react-spa-mui-react-query.md)).
