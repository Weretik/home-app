# Backend specifications (SDD)

Feature specifications of the API, one folder per feature under its owning module:
`specs/<module>/<NNN>-<feature-slug>/`, built from the backend SDD feature template (requirements with `R-*`/`SC-*`,
design, data model, contract, traceability, tasks, checklists). A retroactive SDD describes behavior that was built
before the SDD existed; it gives the behavior stable ids and lists gaps, and changes nothing.

| Module | SDD | Kind | Status | Frontend counterpart |
| --- | --- | --- | --- | --- |
| Expenses | [001 — Expenses and incomes: lists, period filter, editing, sheet import](expenses/001-entries-and-sheet-import/README.md) | retroactive | implemented | [web 001](../../../../web/docs/specs/web/expenses/001-entries-pages/README.md) |
| Expenses | [002 — Savings jar «Копилка» and the savings goal](expenses/002-savings-jar/README.md) | retroactive | implemented | [web 002](../../../../web/docs/specs/web/expenses/002-savings-page/README.md) |
| Expenses | [003 — Budget settings and spending limits](expenses/003-budget-settings-and-limits/README.md) | retroactive | implemented | [web 003](../../../../web/docs/specs/web/expenses/003-budget-settings-page/README.md) |
| Expenses | [004 — Month summary](expenses/004-month-summary/README.md) | retroactive | implemented | [web 004](../../../../web/docs/specs/web/expenses/004-home-expenses-summary/README.md) |
| Expenses | [005 — Budget: forecast, limit carry-over, expenses outside limits](expenses/005-budget-forecast-carryover/README.md) | new | in implementation | [web 005](../../../../web/docs/specs/web/expenses/005-budget-forecast-carryover/README.md) |
