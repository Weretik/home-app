# 001 — Ukrainian texts from the API and the bot

- **Service:** `api/`, modules Notifications, Todos, Gifts, Shopping, Expenses
- **Architecture source:** [architecture](../../../architecture/README.md), [rules](../../../rules.md)
- **Status:** accepted for delivery
- **Owner:** Vitaliy · **Created:** 2026-11-27 · **Updated:** 2026-11-27
- **Frontend SDD:** [web/docs/specs/platform/001-ukrainian-interface](../../../../../web/docs/specs/platform/001-ukrainian-interface/README.md)
  · **Words:** [glossary](../../../../../web/docs/specs/platform/001-ukrainian-interface/glossary.md)
- **Decision:** [ADR 0037](../../../decisions/0037-ukrainian-user-facing-texts.md)

Vitaliy, 2026-11-27: the whole interface in Ukrainian, which includes what the Telegram bot sends and the messages the
API returns for the site to show. Written from the backend SDD template (Weretik/obsidian `Templates/backend-sdd`),
condensed: no contract, schema or data change.

## Scope

| ID     | Requirement |
| ------ | ----------- |
| FR-001 | Every Telegram message, button and command reply the bot writes is Ukrainian: `/start` and the «Хто ви?» buttons («Я Віталій», «Я Маша»), link replies, the test message «✅ Бот на зв'язку», the morning to-dos, the evening shopping list, the weekly and monthly expenses summaries, the gift reminders. |
| FR-002 | The schedule titles and «when» texts the API returns for the «Телеграм» page are Ukrainian. |
| FR-003 | Problem details the site shows (`title`, `detail`, validation messages, domain error messages) are Ukrainian. Error `code`s do not change. |
| FR-004 | Texts the API creates as data from now on are Ukrainian: the to-do made from a gift («Купити подарунок …»). Rows already stored stay as they are. |
| FR-005 | Not translated: seeded reference data (expense, gift and to-do categories, shopping departments and their keywords: the family's editable data, already in the database), the one-time sheet import (it reads the Russian sheet), and the shopping text parser (it keeps accepting Russian; Ukrainian unit words it already shares keep working). |
| FR-006 | Dates and months in bot messages are Ukrainian (`uk-UA` culture or Ukrainian month names); Kyiv time as before. |
| FR-007 | Button callback data and command names (`/start`) do not change, so a button pressed on an old Russian message still works. |

## Design

- Texts stay in the classes that already hold them (`Digests/Texts/*`, `BotTexts`, `*Errors`, validators, endpoint
  summaries that reach the user); no resource files ([ADR 0037](../../../decisions/0037-ukrainian-user-facing-texts.md)).
- A formatter that used the `ru-RU` culture takes `uk-UA`.
- OpenAPI descriptions stay English (docs language); the contract does not change unless an example value changes.

## Tests

| ID     | Check |
| ------ | ----- |
| TS-001 | Unit tests of the digest texts, bot texts and gift-to-do title expect the Ukrainian text. |
| TS-002 | Architecture tests pass; integration tests (CI) expecting messages are updated. |
| TS-003 | `dotnet build` leaves `openapi.json` unchanged or regenerated and committed. |
