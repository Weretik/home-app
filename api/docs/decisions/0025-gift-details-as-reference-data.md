# 0025. Personal details are reference data of the Gifts module, shown read-only on the home page

Status: accepted, 2026-11-26

## Context

Choosing a gift needs small facts about the person: Маша's ring size, necklace and bracelet length, clothes and shoe
sizes («нюансы / детали»). The sheet kept them in a corner. The family wants them on the home page next to the gifts
summary, but edited in one place only.

Options considered: a profile in a future Identity module (does not exist yet, and the facts are about gifts, not
sign-in); free text in the module's settings (cannot be listed per person); small labelled rows per person (chosen).

## Decision

- `Detail` is its own small aggregate in the Gifts module, table `gifts.details`: recipient (`Maria`, `Vitaliy`,
  `Together`), label (≤ 100, such as «Кольцо»), value (≤ 200, such as «18,5–19,0»), `sort_order`, and the `xmin`
  version.
- Details are added, edited (with `If-Match`) and deleted only through `/api/gifts/details`, which the web app uses
  only on the «Настройки» tab of the gifts section. New details go to the end of their recipient's list.
- `GET /api/gifts/summary` includes all details, ordered by recipient (Маша, Виталий, Вместе) and position, and the
  home page shows them without edit controls.

## Consequences

- One source for sizes, visible where gifts are chosen; no duplicates on the home page.
- These are personal data of the two people (body sizes); they are kept until deleted in settings and are not logged.
  When the Identity module arrives, access to them follows its rules.
