# 001 — Todos module (family to-do list)

- **Module:** `Todos` (`api/src/Modules/Todos`), schema `todos`
- **Architecture source:** [architecture](../../../architecture/README.md), [rules](../../../rules.md), reference module
  [Expenses](../../../modules/expenses.md)
- **Status:** accepted for delivery
- **Owner:** Vitaliy
- **Created:** 2026-11-26 · **Updated:** 2026-11-26
- **Frontend SDD:** [web/docs/specs/todos](../../../../../web/docs/specs/todos/001-todos-section/README.md)

Replaces the Google Sheet «✅ Список дел» (sheet «Все дела», 13 rows on 2026-11-26) and its Telegram bot with a
module of the site: Masha and Vitaliy give tasks to each other or to both, break them into steps, change their status
and priority, sort them by categories the family edits, keep notes, and find finished tasks in an archive.

Written from the backend SDD template (Weretik/obsidian `Templates/backend-sdd`), condensed to one page per part.

## State

- Specification: accepted
- Implementation: in progress
- Verification: unit, architecture and integration tests green locally; final backend code audit done
  ([audit](audit.md): 3 findings fixed, the rest accepted with reasons)
- Blockers: none

## Parts

- [Requirements: scope, rules and scenarios](requirements.md)
- [Design: model, schema, contract](design.md)
- [Tasks and traceability](tasks.md)
- [Final backend code audit](audit.md)

## Change history

- 2026-11-26: first version (Vitaliy: «полноценный To-do для семьи, лучше чем в Google таблицах, словно свой Notion»).
