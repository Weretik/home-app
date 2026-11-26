# 0026. The gifts sheet is imported once by a hand-run SQL script

Status: accepted, 2026-11-26

## Context

The «🎁 Список подарков» sheet holds 91 gifts, their links, variants hidden in the notes, and the details. They must
reach production once, when the module goes live. For Expenses the import was a keyed endpoint that parsed the
sheet's cells ([0014](0014-one-time-sheet-import.md)), because the expenses sheet kept growing and had to be imported
again; its rows then still reached production as a SQL dump.

## Decision

- No import endpoint, no import key and no import code in the Gifts module.
- The coordinator converts the sheet once into an SQL script outside the repository: it inserts into
  `gifts.gifts`, `gifts.gift_variants` and `gifts.details` with fixed ids, using the seeded category ids
  ([0024](0024-gift-categories-as-editable-data.md)), and every insert ends with `ON CONFLICT DO NOTHING`, so a second
  run changes nothing.
- Variants come from the notes: lines of «Другие варианты» become `gift_variants` rows (link, optional title and
  price) in their order; the rest of the notes stays in `notes`. Rows marked «Куплено» become `Gifted`.
- The script runs by hand (DBeaver) against production after the deploy that applies the `Initial` migration of the
  module, and once locally first to compare counts with the sheet.
- The module depends on the import only through the table and column names, which are therefore fixed
  ([0021](0021-gifts-module-model.md)).

## Consequences

- No code to maintain for a one-time move, and no secret to set and remove in production.
- A schema change of the five tables before the import runs must keep the script working, or the script changes with
  it.
- Gifts added on the site before the import do not collide with imported ones: the app generates its own UUID v7 ids.
