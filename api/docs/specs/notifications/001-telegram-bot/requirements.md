# Notifications — scope, rules and scenarios

## Goal

As Masha or Vitaliy, I want one Telegram bot that tells me in the morning what I have to do today, tells both of us
what is still open or overdue, reminds us in the evening what is on the shopping list, sums up our spending every week
and at the end of the month, and reminds the one who gives a gift to buy it two weeks before, so that we do not have
to open the site to keep track.

## In scope

- Linking chats from Telegram: a private chat for Vitaliy, one for Masha, and one family group.
- Scheduled digests (Kyiv time): personal morning to-dos, a common to-do message, gift reminders, the evening shopping
  list, the weekly and the monthly expenses summary.
- Commands: `/start` (link), `/today` (my to-dos now), `/help`; a «✅» button on each task of the personal morning
  message marks it done.
- On the site: the bot's status (configured, username), the linked chats with «Отвязать», a test message per chat,
  the schedule and the last deliveries.

## Out of scope

- Sign-in on the site ([ADR 0015](../../../decisions/0015-expenses-open-until-identity.md) applies).
- Adding expenses, shopping items or tasks from Telegram (the old bots keep doing that until they are retired).
- Changing the schedule from the site (fixed times, changed in code).
- AI parsing of messages.

## Actors

- Masha, Vitaliy: Telegram users and site users.
- Telegram Bot API: delivers updates to the webhook and accepts messages.
- Scheduler: GitHub Actions cron, every 15 minutes ([ADR 0034](../../../decisions/0034-scheduler-github-actions-cron.md)).

## Business rules

| ID | Rule |
| --- | --- |
| R-001 | There are three audiences: `Vitaliy`, `Maria` (private chats) and `Family` (a group). Each audience has at most one linked chat and a chat serves at most one audience (unique indexes). |
| R-002 | `/start` in a private chat that is not linked asks «Кто вы?» with buttons «Я Виталий» / «Я Маша». The button links the chat when the slot is free; when the slot holds another chat the bot says so and points to the site. A linked chat answers who it is linked as. |
| R-003 | Adding the bot to a group, or `/start` in a group, links the group as `Family` when the slot is free; otherwise the bot says the family chat is already linked. |
| R-004 | A chat that is not linked gets only the `/start` flow and a short refusal; `/today` and «✅» need a linked chat. |
| R-005 | Unlinking from the site frees the slot (`404` `notifications.chat_not_linked` when it is free). |
| R-006 | Digests are computed in Europe/Kyiv. A digest is due when the current Kyiv time is inside its window (table in the design) and it has not been planned for its period and audience yet. A tick outside the window plans nothing, so a missed window is skipped, not sent late. |
| R-007 | Every planned message is a `delivery` row with a unique key (`<kind>:<period>:<audience>`, e.g. `todos-morning:2026-11-27:Vitaliy`) written in the same transaction as its outbox message; a second tick finds the key and plans nothing (idempotent). The outbox sends it and records `sent_at_utc`; failures are retried by the outbox. |
| R-008 | Personal morning digest (08:00–11:59): the person's active tasks that are overdue or due today, then the shared ones that are overdue or due today; one «✅» button per task (at most 8). Not sent when the list is empty. |
| R-009 | Common to-do message (08:00–11:59): all overdue active tasks of everyone and the number of active tasks per assignee, with a link to the to-do page. Not sent when there are no active tasks. |
| R-010 | Gift reminder (08:00–11:59): for every wanted gift with a giver and a buy-by date 0–14 days ahead, one message to the giver (`Together` → both people) with the name, the recipient, the date, days left and the link `/gifts?gift=<id>`; once per gift, buy-by date and audience. |
| R-011 | Evening shopping list (18:00–21:59): the needed items grouped by department in the department order. Not sent when nothing is needed. |
| R-012 | Weekly expenses (Sunday 20:00–23:59): the week Monday–Sunday: total, by payer, top 5 categories; then the month so far against the monthly limit and the limits at 80 % or more. |
| R-013 | Monthly expenses (last day of the month 20:00–23:59, catch-up on the 1st 00:00–11:59 for the previous month): incomes, expenses by payer, result, savings jar, every limit with its percent, top categories. One per month. |
| R-014 | Messages that are not for a person (common to-dos, shopping, expenses) go to the `Family` chat; when it is not linked, to every linked private chat. Personal messages go only to that person's chat. |
| R-015 | The «✅ title» button completes the task through `Todos.Contracts`; a task that is gone or already archived answers so. Completing a task that came from a gift moves the gift to «Подаренные» ([gifts/002](../../gifts/002-gift-purchase-todo/README.md)). |
| R-016 | The webhook accepts only requests with the secret header derived from the bot token (constant-time compare; `401` otherwise, `404` when no token is configured) and always answers `200` for a valid request, so Telegram does not resend it. Replies to commands are sent at once and best effort. |
| R-017 | The tick needs the scheduler key (`X-Scheduler-Key` = `OutboxTick:Key`); it also sets the webhook when Telegram reports another url. Without a bot token it plans nothing and reports `configured: false`. |
| R-018 | A test message from the site goes at once to a linked chat (`404` when not linked, `409` `notifications.not_configured` without a token, `409` `notifications.telegram_failed` when Telegram refuses). |
| R-019 | All site writes are anonymous and rate limited (`PublicWrite`). |

## Acceptance scenarios

| ID | Scenario |
| --- | --- |
| SC-001 | Vitaliy writes `/start`, taps «Я Виталий»: the site shows his chat as linked. Masha does the same. A third person taps «Я Виталий»: refused. |
| SC-002 | The bot is added to the family group: the group is linked as `Family`. |
| SC-003 | 08:00 Kyiv: Vitaliy gets his overdue and today's tasks with ✅ buttons, Masha hers; then the group gets the common message. The tick at 08:15 sends nothing new. |
| SC-004 | Vitaliy taps «✅ Купить батарейки»: the task is done on the site; the answer says «Готово». |
| SC-005 | A gift for Masha with giver Vitaliy and buy-by 10.12 exists on 26.11: on 26.11 at 08:00 Vitaliy gets «🎁 Пора купить подарок…» with the link; never again for that date. |
| SC-006 | 18:00: two items are needed: the group gets them by department; on a day with an empty list nothing is sent. |
| SC-007 | Sunday 20:00: the group gets the week's spending; on 30.11 at 20:00 the November summary; if that tick was missed, it comes on 1.12 in the morning. |
| SC-008 | A tick with a wrong key answers `401`; a webhook call with a wrong secret answers `401`. |
| SC-009 | The site's «Отправить тест» sends «✅ Бот на связи» to the chosen chat. |

## Non-functional requirements

| Requirement | Value | Check |
| --- | --- | --- |
| Free hosting | no background thread (F1 sleeps); a public-repo cron calls the API | `HostRulesTests.Nothing_RunsInTheBackground`, ADR 0034 |
| Delivery | at least once; a duplicate is possible only when Telegram accepted a message and the save of `sent_at_utc` failed | outbox retries, R-007 |
| Message size | each message under Telegram's 4096 characters: lists are cut with «и ещё N» | formatter unit tests |
| Secrets | only `Telegram:BotToken` and `OutboxTick:Key`, from App Service settings; nothing in the repo | `ConfigurationRulesTests` |
