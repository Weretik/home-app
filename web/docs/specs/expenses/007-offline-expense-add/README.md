# 007 — Add an expense offline (web)

- **Application type:** React Web (Vite SPA, MUI, React Query), installable PWA
- **Application:** `web/`
- **Project architecture source:** [web architecture](../../../architecture.md), [web rules](../../../rules.md), [design rules](../../../design.md)
- **Domain:** expenses, page «Витрати» of the «Бюджет» section
- **Status:** implemented, audited
- **Owner:** Vitaliy
- **Created:** 2026-11-28
- **Backend counterpart:** none. `POST /api/expenses` already takes an `Idempotency-Key`.
- **Decision:** [web ADR 0022](../../../decisions/0022-offline-cache-and-queued-writes.md), amended: adding an expense
  is a queued write.
- **Why:** finding F4 of the [2026-11-27 architecture review](../../../../../docs/reviews/2026-11-27-architecture-before.md),
  quality target OFF2 ([quality targets](../../../../../api/docs/architecture/quality-targets.md)).

An expense is usually recorded in a shop, where the network is often weak. Until now an expense added without a
network failed with «Немає звʼязку…» and was lost once the dialog closed. Shopping and to-dos already queue their
writes (web ADR 0022). This change gives the expense add the same queue.

## Requirements

