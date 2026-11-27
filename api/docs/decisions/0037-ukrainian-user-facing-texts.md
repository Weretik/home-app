# 0037. Texts the API and the bot write are Ukrainian, in place

Status: accepted, 2026-11-27

## Context

The site's interface moves to Ukrainian (web ADR 0014). The API also writes texts people read: Telegram messages and
buttons, the schedule shown on the «Телеграм» page, problem messages, and the title of the to-do made from a gift.

## Decision

- Every text the API writes for people is Ukrainian, kept in the class that already holds it; no resource files and
  no culture negotiation.
- Error codes, callback data, commands and the HTTP contract do not change; only the human text does.
- Seeded reference data, the sheet import and the shopping parser keep their Russian words: they are the family's data
  or read the family's Russian input.

## Consequences

- One deploy changes every message; old Telegram messages keep working buttons because callback data is the same.
- A second language would need resource files later; the texts are already grouped per feature, which makes that move
  mechanical.
