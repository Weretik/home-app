# Feature specifications

The SDD of each backend feature: requirements (`R-*` rules, `SC-*` scenarios), design, data model, contract
decisions, tasks, traceability, readiness checklists and the code audit. Layout: `specs/<area>/<NNN>-<slug>/`, the
same as the web half's `web/docs/specs/`. The module pages in [../modules](../modules/expenses.md) describe the code as
it is; a specification records why and how a feature was built and the evidence that it works.

| Area | Feature | Status | Module page |
| --- | --- | --- | --- |
| gifts | [001 — Gifts module (wishlist, archive, categories, details, summary)](gifts/001-gifts-module/README.md) | implemented, audited | [gifts.md](../modules/gifts.md) |
| todos | [001 — Todos module (tasks, steps, statuses, categories, archive, summary)](todos/001-todos-module/README.md) | implemented, audited | [todos.md](../modules/todos.md) |

Expenses was built before specifications were kept here; its decisions are in ADRs 0013–0020.
