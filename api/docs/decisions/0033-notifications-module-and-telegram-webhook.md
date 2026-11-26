# 0033. The Notifications module: one Telegram bot with a webhook

Status: accepted, 2026-11-26

## Context

The old Apps Script bots reminded the family of their to-dos, the shopping list and the budget. With the data now on
the site, Vitaliy wants one bot for the site: a weekly and a monthly expenses summary, a morning to-do digest for each
person plus a common one, an evening shopping list reminder and a reminder to buy a gift two weeks before its date.
The API runs on App Service F1, which has no Always On: the process sleeps when idle, so it cannot keep a long-polling
loop (`getUpdates`) or an in-process timer. The specification is
[specs/notifications/001-telegram-bot](../specs/notifications/001-telegram-bot/README.md).

## Decision

- A new module `Notifications` (schema `notifications`) owns the bot: linked chats, deliveries, the webhook and the
  digest texts. It reads the other modules only through reader interfaces in their `Contracts`
  (`ITodosDigest`, `ITodoCompletion`, `IGiftsDigest`, `IShoppingDigest`, `IExpensesDigest`), implemented inside the
  owning modules, so it never touches their tables.
- **Incoming updates arrive by webhook**: `POST /api/notifications/telegram/webhook`. A request wakes the sleeping
  app; Telegram retries while it starts. The webhook is authenticated by Telegram's `secret_token` header, derived
  from the bot token (first 32 hex characters of SHA-256 of `webhook:<token>`), so there is no second secret to keep.
  The endpoint answers `200` to every valid request so Telegram never resends it; it is hidden from OpenAPI and has no
  rate limit (it is called by Telegram, not by the site).
- The notifications tick sets the webhook itself (`getWebhookInfo`, then `setWebhook` when the url differs), so a new
  host name or token needs no manual call. The url is `Notifications:ApiUrl`, else `https://{WEBSITE_HOSTNAME}`.
- **Scheduled messages go through the outbox** ([0006](0006-outbox-with-scheduled-tick.md)): the tick writes a
  `deliveries` row with a unique key and an outbox message in one transaction; the outbox sends and retries. The
  unique key makes a repeated or parallel tick plan nothing twice.
- **Interactive answers are sent at once**, not through the outbox: a reply to `/start`, `/today` or a «✅» button
  must come in seconds, not at the next tick, and it changes no data that could get out of step with the message. They
  are best effort: a failed reply is logged and the person simply asks again. The site's test message is sent at once
  for the same reason and reports Telegram's refusal as `409` `notifications.telegram_failed`; it stores nothing, so it
  takes no `Idempotency-Key`.
- The only secret is `Telegram:BotToken` (App Service `Telegram__BotToken`). Without it the module reports
  "not configured" and the tick plans nothing.

## Consequences

- No long-running process and no extra service; the bot costs nothing on the free tier.
- Messages are delivered at least once; a duplicate is possible only when Telegram accepted a message and saving
  `sent_at_utc` failed.
- The first update after a long idle period waits for the app to start (seconds); Telegram retries meanwhile.
- The schedule is code (`DigestSchedule`); changing a time is a code change.
