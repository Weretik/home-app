# Web feature specifications

Scenario-first specifications (SDD) of web features, built from the frontend SDD feature template with the React
variant and written before the code. One folder per feature under its area: `specs/<area>/<NNN>-<slug>/`, the same
layout as the backend's [api/docs/specs](../../../api/docs/specs/README.md). A specification keeps its `R-*`, `SC-*`,
`TS-*` and `EN-*` ids once accepted; the lasting description of a built feature lives in
[architecture](../architecture.md), not here. A retroactive SDD describes behavior built before the SDD existed; it
gives it stable ids, lists gaps, and changes nothing.

| Area          | SDD                                                                                                             | Kind        | Status                        | Backend counterpart                                                                  |
| ------------- | --------------------------------------------------------------------------------------------------------------- | ----------- | ----------------------------- | ------------------------------------------------------------------------------------ |
| expenses      | [001 — Expenses and incomes pages](expenses/001-entries-pages/README.md)                                        | retroactive | implemented                   | [api 001](../../../api/docs/specs/expenses/001-entries-and-sheet-import/README.md)   |
| expenses      | [002 — Savings page «Копилка» and the goal card](expenses/002-savings-page/README.md)                           | retroactive | implemented                   | [api 002](../../../api/docs/specs/expenses/002-savings-jar/README.md)                |
| expenses      | [003 — Budget settings page «Настройки»](expenses/003-budget-settings-page/README.md)                           | retroactive | implemented                   | [api 003](../../../api/docs/specs/expenses/003-budget-settings-and-limits/README.md) |
| expenses      | [004 — Home page expenses summary](expenses/004-home-expenses-summary/README.md)                                | retroactive | implemented                   | [api 004](../../../api/docs/specs/expenses/004-month-summary/README.md)              |
| expenses      | [005 — Budget: forecast, carry-over, expenses outside limits](expenses/005-budget-forecast-carryover/README.md) | new         | in implementation             | [api 005](../../../api/docs/specs/expenses/005-budget-forecast-carryover/README.md)  |
| gifts         | [001 — Gifts section](gifts/001-gifts-section/README.md)                                                        | new         | implemented, ready for review | [api 001](../../../api/docs/specs/gifts/001-gifts-module/README.md)                  |
| gifts         | [002 — Gift purchase: giver, buy-by date, to-do link](gifts/002-gift-purchase-todo/README.md)                   | new         | implemented                   | [api 002](../../../api/docs/specs/gifts/002-gift-purchase-todo/README.md)            |
| notifications | [001 — «Телеграм» page](notifications/001-telegram-page/README.md)                                              | new         | implemented                   | [api 001](../../../api/docs/specs/notifications/001-telegram-bot/README.md)          |
| shopping      | [001 — Shopping list «Покупки»](shopping/001-shopping-list/README.md)                                           | new         | implemented                   | [api 001](../../../api/docs/specs/shopping/001-shopping-list/README.md)              |
| shopping      | [002 — Name suggestions in the quick add](shopping/002-name-suggestions/README.md)                              | new         | implemented, audited          | [api 002](../../../api/docs/specs/shopping/002-name-suggestions/README.md)           |
| shopping      | [003 — History by month](shopping/003-history-by-month/README.md)                                               | new         | implemented, audited          | none (web only)                                                                      |
| todos         | [001 — «Дела» section](todos/001-todos-section/README.md)                                                       | new         | implemented, audited          | [api 001](../../../api/docs/specs/todos/001-todos-module/README.md)                  |
