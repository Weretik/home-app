# Tasks

| ID    | Task                                                                                                            | Depends on | Test written first                       | Status |
| ----- | --------------------------------------------------------------------------------------------------------------- | ---------- | ---------------------------------------- | ------ |
| EN-1  | Line limits in `tools/check-structure.mjs`; split the two files over them                                       | —          | `npm run architecture:structure`         | done   |
| EN-2  | Generated client with `/api/shopping/*` (`npm run contracts:generate`)                                          | backend    | `npm run typecheck`                      | done   |
| TS-1  | SDD (this folder)                                                                                               | —          | —                                        | done   |
| TS-2  | Pure logic: `item-status`, `list-groups`, `history-days`, `item-form`, `department-order`, `list-settings-form` | —          | their `.test.ts`                         | done   |
| TS-3  | Data layer: `shopping-api`, `item-writes-api`, `status-api`, `write-failure`, `use-me`                          | EN-2       | `use-me.test.ts`                         | done   |
| TS-4  | Quick add, preview, ⭐ Частые, undo snackbar                                                                    | TS-3       | `ListPage.test.tsx`                      | done   |
| TS-5  | Section, tabs, Список page, groups, rows, cart, states                                                          | TS-3       | `ListPage.test.tsx`                      | done   |
| TS-6  | Item edit dialog and delete                                                                                     | TS-5       | `item-form.test.ts`, `ListPage.test.tsx` | done   |
| TS-7  | История page                                                                                                    | TS-3       | `HistoryPage.test.tsx`                   | done   |
| TS-8  | Настройки: departments, keywords, list settings                                                                 | TS-3       | `SettingsPage.test.tsx`                  | done   |
| TS-9  | Routes in `App.tsx`; docs: architecture.md, ADR 0012, decisions README                                          | TS-5       | `AppLayout.test.tsx`                     | done   |
| TS-10 | Home summary `home/shopping-summary` + registry entry                                                           | EN-2       | `ShoppingSummary.test.tsx`               | done   |
| TS-11 | Verification and code audit                                                                                     | all        | all checks                               | done   |

Phases: [01 foundation](01-foundation.md), [02 list](02-list.md), [03 history and settings](03-history-settings.md),
[04 summary and verification](04-summary-verification.md).
