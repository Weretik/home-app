# Frontend design

## Clients and boundaries

- Client: `web/` only; variant `react` ([react.md](react.md)). No backend code is touched (another change owns `api/`).
- Feature `src/features/shopping` (new) and the summary `src/features/home/shopping-summary` (new); the route in
  `src/app/root/App.tsx` and one registry entry in `src/features/home/page/summary-sections.ts` change.
- The two features never import each other (the summary duplicates the few labels it needs, web ADR 0008).

## Architecture baseline

| Area            | Evidence                                        | Rule                                            | Target                                                                         |
| --------------- | ----------------------------------------------- | ----------------------------------------------- | ------------------------------------------------------------------------------ |
| Feature folders | `src/features/expenses`                         | rules.md Structure, `tools/check-structure.mjs` | role folders ≤6 files, files or subfolders                                     |
| API access      | `src/platform/api.ts`, `expenses/data/*-api.ts` | rules.md API, ADR 0003                          | only `*-api.ts` call `api` through `apiCall`                                   |
| Server state    | React Query in Expenses                         | ADR 0002, rules.md App                          | React Query; no component copies of server data                                |
| Page state      | `?year=&month=` hooks                           | ADR 0006                                        | tabs as nested routes; no URL filters needed except history paging (in-memory) |
| Writes          | `expenses/data/writes-api.ts`                   | ADR 0007, rules.md Writes                       | `Idempotency-Key` on add, `If-Match` on edit / delete, pending disables        |
| Line limits     | none before this change                         | task requirement                                | `check-structure.mjs` also checks `.tsx` ≤150, other `.ts` ≤200 lines (EN-1)   |

## Existing code touched

| File                                                                 | Today                                          | Decision                                                                                                         |
| -------------------------------------------------------------------- | ---------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- |
| `src/app/root/App.tsx`                                               | `/shopping` → `ComingSoonPage`                 | replace with the section and three nested routes                                                                 |
| `src/features/home/page/summary-sections.ts`                         | `shopping` with `Summary: null`                | point at `ShoppingSummary` (one entry)                                                                           |
| `src/features/expenses/data/writes-api.ts` (212 lines)               | writers of three kinds, hooks, key, error text | the jar's writer moves next to its reads in `data/savings-api.ts`; `useSave` / `useRemove` share one hook (EN-1) |
| `src/features/home/expenses-summary/view/LimitsCard.tsx` (172 lines) | card + phone list + table                      | new `expenses-summary/limits/`: `LimitsCard`, `LimitList`, `LimitTable`, `LimitFill`, `limit-format.ts` (EN-1)   |

## Planned file map (`src/features/shopping`)

```
data/                 shopping-api.ts      list (20 s polling), frequent, department match; query keys; types
                      item-writes-api.ts   add (Idempotency-Key), update / delete (If-Match), clear cart, undo add,
                                           the shared write hook, useIdempotencyKey
                      status-api.ts        optimistic status toggle (cancel, snapshot, rollback, refetch)
                      item-status.ts       pure: apply a status change to the cached list
                      write-failure-api.ts Russian text of a failed write, the stale check
                      labels.ts            person labels, item caption, dates and times
section/              ShoppingSection.tsx  title, counts, «Я», tabs, gear, <Outlet />
                      SectionTabs.tsx      Список / История tabs and the «Настройки» gear
                      shopping-pages.ts    the page list and paths
                      MeToggle.tsx         «Я: Виталий / Маша»
                      use-me.ts            the device's person (localStorage in try/catch, shared store)
quick-add/            QuickAdd.tsx         sticky field, Срочно flag, Enter adds
                      DepartmentPreview.tsx the matched department of a single item
                      FrequentChips.tsx    ⭐ Частые row
                      UndoSnackbar.tsx     «Добавлено: … · Отменить»
                      use-quick-add.ts     add + undo controller
                      use-debounced-value.ts
list-page/            ListPage.tsx         the page: quick add, groups, cart, states
                      DepartmentGroup.tsx  one department card
                      ItemRow.tsx          checkbox row (urgent, quantity, note, who)
                      CartCard.tsx         «В корзине (N)», «Убрать купленные»
                      list-groups.ts       pure: groups in store order, cart order, counts
item-editing/         ItemDialog.tsx       edit dialog + delete confirmation
                      ItemFields.tsx       name, quantity, note, department, Срочно
                      item-form.ts         pure: values ↔ request, checks
page-parts/           FormDialog.tsx, ConfirmDialog.tsx, RowMenu.tsx, PageStatus.tsx (loading / error / empty),
                      use-checked-form.ts (values, own checks and API field errors of a form)
history-page/         HistoryPage.tsx, HistoryDay.tsx, history-days.ts (pure day grouping), history-api.ts,
                      use-return-item.ts («Вернуть в список» controller)
settings-page/        SettingsPage.tsx, ListSettingsCard.tsx, list-settings-form.ts, settings-api.ts
department-settings/  DepartmentsCard.tsx, DepartmentRow.tsx, DepartmentDialog.tsx, department-form.ts,
                      department-order.ts (pure move up / down), departments-api.ts
keyword-settings/     KeywordsCard.tsx, KeywordFilters.tsx, KeywordRows.tsx, KeywordDialog.tsx, keyword-form.ts,
                      keywords-api.ts
```

