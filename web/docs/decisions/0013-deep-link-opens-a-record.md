# 0013. A deep link opens one record: `?gift=<id>`

Status: accepted, 2026-11-26

## Context

The Telegram bot reminds the giver two weeks before a gift's «купить до» date and links to the gift on the site; a
to-do made from a gift links there too. The site has no page per gift: gifts are rows of a list, edited in a dialog
(ADR 0007), and the list state lives in the URL (ADR 0006). A link must land on the one gift, from a phone, without a
new route.

## Decision

- A record is addressed by a query parameter on its section's list: `/gifts?gift=<id>`. The list page reads it, waits
  for its data, opens that record's edit dialog, and removes the parameter with `replace`, so closing the dialog or a
  reload does not reopen it and «Назад» leaves the page.
- A link that does not match the tab it lands on moves to the tab that holds the record (`/gifts` →
  `/gifts/archive?gift=<id>` for a gift already given); a record that exists nowhere is dropped with a short message.
- The decision is pure (`resolveGiftLink`) and tested; the hook only wires it to the router and the editor.
- Other features link to a record by plain in-app path (`/gifts?gift=<id>`), never by importing the target feature.

## Consequences

- Links from Telegram, from «Дела» and shared by hand all work the same way, with no new route and no extra request.
- The record must be in the loaded list: fine for gifts (the whole list is one request); a paged list (to-dos) would
  need its own read by id before it can use the same pattern.
