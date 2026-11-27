# 002 — A gift to buy becomes a to-do

- **Modules:** `Gifts` (publishes), `Todos` (consumes and publishes back), `Notifications` (reminds)
- **Status:** accepted for delivery
- **Owner:** Vitaliy
- **Created:** 2026-11-26
- **Related:** [notifications/001 — Telegram bot](../../notifications/001-telegram-bot/README.md),
  [ADR 0035](../../../decisions/0035-integration-events-through-the-outbox.md); frontend
  [web/docs/specs](../../../../../web/docs/specs/README.md)

Vitaliy: «ставить дату в подарках, к какой дате купить подарок, для кого, и кто кому дарит; он попадает в список дел
для этого человека, и напоминание за 2 недели… и как ставить выполнено, подарок автоматически переносится в
подаренное».

Written from the backend SDD template, condensed to one page.

## Rules

| ID | Rule |
| --- | --- |
| R-001 | A gift gets two optional fields: `giver` («Кто дарит»: `Vitaliy`, `Maria`, `Together`) and `buyBy` («Купить до», a day in 2000–2100, `gifts.buy_by_out_of_range`). «Кому» is the existing `recipient`. |
| R-002 | A gift is *planned* when it is `Wanted` and has both a giver and a buy-by date. |
| R-003 | When a save makes a gift planned, or changes the name, recipient, giver or buy-by date of a planned gift, Gifts publishes `GiftPurchasePlanned` in the same transaction (outbox). When a planned gift stops being planned (gifted, dropped, giver or date cleared, deleted), Gifts publishes `GiftPurchaseClosed` with the reason. |
| R-004 | Todos keeps one task per gift (`source_kind = 'gift'`, `source_id` = gift id, unique together). On `GiftPurchasePlanned` it creates or updates it: title «🎁 Купити подарунок: <name>» plus « (для Маші)» / « (для Віталія)» when the recipient is one person, assignee = giver (`Together` → `Shared`), due = buy-by, priority `High`, notes with the gift link `<site>/gifts?gift=<id>`; an archived task is reopened as `Todo`. |
| R-005 | On `GiftPurchaseClosed`: reason `Gifted` → the task becomes `Done`; any other reason → `Cancelled`. A task already archived stays as it is. These changes publish nothing back. |
| R-006 | When a person moves a task with a source into `Done` (site or bot), Todos publishes `TodoTaskCompleted { taskId, sourceKind, sourceId }`. Gifts moves a `Wanted` gift to `Gifted` with today's Kyiv date; any other status is left alone. |
| R-007 | Every handler is safe to run twice (upsert by source, status checks); a lost race on a row version or the unique source index fails the attempt and the outbox retries it. |
| R-008 | The task list shows the source (`sourceKind`, `sourceId`) so the site can link the task to the gift. |
| R-009 | The reminder two weeks before the buy-by date is sent by the Notifications module (its R-010). |

## Scenarios

| ID | Scenario |
| --- | --- |
| SC-001 | Masha's ring: giver Vitaliy, buy-by 20.12 → within a tick Vitaliy has «🎁 Купити подарунок: Кольцо (для Маші)» due 20.12. |
| SC-002 | The buy-by date moves to 18.12 → the same task's due date is 18.12 (no second task). |
| SC-003 | Vitaliy ticks the task done → the ring is in «Подаренные», gifted today. |
| SC-004 | The gift is dropped instead → the task is cancelled. |
| SC-005 | The giver is cleared → the task is cancelled; set again → it is reopened. |
| SC-006 | The same outbox message is handled twice → one task, one state. |

## Contract changes

- `GiftRequest` and `GiftItem` get `giver` (`Recipient` enum or null) and `buyBy` (date or null).
- `TodoTaskItem` gets `sourceKind` (`"gift"` or null) and `sourceId` (uuid or null), read only.

## Data

- `gifts.gifts`: `giver varchar(16) null`, `buy_by date null`; `gifts.outbox_messages` (new).
- `todos.tasks`: `source_kind varchar(20) null`, `source_id uuid null`, unique index on both (filtered, not null);
  `todos.outbox_messages` (new).
- Migrations only add columns and tables.

## Tasks

| ID | Responsibility | Test level | Covers |
| --- | --- | --- | --- |
| GT-001 | `Gift` giver and buy-by, `PurchasePlan` diff → messages | unit | R-001…R-003 |
| GT-002 | Gifts create/update/status/delete publish | integration | R-003 |
| GT-003 | Todos handlers for planned and closed | unit (texts) + integration | R-004, R-005, R-007 |
| GT-004 | Todos publishes `TodoTaskCompleted`; Gifts handler | integration | R-006 |
| GT-005 | Contract fields, regenerated OpenAPI | architecture | R-008 |
