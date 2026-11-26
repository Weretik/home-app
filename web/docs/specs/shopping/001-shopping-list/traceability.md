# Traceability

| Rule | Scenarios   | Tasks             | Tests / evidence                                              |
| ---- | ----------- | ----------------- | ------------------------------------------------------------- |
| R-1  | SC-1, SC-3  | TS-3, TS-4        | `ListPage.test.tsx` (add, preview)                            |
| R-2  | SC-2        | TS-4              | `ListPage.test.tsx` (undo)                                    |
| R-3  | SC-4        | TS-2, TS-5        | `list-groups.test.ts`, `ListPage.test.tsx`                    |
| R-4  | SC-5        | TS-2, TS-3, TS-5  | `item-status.test.ts`, `ListPage.test.tsx` (toggle, rollback) |
| R-5  | —           | TS-3              | `shopping-api.ts` options; ADR 0012                           |
| R-6  | SC-7        | TS-6              | `item-form.test.ts`, `ListPage.test.tsx` (edit)               |
| R-7  | SC-6        | TS-5              | `ListPage.test.tsx` (clear cart)                              |
| R-8  | SC-8        | TS-4              | `ListPage.test.tsx` (chip)                                    |
| R-9  | SC-14       | TS-3              | `use-me.test.ts`                                              |
| R-10 | SC-10       | TS-7              | `history-days.test.ts`, `HistoryPage.test.tsx`                |
| R-11 | SC-11       | TS-8              | `department-order.test.ts`, `SettingsPage.test.tsx`           |
| R-12 | SC-12       | TS-8              | `SettingsPage.test.tsx` (search, learned, edit, add with key) |
| R-13 | SC-13       | TS-8              | `list-settings-form.test.ts`, `SettingsPage.test.tsx`         |
| R-14 | SC-15       | TS-10             | `ShoppingSummary.test.tsx`                                    |
| R-15 | SC-9, SC-16 | TS-5, TS-7, TS-10 | page tests (empty, error)                                     |
