# 0031. The Shopping module, with the list, cart and history computed on read

Status: accepted, 2026-11-26

## Context

The family's shopping list lives in the Google Sheet «🛒 Список покупок» and a Telegram bot: a «Список» tab of items
with status Нужно / Куплено, an «История» tab that a timer fills with items bought more than three hours ago (or on
«🧹 Убрать купленные»), and a «Справочник» of departments and keywords. It moves to the site next, with notes, an
urgent flag, editable departments and keywords, a history page and a home summary.

## Decision

- A new module, `Shopping`, with its own schema `shopping` and an empty `Shopping.Contracts`. It owns items,
  departments, keywords and its settings; no other module reads them. `Person` is a copy of the Expenses enum, not a
  reference to it.
- One `ShoppingItem` aggregate serves the list, the cart and history. Its stored status is only `Needed` or
  `Bought`; where a bought item is shown is computed on every read from `BoughtAtUtc`, `ClearedAtUtc` and
  `ShoppingSettings.CartKeepHours` (`CartWindow`, one rule used both in memory and as a LINQ expression in SQL). No
  timer, background job or scheduled tick moves rows; «Убрать купленные» is one conditional `UPDATE` setting
  `ClearedAtUtc`.
- Setting a status is a target value (no version), so two people ticking the same item cannot undo each other. Edits
  and deletes carry the `xmin` version ([0017](0017-row-version-via-xmin-for-edits.md)); creates carry an
  `Idempotency-Key` ([0007](0007-idempotency-key-for-creates.md)) whose stored outcome is JSON, because one add can
  create, revive and update several items.
- Departments are data with a store-walk order and exactly one fallback («Другое»). Items and keywords reference them
  with restrictive foreign keys inside the schema; deleting a department moves its rows to the fallback in one
  explicit transaction.
- Frequent items and the summary are computed on read (grouped in SQL); nothing is stored.
- The dictionary is seeded by the first migration from the sheet's «Справочник», with UUID v5 ids so scripts can
  compute them.

## Consequences

- The cart empties itself at the right moment for every reader, and a change of `CartKeepHours` applies at once,
  also to items bought earlier.
- History and the list are one table: history grows by a few thousand rows a year; the `(status, bought_at_utc)` index
  keeps the cart, history and frequent queries cheap.
- Two simultaneous adds of the same new name can create two rows (no unique index on the name, since history repeats
  names); for two people this is visible at once and deleted by hand.
- The deploy builds one more migration bundle (`ShoppingDbContext`) in `.github/workflows/api.yml`. The first
  migration creates the `pg_trgm` extension, which a database owner may create (it is a trusted extension).
