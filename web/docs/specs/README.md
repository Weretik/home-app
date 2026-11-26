# Web specifications (SDD)

Feature specifications of the web app, built from the frontend SDD feature template with the React variant:
`specs/<surface>/<domain>/<NNN>-<feature-slug>/`. The only surface is `web` (the Vite React app). A retroactive SDD
describes behavior built before the SDD existed; it gives it stable ids, lists gaps, and changes nothing.

| Domain   | SDD                                                                                                                 | Kind        | Status            | Backend counterpart                                                                      |
| -------- | ------------------------------------------------------------------------------------------------------------------- | ----------- | ----------------- | ---------------------------------------------------------------------------------------- |
| expenses | [001 — Expenses and incomes pages](web/expenses/001-entries-pages/README.md)                                        | retroactive | implemented       | [api 001](../../../api/docs/sdd/specs/expenses/001-entries-and-sheet-import/README.md)   |
| expenses | [002 — Savings page «Копилка» and the goal card](web/expenses/002-savings-page/README.md)                           | retroactive | implemented       | [api 002](../../../api/docs/sdd/specs/expenses/002-savings-jar/README.md)                |
| expenses | [003 — Budget settings page «Настройки»](web/expenses/003-budget-settings-page/README.md)                           | retroactive | implemented       | [api 003](../../../api/docs/sdd/specs/expenses/003-budget-settings-and-limits/README.md) |
| expenses | [004 — Home page expenses summary](web/expenses/004-home-expenses-summary/README.md)                                | retroactive | implemented       | [api 004](../../../api/docs/sdd/specs/expenses/004-month-summary/README.md)              |
| expenses | [005 — Budget: forecast, carry-over, expenses outside limits](web/expenses/005-budget-forecast-carryover/README.md) | new         | in implementation | [api 005](../../../api/docs/sdd/specs/expenses/005-budget-forecast-carryover/README.md)  |
