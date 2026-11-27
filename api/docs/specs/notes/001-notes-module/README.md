# 001 — Notes module (what Masha and Vitaliy like, dislike and should know about each other)

- **Module:** `Notes` (`api/src/Modules/Notes`), schema `notes`
- **Architecture source:** [architecture](../../../architecture/README.md), [rules](../../../rules.md), reference module
  [Todos](../../../modules/todos.md)
- **Status:** accepted for delivery
- **Owner:** Vitaliy
- **Created:** 2026-11-27 · **Updated:** 2026-11-27
- **Decisions:** [ADR 0039](../../../decisions/0039-notes-module-model.md) (the module and its model),
  [ADR 0040](../../../decisions/0040-current-member-through-identity-contracts.md) (who is signed in, for other modules)
- **Frontend SDD:** [web/docs/specs/notes/001-notes-section](../../../../../web/docs/specs/notes/001-notes-section/README.md)

On 2026-11-27 Vitaliy asked for a place where he and Masha keep facts and preferences about each other: «Маша любит
такие-то цветы, не любит молоко из АТБ, но любит из Агромола», «Виталий любит окрошку и технологии, не любит томатный
сок». Either of them adds, edits and deletes notes about themselves or about the other one, sees the notes about
themselves or about the partner, and sees all of them together. He asked for its own backend module with its own
schema and its own section of the site, not a part of the profile (the profile page, built in the identity work,
only links to it).

Written from the backend SDD template (Weretik/obsidian `Templates/backend-sdd`), condensed to one page per part.

## State

- Specification: accepted
- Implementation: in progress
- Verification: see [tasks](tasks.md#delivery-readiness-filled-before-merge)
- Blockers: none

## Parts

- [Requirements: scope, rules and scenarios](requirements.md)
- [Design: model, schema, contract](design.md)
- [Tasks and traceability](tasks.md)
- Final backend code audit: `audit.md` (written after the implementation)

## Change history

- 2026-11-27: first version (Vitaliy: «страница для себя и общая, где любой может увидеть, добавить или поменять
  заметку про себя или человека … с категориями и описанием»; «отдельной страницей … и даже модулем … в бекенде со
  своей схемой»; «мы можем посмотреть про себя или про партнёра»).