Home summary (`src/features/home/shopping-summary`):

```
data/   shopping-summary-api.ts (GET /api/shopping/summary), summary-labels.ts (item caption, counts)
view/   ShoppingSummary.tsx (states, layout, «Открыть список»), SummaryKpis.tsx, DepartmentsCard.tsx,
        NextItemsCard.tsx, FrequentCard.tsx
```

Tests next to the code: `item-status`, `list-groups`, `history-days`, `item-form`, `department-order`,
`list-settings-form`, `use-me`, `ListPage`, `HistoryPage`, `SettingsPage`, `ShoppingSummary`.

## State and data

- Query keys under `['shopping', …]`: `list`, `frequent`, `match`+name, `history`, `departments`, `keywords`+filter,
  `settings`; the summary uses `['shopping', 'summary']`. Item writes invalidate `['shopping']` (list, frequent,
  history, departments counts, summary) — a small app, one prefix is simpler than per-key bookkeeping.
- List polling: `staleTime: 0` (a return to the tab always reloads), `refetchInterval` 20 s unless a status toggle is pending; `refetchIntervalInBackground` false (only
  while visible); `refetchOnWindowFocus` true.
- Optimistic toggle: `onMutate` cancels list queries, keeps the snapshot, writes `withStatus(...)`; `onError` restores
  it; `onSettled` invalidates. Mutation key `['shopping', 'status']`.
- «Я»: a tiny external store (`useSyncExternalStore`) over `localStorage` key `home.shopping.me`, every access in
  `try/catch`, default `Vitaliy`.
- Page state: the open tab is the route; edit dialogs and the undo snackbar are local component state.

## Navigation

- `/shopping` (index, Список), `/shopping/history`, `/shopping/settings` (gear, not a tab, design.md). Unknown paths
  keep the app's redirect to `/`.
- Home: «Открыть список» links to `/shopping`.

## States

| Page      | Loading                   | Empty                                                    | Error                                       | Offline / stale                       |
| --------- | ------------------------- | -------------------------------------------------------- | ------------------------------------------- | ------------------------------------- |
| Список    | spinner «Загружаю список» | «Список пуст 🎉» + hint about the field and ⭐           | reason + «Повторить»                        | last list kept + warning «Нет связи…» |
| История   | spinner                   | «Истории пока нет»                                       | reason + «Повторить»                        | same as list                          |
| Настройки | spinner per card          | «Слов не найдено» / no departments impossible (fallback) | per card                                    | per card                              |
| Summary   | spinner                   | «Список пуст 🎉» under the KPI cards                     | «Не удалось загрузить сводку» + «Повторить» | —                                     |

## Mutations and repeats

| Mutation                                            | Repeat protection                                                                                                                 |
| --------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| add items (`addShoppingItems`)                      | `Idempotency-Key` per attempt (same text → same key, renewed after success or text change); field disabled while pending          |
| undo add                                            | deletes with `If-Match`; snackbar closes at once, so no double tap                                                                |
| set status                                          | idempotent on the server; no key; the checkbox is not disabled (optimistic), repeats are harmless                                 |
| update / delete item, department, keyword; settings | `If-Match` version; submit disabled while pending; stale → reload                                                                 |
| create department / keyword                         | `Idempotency-Key` per opened form (reused on retry, renewed on «key reused»); submit disabled while pending; duplicate → 409 text |
| clear cart, reorder                                 | button / arrows disabled while pending; both idempotent in effect                                                                 |

## Decisions

- New lasting decision: a live shared list (optimistic checkbox, polling instead of sockets, per-device «кто я»)
  → web ADR 0012 (task TS-9).
