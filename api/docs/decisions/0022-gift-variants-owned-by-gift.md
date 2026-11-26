# 0022. Gift variants belong to the gift and are replaced as a whole

Status: accepted, 2026-11-26

## Context

One gift often has several candidates: other shops, other models, each with its own link and sometimes a price. In the
sheet they were lines like «Другие варианты: …» inside the notes, hard to read and impossible to open from a phone.
The family wants to add and remove them easily and look through them quickly.

Options considered: variants as a separate aggregate with their own endpoints (more requests and races for a list that
is always edited together with the gift); variants as JSON in a column of `gifts` (the import and any later query would
have to parse it); child rows owned by the gift (chosen).

## Decision

- `GiftVariant` is an entity inside the `Gift` aggregate: an optional title (≤ 200), a link (required, absolute
  `http`/`https`, ≤ 2000), an optional price (money rules) and its position. At most 20 per gift.
- It is stored in `gifts.gift_variants` (`gift_id` foreign key with `ON DELETE CASCADE`) and mapped as an EF Core owned
  collection: it loads and saves with its gift and has no `DbSet` or endpoint of its own.
- Create and update send the whole list in order, and an update replaces the stored list (target values, not deltas):
  a repeat of the same update changes nothing visible, and two edits cannot interleave their variant changes. Variant
  ids are new after every update; clients must not keep them across an edit.
- A change of the variants alone still moves the gift's version: the module's `DbContext` marks the gift modified when
  its variants change, so the `xmin` check covers the whole aggregate.

## Consequences

- One request edits a gift completely, which fits the gift dialog; no separate variant API to keep in step.
- Replacing the list deletes and inserts a few rows per edit; with at most 20 per gift that is negligible.
- Ids of variants are not stable across edits; anything that wants to refer to one variant later (for example "bought
  this one") needs a stable key first.
