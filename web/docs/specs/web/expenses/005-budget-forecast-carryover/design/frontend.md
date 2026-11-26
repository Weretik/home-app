# Budget forecast and carry-over (web) — frontend design

## Clients and boundaries

- Client: `web/` (one Vite React SPA, [web ADR 0001](../../../../../decisions/0001-single-vite-app-without-nx.md)).
- Variant: React Web, see [react.md](react.md).
- Features touched: `src/features/expenses` (section, list, dialogs, new forecast tab), `src/features/home` (expenses
  summary), `src/app/shell/sections.ts` and `src/app/root/App.tsx` (name and route).
- Public API boundary: the generated client `src/api-client/generated/api.ts` via `src/platform/api.ts`, called only
  from `*-api.ts` files of the feature.
- Shared behavior: none across features — `home` keeps its own labels and formatters (features never import each other).

## Architecture baseline

| Area              | Evidence                                   | Rule                                                                                              | Target                                                                                                           | Finding                                                                              |
| ----------------- | ------------------------------------------ | ------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------ |
| Feature isolation | `npm run architecture:structure`           | [web rules, Structure](../../../../../rules.md#structure)                                         | `home` and `expenses` do not import each other                                                                   | —                                                                                    |
| Folder size       | same check                                 | ≤ 6 production files a folder, files or subfolders not both                                       | new folders `planned-data/`, `planned-editing/`, `forecast-page/`; home `limits/` split from `view/` and `data/` | —                                                                                    |
| API access        | `src/platform/api.ts`                      | [web rules, API](../../../../../rules.md#api)                                                     | only `*-api.ts` call the client                                                                                  | —                                                                                    |
| Server state      | React Query                                | [web ADR 0002](../../../../../decisions/0002-react-spa-mui-react-query.md)                        | every write invalidates its module's prefix                                                                      | [web ADR 0009](../../../../../decisions/0009-writes-refresh-their-backend-module.md) |
| Writes            | `useSave`/`useRemove`, `useIdempotencyKey` | [web ADR 0007](../../../../../decisions/0007-editing-in-dialogs-with-idempotency-and-versions.md) | plans follow the same writer pattern                                                                             | —                                                                                    |

## Audit of existing code that the feature changes

| File                                                                                             | Current roles                                                          | Keep or split                                                                                                | Target                                                                           | Task   |
| ------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------ | -------------------------------------------------------------------------------- | ------ |
| `features/expenses/data/writes-api.ts`                                                           | writers of three kinds, save/remove hooks, idempotency key, error text | split: the generic hooks move to their own folder so plans reuse them without growing `data/` past six files | `features/expenses/write-data/write-hooks-api.ts` (+ kinds stay with their data) | TS-003 |
| `features/home/expenses-summary/data/limit-rows.ts`, `view/LimitsCard.tsx`, `view/LimitFill.tsx` | rows + «ИТОГО», table/list card                                        | move to `expenses-summary/limits/` with the notes, list and table (seven files would overflow `view/`)       | `home/expenses-summary/limits/`                                                  | TS-007 |
| `features/expenses/expense-editing/ExpenseDialog.tsx`, `expense-form.ts`                         | expense form                                                           | keep; add the checkbox and field                                                                             | same                                                                             | TS-002 |
| `features/expenses/section/expenses-pages.ts`                                                    | tab list                                                               | keep; add «Прогноз»                                                                                          | same                                                                             | TS-006 |

## Planned file map

| Path (under `web/src/`)                                                                                                            | State                      | One responsibility                                                  | Task           |
| ---------------------------------------------------------------------------------------------------------------------------------- | -------------------------- | ------------------------------------------------------------------- | -------------- |
| `app/shell/sections.ts`, `features/expenses/section/ExpensesSection.tsx`, `features/home/page/summary-sections.ts`                 | existing                   | name «Бюджет»                                                       | TS-001         |
| `api-client/generated/api.ts`                                                                                                      | existing (generated)       | regenerated contract                                                | EN-001         |
| `features/expenses/expense-editing/{ExpenseDialog.tsx,expense-form.ts,expense-form.test.ts}`                                       | existing / planned test    | «Не в лимиты» field                                                 | TS-002         |
| `features/expenses/expense-list/OutsideLimitsChip.tsx`                                                                             | planned                    | the chip                                                            | TS-002         |
| `features/expenses/write-data/`                                                                                                    | planned                    | shared save/remove hooks, idempotency key, write error text         | TS-003         |
| `features/expenses/planned-data/{planned-api.ts,planned-writes-api.ts}`                                                            | planned                    | plan reads (`['expenses','planned',month]`), plan writes and record | TS-003         |
| `features/expenses/planned-editing/{planned-form.ts,PlannedDialog.tsx,PlannedEditor.tsx}` (+ tests)                                | planned                    | plan form values, checks, request; add/edit/delete dialogs          | TS-004         |
| `features/expenses/planned-editing/{record-form.ts,RecordDialog.tsx}` (+ test)                                                     | planned                    | «Записать как расход» form and dialog                               | TS-005         |
| `features/expenses/entry-dialogs/CategoryField.tsx`                                                                                | planned                    | category select shared by expense and plan dialogs                  | TS-004         |
| `features/expenses/forecast-page/{ForecastPage.tsx,PlannedList.tsx,PlannedTable.tsx,planned-rows.tsx}` (+ `ForecastPage.test.tsx`) | planned                    | the tab: total, phone list, computer table                          | TS-006         |
| `app/root/App.tsx`                                                                                                                 | existing                   | route `/expenses/forecast`                                          | TS-006         |
| `features/home/expenses-summary/limits/{LimitsCard,LimitList,LimitTable,LimitNotes,LimitFill}.tsx`, `limit-rows.ts` (+ test)       | existing (moved) / planned | limit rows with carry and plan notes                                | TS-007         |
| `features/home/expenses-summary/data/figure-lines.ts` (+ test)                                                                     | planned                    | outside-limits line and forecast lines (pure)                       | TS-008, TS-009 |
| `features/home/expenses-summary/{figures/KpiCards.tsx,figures/ForecastCard.tsx,overview/ExpensesSummary.tsx}`                      | existing / planned         | KPI line and the «Прогноз» card                                     | TS-008, TS-009 |

## State and data

- Server state only in React Query. Keys: plans `['expenses', 'planned', month]`; summary `['expenses', 'summary', month]`.
- Mapping: generated types (`Schema<'PlannedExpenseItem'>` etc.) to form values in `planned-form.ts` /
  `record-form.ts`; amounts typed as text and parsed by the shared entry rules.
- Loading, empty, error and retry states reuse `month-view/ListStatus.tsx`.
- Invalidation: every write (plans, record, expenses, budget) invalidates the module prefix `['expenses']`, the
  summary included ([web ADR 0009](../../../../../decisions/0009-writes-refresh-their-backend-module.md); audit AF-01).
  Record and the budget settings use `useChange` (no create or delete).

## Navigation and deep links

- `/expenses/forecast?year=&month=`; unknown month values fall back to the current month like the other tabs.
- Switching tabs keeps only the month.

## Visual system and responsiveness

- Theme tokens and card patterns of [design rules](../../../../../design.md); light and dark schemes.
- Below `md` a compact list, from `md` a table (as other tabs). Notes under limit rows wrap with « · ».
- Risk: long plan names and amounts at 360 px — names wrap, amounts use tabular numbers; checked in TS-010.

## API integration

See [contract](../contracts/api-contract.md).

## Test design

- Pure logic with Vitest: `planned-form`, `record-form`, `expense-form`, `limit-rows`, `figure-lines`.
- Pages with Testing Library and a mocked client: `ForecastPage.test.tsx`, `ExpensesPage.test.tsx`, `HomePage.test.tsx`.
- No E2E harness exists in the project; not added for this feature (main path covered by page tests).

## Allowed tools and forbidden workarounds

- Allowed: React Query, MUI, the generated client; `npm run contracts:generate` after the backend change.
- Forbidden: raw `fetch`, editing `generated/`, importing `features/expenses` from `features/home`, computing
  carry-over or forecast in the browser.

## Chosen approach and significant tools

No new approach or package.