| Id       | Requirement                                                                                                                                                                                  |
| -------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| R-OEA-01 | A new expense saved without a network is kept on the device and sent by itself when the network is back, also after the app was closed and opened again.                                     |
| R-OEA-02 | It is sent exactly once: the request keeps the form's `Idempotency-Key`, so a repeat after a lost answer adds nothing twice.                                                                 |
| R-OEA-03 | Offline, the dialog closes at once and the page says «Витрата додасться, коли з’явиться мережа».                                                                                             |
| R-OEA-04 | Until it is sent, the expense is shown at once above the list under «Очікують мережі» (category, amount, date). It goes away once it is sent, and the list then shows the saved expense.     |
| R-OEA-05 | Online, an add behaves as before: the dialog shows a refusal (`400`, `409`, `429`) and keeps the data. A server error (`5xx`) is sent again by itself, up to three times, with the same key. |
| R-OEA-06 | A refusal of an add made offline is shown in the top bar «Не вдалося зберегти: витрату 120,00 ₴» (the queue's existing snackbar).                                                            |
| R-OEA-07 | Edits and deletes of an expense, incomes and the jar stay online-only. An edit sent later could overwrite a newer change, and these writes are rare on the go.                               |

## Scenarios

| Id        | Given                          | When                               | Then                                                                                             | Test                                                                                                                              |
| --------- | ------------------------------ | ---------------------------------- | ------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------- |
| SC-OEA-01 | no network                     | the member adds «Дом», 120         | the dialog closes, the snackbar shows, «Очікують мережі» lists «Дом · 120,00 ₴», nothing is sent | `ExpensesPage.test.tsx` «offline, an add closes the dialog…»                                                                      |
| SC-OEA-02 | SC-OEA-01, then network back   | —                                  | one `POST /api/expenses` with the `Idempotency-Key`; the list reloads; «Очікують мережі» is gone | the same test                                                                                                                     |
| SC-OEA-03 | online, the server answers 500 | the member saves                   | the add is sent again by itself with the same key; the dialog closes on success                  | «a server error on an add is sent again by itself with the same key»                                                              |
| SC-OEA-04 | online, the server answers 429 | the member saves, then saves again | the dialog shows «Забагато змін поспіль…»; the second save sends the same key                    | «adds an expense with an Idempotency-Key and keeps the key when it is sent again»                                                 |
| SC-OEA-05 | an offline add, app closed     | the app opens with the network     | the paused write is restored from IndexedDB and sent (the queue's defaults, ADR 0022)            | `query-persistence.test.ts` (paused queued writes are kept) and the queue's mutation defaults (ADR 0022); manual check on a phone |

## Design

- `data/writes-api.ts`:
  - `addExpenseRequest(input)` builds the serialisable `QueuedRequest`: `POST /api/expenses`, the body, the
    `Idempotency-Key` header, module `expenses`, label «витрату 120,00 ₴».
  - `useSaveExpense(isEdit)` returns a `Saver`: for an edit the existing plain `useSave(expenseWriter)`, for an add
    `useQueuedWrite(addExpenseRequest)`. `isPending` is the queued write's `isSaving`, so the form does not hang
    while the write waits offline.
  - `usePendingExpenses()` reads `useMutationState` for the paused queued writes to `POST /api/expenses`. They are
    persisted with the cache, so they are listed after a restart too.
- `write-data/write-hooks-api.ts`: the `Saver<Body>` shape (`mutate`, `isPending`, `error`), which both a plain
  mutation and a queued write satisfy.
- `entry-editing/use-entry-form.ts`: takes a `Saver` and an optional `onQueued`. When `onQueued` is set and the app is
  offline (`isOnline()`), it is called right after `mutate`, so the dialog closes at once.
- `expense-editing/ExpenseDialog.tsx`, `ExpenseEditor.tsx`: pass `onQueued` for an add only. The editor closes the
  dialog and tells the page.
- `expense-list/ExpensesPage.tsx`: the snackbar and `<PendingExpenses />` above the list.
- `expense-list/PendingExpenses.tsx`: the «Очікують мережі» card. Each pending expense shows its category name (from
  the cached categories), amount and date. The card is not rendered when nothing waits.

No new dependency, no contract change. The refresh after the write is the queue's default: it invalidates
`moduleKeys.expenses` (ADR 0009).

## File map (as built)

Paths are relative to `web/src/features/expenses/`.

| Path                                                                | State          | One responsibility                                                     | Task   |
| ------------------------------------------------------------------- | -------------- | ---------------------------------------------------------------------- | ------ |
| `data/writes-api.ts`                                                | existing       | the queued add request, `useSaveExpense(isEdit)`, `usePendingExpenses` | TS-001 |
| `write-data/write-hooks-api.ts`                                     | existing       | the `Saver` shape                                                      | TS-001 |
| `entry-editing/use-entry-form.ts`                                   | existing       | `onQueued` when a queued write is made offline                         | TS-001 |
| `expense-editing/ExpenseDialog.tsx`                                 | existing       | the queued saver and `onQueued` for an add                             | TS-001 |
| `expense-editing/ExpenseEditor.tsx`                                 | existing       | closes the dialog and tells the page                                   | TS-001 |
| `expense-list/ExpensesPage.tsx`                                     | existing       | the snackbar and the pending card                                      | TS-002 |
| `expense-list/PendingExpenses.tsx`                                  | new            | «Очікують мережі»                                                      | TS-002 |
| `expense-list/ExpensesPage.test.tsx`                                | existing       | SC-OEA-01 … 04                                                         | TS-003 |
| docs (this folder, ADR 0022, architecture, specs index, the review) | existing / new | —                                                                      | TS-004 |

## Tasks

| Task   | Work                                                    | Evidence                                           |
| ------ | ------------------------------------------------------- | -------------------------------------------------- |
| TS-001 | queued add of an expense, the `Saver` shape, `onQueued` | `npm run typecheck`                                |
| TS-002 | snackbar and «Очікують мережі»                          | `npm run lint`, `npm run architecture:structure`   |
| TS-003 | page tests                                              | `npx vitest run src/features/expenses`: 118 passed |
| TS-004 | docs                                                    | links checked                                      |

Whole web suite on 2026-11-28: 124 files, 610 tests passed, typecheck, lint and the structure check clean.

## Architecture audit

| Check                       | Result                                                                                                                                                                                                                                                                                                                        |
| --------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Layers and boundaries       | ✅ The feature reaches the queue through `platform/offline` (allowed: features → platform) and the API only through the queue's single client. No import from another feature.                                                                                                                                                |
| Folders ≤ 6 code files      | ✅ `expense-list/` is at 6 (the next file splits it), `data/` at 6, `expense-editing/` at 3, `entry-editing/` at 3, `write-data/` at 2.                                                                                                                                                                                       |
| File size                   | ✅ Largest are `ExpenseDialog.tsx` 136 / 150 and `writes-api.ts` 136 / 200. The next field in the dialog moves the payment and payer selects into their own component.                                                                                                                                                        |
| One responsibility per file | ✅ The pending card is its own component; the data hook stays in `*-api.ts`.                                                                                                                                                                                                                                                  |
| Idempotency                 | ✅ The request keeps the form's `Idempotency-Key` across the pause, the restart and the retries; the server's unique index on the key does the rest.                                                                                                                                                                          |
| Conflicts                   | ✅ A create has no version. Edits are not queued (R-OEA-07), so no stale `If-Match` is sent later.                                                                                                                                                                                                                            |
| Contract                    | ✅ No change.                                                                                                                                                                                                                                                                                                                 |
| Texts                       | ✅ Ukrainian, with the typographic apostrophe used across the app.                                                                                                                                                                                                                                                            |
| Known limits                | The pending expense is not counted in the month's totals or the home summary until it is sent; the card says it is waiting. A write that loses the network mid-request while the app still counts itself online leaves the dialog open with the button enabled; a second press sends the same key, so nothing is added twice. |

## Change history

- 2026-11-28 — created, implemented and audited.
