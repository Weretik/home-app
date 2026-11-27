# 001 — «Телеграм» page (bot status and linked chats)

- **App type:** React Web (Vite, MUI, React Query, React Router)
- **App:** `web/`, feature `src/features/notifications`
- **Architecture source:** [architecture](../../../architecture.md), [rules](../../../rules.md), [design rules](../../../design.md)
- **Status:** accepted for delivery
- **Owner:** Vitaliy · **Created:** 2026-11-26 · **Updated:** 2026-11-26
- **Backend SDD:** [api/docs/specs/notifications](../../../../../api/docs/specs/notifications/001-telegram-bot/README.md)
  (rules, schedule, contract `getNotificationsStatus`, `unlinkNotificationsChat`, `sendNotificationsTest`)
- **Related:** [gifts 002 — gift purchase to-do](../../gifts/002-gift-purchase-todo/README.md)

One Telegram bot for the site sends the morning to-dos, the evening shopping list, the weekly and monthly expenses
summaries and the gift reminders. Chats are linked from Telegram itself (`/start` → «Я Виталий / Я Маша», the bot
added to the family group), so the site never asks for a chat id. This page shows whether the bot works, which chats
are linked, lets the family unlink a chat or send a test message, and lists the schedule and the last deliveries.
Written from the frontend SDD template (Weretik/obsidian `Templates/frontend-sdd`), React variant, condensed.

## State

- Specification: accepted · Implementation: done, bound to the generated client · Verification: local checks green, audit in [audit.md](audit.md) ·
  Blockers: none

## Requirements and behaviour

| ID     | Behaviour                                                                                                                                                                                                                                                                                                                                                                  |
| ------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| FR-001 | `/notifications` «Телеграм» is a service page: in the side menu on a computer (under «Состояние»), and a card on the home page on a phone, as «Состояние» is.                                                                                                                                                                                                              |
| FR-002 | «Бот» card: when the API has no bot token (`configured: false`) a warning says the bot is not set up yet and what to add (`Telegram__BotToken` in the App Service settings); otherwise «Бот работает» with `@username` and an «Открыть бота» button to `botLink` (new tab). If Telegram could not be reached (`botUsername` null) the button is hidden and a note says so. |
| FR-003 | «Как подключить» steps: 1) open the bot and send `/start`, 2) press «Я Виталий» or «Я Маша», 3) a note that the family group is optional: without it the common messages go to each private chat (Vitaliy, 2026-11-27: the bot will not be in a group).                                                                                                                    |
| FR-004 | «Чаты» card: three rows — Виталий, Маша, Семейная группа — each with «подключён» (chat title, linked date) or «не подключён». A linked row has «Тест» (the API sends «✅ Бот на связи» at once, a snackbar confirms or shows the problem) and «Отвязать» (asks first, then `DELETE`).                                                                                      |
| FR-005 | «Расписание» card: the API's `schedule` list as rows «title — when» (Russian text from the API, the page adds nothing).                                                                                                                                                                                                                                                    |
| FR-006 | «Последние отправки» card: the last deliveries (kind title from the schedule, audience, planned time, «отправлено» time or «в очереди»); hidden when empty. Times in Kyiv as «27.11 08:15».                                                                                                                                                                                |
| FR-007 | States: loading skeleton; an error shows the problem message and «Повторить»; writes disable their buttons while pending and show the problem message in a snackbar.                                                                                                                                                                                                       |
| FR-008 | Works at 360 px without horizontal scroll, both colour schemes, design tokens only.                                                                                                                                                                                                                                                                                        |
| FR-009 | «Бот» card: when the bot is set up and `schedulerSilent` is true, a red alert «Планувальник мовчить з 27.11, 08:00» (or «жодного виклику ще не було») says digests are not going out and to check cron-job.org (backend R-020, 2026-11-27).                                                                                                                                |

## Design

### Structure (`src/features/notifications`, at most 6 production files a folder)

```
notifications/
  data/   notifications-api.ts (status query), notification-writes-api.ts (unlink, test), labels.ts (audience
          labels, delivery titles from the schedule, Kyiv time format)
  page/   NotificationsPage (title, states, two columns), InfoCard (titled card frame), BotCard (bot state and the
          link steps), ScheduleCard, DeliveriesCard
  chats/  ChatsCard (rows, unlink and test with their snackbar), ChatRow, UnlinkDialog
```

### State and data

- Server state only in React Query under a new module prefix `moduleKeys.notifications = ['notifications']`
  (`src/platform/query-keys.ts`); `['notifications', 'status']` is the only read. The unlink invalidates the prefix
  (ADR 0009); the test send changes nothing stored and refreshes nothing.
- «Тест» sends `POST /api/notifications/test` `{ audience }`; the API sends at once and stores nothing, so no
  `Idempotency-Key` (backend SDD, "Contract"); «Отвязать» sends `DELETE /api/notifications/chats/{audience}`.
- No URL state: the page has nothing to filter.

### Routes and shell

- `App.tsx`: `/notifications` → `NotificationsPage`.
- `sections.ts`: `serviceSections` gets «Телеграм» (`TelegramIcon` from MUI icons).
- `home/page/HomePage.tsx`: a second service card «Телеграм» next to «Состояние».

## Tasks and traceability

| ID     | Responsibility                                                        | Test                                    | Covers         |
| ------ | --------------------------------------------------------------------- | --------------------------------------- | -------------- |
| TS-201 | Regenerate the client from the contract                               | `npm run contracts:generate`, typecheck | —              |
| TS-202 | `labels.ts`: person labels, delivery time format                      | Vitest unit                             | FR-004, FR-006 |
| TS-203 | Page: not configured, linked and unlinked chats, schedule, deliveries | Testing Library, mocked fetch           | FR-002…FR-007  |
| TS-204 | Unlink asks and sends `DELETE`; test sends `POST` and confirms        | Testing Library                         | FR-004         |
| TS-205 | Route, side menu, home card; docs; structure check                    | `npm run architecture:structure`, tests | FR-001, FR-008 |
| TS-206 | Page: red alert when the scheduler is silent, none while it ticks     | Testing Library, mocked fetch           | FR-009         |

## Readiness

- [x] Behaviour, states and routes defined; contract taken from the backend SDD.
- [x] Delivery: typecheck, lint, tests, structure check green locally (CI pending).
- [x] Final frontend code audit: [audit.md](audit.md).
