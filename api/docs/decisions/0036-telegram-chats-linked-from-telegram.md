# 0036. Telegram chats are linked from Telegram, not configured

Status: accepted, 2026-11-26

## Context

The bot must know which chat is Vitaliy's, which is Masha's and which is the family group. The old bots kept a
`USERS` JSON with Telegram ids in Apps Script properties, which had to be found (`/id`) and typed by hand; Masha's id
was never filled in. The site has no sign-in yet ([0015](0015-expenses-open-until-identity.md)).

## Decision

- The table `notifications.linked_chats` holds at most one chat per audience (`Vitaliy`, `Maria`, `Family`), each chat
  once (unique indexes).
- In a private chat `/start` asks «Кто вы?» with two buttons; a tap links the chat when that audience is free. Adding
  the bot to a group, or `/start` there, links the group as `Family` when it is free. A taken slot is refused with a
  pointer to the site.
- The site shows the linked chats and can unlink one, which frees the slot for linking again.

## Consequences

- No chat ids in settings; linking takes one tap.
- Whoever taps first takes a free slot. The family links right after the deploy, and the site shows every linked chat,
  so a wrong link is visible and undone with «Отвязать».
