# 001 — Notifications module (the family Telegram bot)

- **Module:** `Notifications` (`api/src/Modules/Notifications`), schema `notifications`
- **Architecture source:** [architecture](../../../architecture/README.md), [rules](../../../rules.md), reference module
  [Todos](../../../modules/todos.md)
- **Status:** accepted for delivery
- **Owner:** Vitaliy
- **Created:** 2026-11-26 · **Updated:** 2026-11-27
- **Related backend SDD:** [gifts/002 — a gift to buy becomes a to-do](../../gifts/002-gift-purchase-todo/README.md)
- **Frontend SDD:** [web/docs/specs/notifications](../../../../../web/docs/specs/README.md)

One Telegram bot for the site replaces the reminders of the old Apps Script bots: a weekly and a monthly expenses
summary, a morning to-do digest for each person and a common one, an evening shopping list reminder, and a reminder
to buy a gift two weeks before its date. The bot also answers a few commands and lets a person tick a task done from
the morning message.

Written from the backend SDD template (Weretik/obsidian `Templates/backend-sdd`), condensed to one page per part.

## State

- Specification: accepted
- Implementation: in progress
- Verification: see [tasks](tasks.md#delivery-readiness-filled-before-merge)
- Blockers: the bot token (`Telegram__BotToken`) and the scheduler key (`OutboxTick__Key`, `SCHEDULER_KEY`) are set
  by Vitaliy; without them the module answers "not configured" and sends nothing.

## Parts

- [Requirements: scope, rules and scenarios](requirements.md)
- [Design: model, schema, contract, schedule](design.md)
- [Tasks and traceability](tasks.md)
- [Final backend code audit](audit.md)

## Change history

- 2026-11-26: first version (Vitaliy: «бот на сайт в телеграм, который будет давать сводку… каждое утро список дел…
  каждый вечер после 17 что есть в списке покупок… напоминание за 2 недели купить подарок»).
