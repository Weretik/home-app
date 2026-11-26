# Feature specifications

The SDD of each backend feature, built from the backend SDD feature template and written before the code
([ADR 0004](../../../docs/decisions/0004-sdd-first-feature-delivery.md)): requirements (`R-*` rules, `SC-*`
scenarios), design, data model, contract decisions, tasks, traceability, readiness checklists and the code audit.
Layout: `specs/<module>/<NNN>-<slug>/`, the same as the web half's [web/docs/specs](../../../web/docs/specs/README.md).
The module pages in [../modules](../modules/expenses.md) describe the code as it is; a specification records why and
how a feature was built and the evidence that it works. A retroactive SDD describes behavior that was built before the
SDD existed; it gives the behavior stable ids and lists gaps, and changes nothing.

| Module | SDD | Kind | Status | Frontend counterpart |
| --- | --- | --- | --- | --- |
| Expenses | [001 — Expenses and incomes: lists, period filter, editing, sheet import](expenses/001-entries-and-sheet-import/README.md) | retroactive | implemented | [web 001](../../../web/docs/specs/expenses/001-entries-pages/README.md) |
| Expenses | [002 — Savings jar «Копилка» and the savings goal](expenses/002-savings-jar/README.md) | retroactive | implemented | [web 002](../../../web/docs/specs/expenses/002-savings-page/README.md) |
| Expenses | [003 — Budget settings and spending limits](expenses/003-budget-settings-and-limits/README.md) | retroactive | implemented | [web 003](../../../web/docs/specs/expenses/003-budget-settings-page/README.md) |
| Expenses | [004 — Month summary](expenses/004-month-summary/README.md) | retroactive | implemented | [web 004](../../../web/docs/specs/expenses/004-home-expenses-summary/README.md) |
| Expenses | [005 — Budget: forecast, limit carry-over, expenses outside limits](expenses/005-budget-forecast-carryover/README.md) | new | in implementation | [web 005](../../../web/docs/specs/expenses/005-budget-forecast-carryover/README.md) |
| Gifts | [001 — Gifts module (wishlist, archive, categories, details, summary)](gifts/001-gifts-module/README.md) | new | implemented, audited | [web 001](../../../web/docs/specs/gifts/001-gifts-section/README.md) |
| Shopping | [001 — Shopping list (list, cart, history, departments, keywords, frequent items, summary)](shopping/001-shopping-list/README.md) | new | implemented | [web 001](../../../web/docs/specs/shopping/001-shopping-list/README.md) |
| Todos | [001 — Todos module (tasks, steps, statuses, categories, archive, summary)](todos/001-todos-module/README.md) | new | implemented, audited | [web 001](../../../web/docs/specs/todos/001-todos-section/README.md) |
