# Budget forecast and carry-over (web) — traceability

| Scenario  | Rules                        | Tasks                  | Test level         | Tests                                                                          | Evidence                                                        | Status  |
| --------- | ---------------------------- | ---------------------- | ------------------ | ------------------------------------------------------------------------------ | --------------------------------------------------------------- | ------- |
| SC-W5-001 | R-W5-001                     | TS-001                 | component          | `src/app/shell/AppLayout.test.tsx`, `src/features/home/page/HomePage.test.tsx` | [TS-001](tasks/shell/TS-001-section-name.md#evidence)           | planned |
| SC-W5-002 | R-W5-002                     | TS-003, TS-006, EN-001 | component          | `src/features/expenses/forecast-page/ForecastPage.test.tsx`                    | [TS-006](tasks/forecast/TS-006-forecast-page.md#evidence)       | planned |
| SC-W5-003 | R-W5-003                     | TS-004, TS-006         | unit + component   | `planned-editing/planned-form.test.ts`, `ForecastPage.test.tsx`                | [TS-004](tasks/forecast/TS-004-plan-dialogs.md#evidence)        | planned |
| SC-W5-004 | R-W5-003                     | TS-004, TS-006         | component          | `ForecastPage.test.tsx`                                                        | [TS-006](tasks/forecast/TS-006-forecast-page.md#evidence)       | planned |
| SC-W5-005 | R-W5-004                     | TS-005, TS-006         | unit + component   | `planned-editing/record-form.test.ts`, `ForecastPage.test.tsx`                 | [TS-005](tasks/forecast/TS-005-record-dialog.md#evidence)       | planned |
| SC-W5-006 | R-W5-005                     | TS-002                 | unit + component   | `expense-editing/expense-form.test.ts`, `expense-list/ExpensesPage.test.tsx`   | [TS-002](tasks/entries/TS-002-outside-limits-field.md#evidence) | planned |
| SC-W5-007 | R-W5-006                     | TS-007                 | unit + component   | `home/expenses-summary/limits/limit-rows.test.ts`, `HomePage.test.tsx`         | [TS-007](tasks/home/TS-007-limit-notes.md#evidence)             | planned |
| SC-W5-008 | R-W5-006                     | TS-008                 | unit               | `home/expenses-summary/data/figure-lines.test.ts`                              | [TS-008](tasks/home/TS-008-outside-limits-line.md#evidence)     | planned |
| SC-W5-009 | R-W5-007                     | TS-009                 | unit + component   | `figure-lines.test.ts`, `HomePage.test.tsx`                                    | [TS-009](tasks/home/TS-009-forecast-card.md#evidence)           | planned |
| SC-W5-010 | R-W5-002, R-W5-006, R-W5-007 | TS-010                 | component + manual | page tests at phone width; manual 360 px check                                 | [TS-010](tasks/verification/TS-010-verification.md#evidence)    | planned |

No E2E harness exists in the project; the main path is covered by page tests with a mocked client.
