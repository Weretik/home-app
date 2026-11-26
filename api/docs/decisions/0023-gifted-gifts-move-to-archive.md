# 0023. A gifted or dropped gift moves to an archive by its status

Status: accepted, 2026-11-26

## Context

In the sheet a bought gift was greyed out («Куплено») and stayed among the wanted ones, so the list grew with things
that no longer needed attention. The family wants a gifted or bought item to move to «Подаренные» instead, and to be
able to drop an idea or move it back.

Options considered: deleting gifted rows (loses what was given, which is useful next year); a separate archive table
(moving rows between tables, two shapes to keep in step); a status on the gift (chosen).

## Decision

- `GiftStatus` is `Wanted` (the active list), `Gifted` (given or bought; the sheet's «Куплено») or `Dropped` («Не
  актуально»). A new gift is `Wanted`; an edit of the gift keeps its status.
- The status changes through its own operation `PUT /api/gifts/items/{id}/status` with the version in `If-Match`. It
  sets a target value, so a repeat changes nothing.
- `gifted_on` is set only while the status is `Gifted`: the day sent, else the day the gift already has (a repeated
  «Подарено» keeps it), else today in `Europe/Kyiv`. Any other status clears it.
- `GET /api/gifts/items?archived=false` returns `Wanted` gifts by priority, nearest occasion and creation;
  `archived=true` returns `Gifted` and `Dropped` gifts, latest gifted day first. `ix_gifts_status` serves both.
- The summary counts `Gifted` gifts and their known prices; `Dropped` gifts count nowhere.

## Consequences

- The active list holds only what still matters, and the archive remembers what was given and when.
- "Today" depends on the household's time zone, not on the server's, as for the month summary.
- `gifted_on` without `Gifted` is prevented by the domain, not by a database check, so a hand-edited row can still be
  corrected in the app.
