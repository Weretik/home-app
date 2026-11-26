# 0024. Gift categories are editable data with a seeded start

Status: accepted, 2026-11-26

## Context

The sheet sorted gifts into categories (Техника, Дом и уют, Кухня …). The family wants to add, rename and delete them
in the settings of the section. Expenses has its own categories, which mean something else (what money was spent on).

## Decision

- `gifts.categories` (`id`, `name` unique ≤ 100, `sort_order`) belongs to the Gifts module; it shares nothing with the
  expense categories.
- The first migration inserts the 11 starting categories with fixed ids (Техника … Другое, sort orders 10–110), the
  ids the import SQL refers to. The seed is never edited afterwards; later changes go through the app.
- Settings can add a category (at the end: the last sort order + 10, with an `Idempotency-Key`), rename it (a target
  value, no version) and delete it. A taken name answers `409` `gifts.category_name_taken` (unique index
  `ux_categories_name`).
- A category that any gift uses, archived ones included, cannot be deleted: `409` `gifts.category_in_use`. The handler
  checks first; the foreign key `fk_gifts_categories` (`restrict`) refuses a delete that races with a new gift, and the
  handler answers that with the same `409`.

## Consequences

- Categories change without a release, and a gift never points at a missing category.
- Deleting a used category needs moving its gifts first; there is no merge operation yet.
