# 0017. Edits are guarded by a row version from PostgreSQL xmin

Status: accepted, 2026-11-26

## Context

Masha and Vitaliy edit the same expenses, incomes and savings movements, often from two phones. Without a guard the
later save silently overwrites the earlier one. The rules ask for a named race guard on shared state.

## Decision

- Expenses, incomes and savings movements use optimistic concurrency. Each aggregate has `Version` (`uint`), mapped
  with `IsRowVersion()` to PostgreSQL's system column `xmin`, which changes on every update. No column is added and
  nothing in the code increments it.
- List items expose `version`. `PUT` sends it in the `If-Match` header (quoted or not; a missing or unreadable value
  is `400`).
- The handler compares the sent version with the loaded row, and EF Core adds `xmin` to the `UPDATE` condition. Either
  mismatch answers `409` `expenses.changed_meanwhile`; the client reloads and lets the person try again.
- Deletes are not version-checked: deleting a row someone just edited is what the person asked for.

## Consequences

- Lost updates are prevented without an extra column, trigger or counter to maintain.
- `xmin` is a transaction id and changes on any update of the row, also one made outside the API (for example a
  script in DBeaver); an edit started before it answers `409` and the person reloads.
- The version is PostgreSQL-specific; moving to another database would need a real version column.
- The version is not an HTTP `ETag` for caching; it is only a concurrency token. A future module can reuse the same
  pattern for its own edits.
