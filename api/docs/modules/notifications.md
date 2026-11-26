# Notifications module

The family Telegram bot of the site: a personal morning to-do digest for Masha and Vitaliy, a common to-do message,
a reminder to buy a gift two weeks before its date, the evening shopping list, and the weekly and monthly expenses
summaries. It replaces the reminders of the old Apps Script bots. The module was added in
[ADR 0033](../decisions/0033-notifications-module-and-telegram-webhook.md) from the specification
[specs/notifications/001-telegram-bot](../specs/notifications/001-telegram-bot/README.md); the scheduler is
[ADR 0034](../decisions/0034-scheduler-github-actions-cron.md), chats are linked as
[ADR 0036](../decisions/0036-telegram-chats-linked-from-telegram.md) describes.

Code: [`src/Modules/Notifications`](../../src/Modules/Notifications). Schema: `notifications`.

## Model

| Concept | Meaning |
| --- | --- |
| `LinkedChat` | The Telegram chat of one `Audience`: `Vitaliy`, `Maria` (private chats) or `Family` (a group). One chat per audience, one audience per chat. |
| `Delivery` | One planned message with a unique key `<kind>:<period>:<audience>`; `sent_at_utc` is set when Telegram accepted it. |
| `DigestKind` | `TodosMorning`, `TodosCommon`, `GiftReminder`, `ShoppingEvening`, `ExpensesWeek`, `ExpensesMonth`. |
| `DigestSchedule` | The windows in Kyiv time (below); the only place the times live. |

## Schedule (Europe/Kyiv, the scheduler calls every 15 minutes)

| Message | When | To |
| --- | --- | --- |
| Personal to-dos: overdue and today's, then shared ones, with «✅» buttons | every day 08:00–11:59 | each linked person |
| Common to-dos: open per person, everything overdue | every day 08:00–11:59 | family group, or both people |
| Gift reminder with the link to the gift | 0–14 days before the gift's buy-by date, 08:00–11:59, once per date | the giver (both for «Вместе»), else the group |
| Shopping list by department (only when not empty) | every day 18:00–21:59 | family group, or both people |
| Expenses of the week (Monday–Sunday) and the month so far | Sunday 20:00–23:59 | family group, or both people |
| Full month summary | last day of the month 20:00–23:59, catch-up on the 1st until 11:59 | family group, or both people |

A message with nothing to say (no tasks for today, an empty list) is not sent. A window that no tick hit is skipped.
The personal and the common morning messages leave in the same outbox tick, so their order in Telegram is not
guaranteed. Audit: [notifications/001 audit](../specs/notifications/001-telegram-bot/audit.md).

## How it works

```mermaid
sequenceDiagram
    participant Cron as GitHub Actions cron
    participant Api as API (Notifications)
    participant Mods as Todos, Gifts, Shopping, Expenses (Contracts)
    participant TG as Telegram
    Cron->>Api: POST /internal/notifications/tick (X-Scheduler-Key)
    Api->>TG: getWebhookInfo / setWebhook (when the url differs)
    Api->>Mods: read only what the due digests need
    Api->>Api: deliveries + outbox messages, one save (unique keys)
    Cron->>Api: POST /internal/outbox/tick
    Api->>TG: sendMessage; deliveries.sent_at_utc
    TG->>Api: POST /api/notifications/telegram/webhook (secret header)
    Api->>TG: answers to /start, /today, /help, «✅»
```

- The module reads other modules only through their `Contracts`: `ITodosDigest`, `ITodoCompletion` (Todos),
  `IGiftsDigest` (Gifts), `IShoppingDigest` (Shopping), `IExpensesDigest` (Expenses); each owner implements its
  interface.
- Texts are built by pure types in `Application/Digests/Texts` (HTML, Russian) and tested with fixed input and day.
- Commands: `/start` (link; a private chat chooses «Я Виталий» / «Я Маша»; a group becomes `Family`), `/today`,
  `/help`. Removing the bot from a group unlinks it.

## HTTP API

Site (`/api/notifications`, tag `Notifications`): `getNotificationsStatus` (`GET /status`),
`unlinkNotificationsChat` (`DELETE /chats/{audience}`), `sendNotificationsTest` (`POST /test`, body `{ audience }`).
The test message is sent at once and stores nothing, so it takes no `Idempotency-Key`.

Machine (hidden from OpenAPI, no rate limit): `POST /api/notifications/telegram/webhook` (Telegram, header
`X-Telegram-Bot-Api-Secret-Token` derived from the token) and `POST /internal/notifications/tick` (scheduler,
`X-Scheduler-Key`).

## Configuration

| Setting | Where | Meaning |
| --- | --- | --- |
| `Telegram:BotToken` | App Service `Telegram__BotToken` (secret) | the bot from @BotFather; empty = not configured |
| `OutboxTick:Key` | App Service `OutboxTick__Key` (secret) = GitHub secret `SCHEDULER_KEY` | the scheduler key of both ticks |
| `Site:Url` | `appsettings.json` | links in messages (`/gifts?gift=…`, `/todos`, …) |
| `Notifications:ApiUrl` | optional | webhook base url; default `https://{WEBSITE_HOSTNAME}` set by App Service |

## Database

| Table | Columns |
| --- | --- |
| `linked_chats` | `id`, `audience` (unique), `chat_id` (unique), `title`, `linked_at_utc` |
| `deliveries` | `id`, `key` (unique), `kind`, `audience`, `chat_id`, `created_at_utc`, `sent_at_utc` |
| `outbox_messages` | the shared outbox table |

## Tests

- Unit (`tests/UnitTests/Notifications`): schedule windows and period keys, routing, the planner, every text.
- Integration (`tests/IntegrationTests/Api/Notifications`): status, unlink, test send, webhook secret, linking from
  updates, a tick that plans once, the gift ↔ to-do flow through the outbox.
- Architecture: the module is found by folder and checked like every other one.
