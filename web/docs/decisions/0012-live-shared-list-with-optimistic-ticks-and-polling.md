# 0012. A live shared list: optimistic ticks, polling and «кто я» per device

Status: accepted, 2026-11-26. The «кто я» part is superseded by [0016](0016-pages-start-on-the-signed-in-person.md).

## Context

The shopping list replaces a Google Sheet that two people edit at the same time, often in a shop with a weak signal:
one adds bread at home while the other ticks milk at the till. The list must feel instant on a tap and show the other
person's changes within seconds. There is no sign-in yet (API ADR 0015), but every item records who added and who
bought it.

## Decision

- **Optimistic checkbox.** A tick changes the cached list at once (`withStatus` in `features/shopping/data`), the
  request follows; a failure restores the snapshot and says why; the list reloads after the last pending tick. The
  status write is idempotent on the server, so the checkbox is never locked and needs no key.
- **Polling, not sockets.** The open section reloads the list every 20 s while the tab is visible
  (`refetchIntervalInBackground: false`), on window focus and after each write. Polling pauses while a tick is
  pending so a reload cannot undo it. A hosted socket service is not worth it for two people and a list of tens of
  rows.
- **«Кто я» per device.** «Я: Виталий / Маша» in the section header is stored in `localStorage`
  (`home.shopping.me`, every access in `try/catch`, default Виталий) and shared through `useSyncExternalStore`. It is
  sent as `addedBy` / `by`. It is a convenience, not identity; sign-in will replace it.
- Other writes follow ADR 0007: an add sends an `Idempotency-Key`, edits and deletes send the row's version in
  `If-Match`, and «Отменить» after an add deletes the created rows with their versions.

## Consequences

- A change by the other person shows up within 20 s, or at once when the app comes back to the foreground.
- Up to three requests per minute per open phone; nothing while the tab is hidden.
- Two people can tick the same item; the last write wins, which matches the sheet.
- When sign-in arrives, `use-me.ts` is replaced by the signed-in person and the toggle disappears.
