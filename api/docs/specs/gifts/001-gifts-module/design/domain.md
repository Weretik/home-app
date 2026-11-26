# Gifts — domain design

## Owning module

No existing module owns gift data: Expenses owns money that was spent, not wishes. A gift idea has its own life cycle
(wanted → gifted/dropped), its own categories and its own reference data, so the feature brings a new module `Gifts`
with five projects and the schema `gifts` ([ADR 0021](../../../../decisions/0021-gifts-module-model.md)). Alternatives
considered: growing Expenses (rejected: unrelated data, different categories, would couple two areas that change
independently); one generic "lists" module for shopping, to-dos and gifts (rejected: the three share almost no rules).
`Gifts.Contracts` stays empty: no other module needs gifts, and Gifts needs nothing from Expenses (a gift's price is a
wish, not an expense).

## Aggregates

| Aggregate | Folder | Rules it owns |
| --- | --- | --- |
| `Gift` (root) with `GiftVariant` children | `Domain/Wishlist/` | R-001…R-006: name, price, links, occasion, notes, ≤ 20 variants replaced as a whole, status transitions and the gifted day |
| `Category` (root) | `Domain/Categories/` | R-010: name trimmed 1–100 |
| `Detail` (root) | `Domain/People/` | R-011: label 1–100, value 1–200, recipient |

- `Gift.Create(...)` and `Gift.Update(...)` take the editable fields and a list of `VariantDraft(Title, Link, Price)`;
  both check everything and either change nothing or replace every field and the whole variant list
  ([ADR 0022](../../../../decisions/0022-gift-variants-owned-by-gift.md)). `Update` keeps `Status`, `GiftedOn` and
  `CreatedAtUtc`.
- `Gift.ChangeStatus(status, giftedOn, today)` applies R-006: `Gifted` sets the day (sent or `today`), other statuses
  clear it ([ADR 0023](../../../../decisions/0023-gifted-gifts-move-to-archive.md)). The handler supplies `today` in
  `Europe/Kyiv` (`HouseholdDay`), so the domain never reads a clock or a time zone.
- `GiftVariant` is an entity inside the aggregate (it has an id for the client and the import), never loaded or saved
  on its own; there is no `DbSet` for it.
- `Recipient` (`Maria`, `Vitaliy`, `Together`), `Priority` (`High`, `Normal`, `Someday`) and `GiftStatus` (`Wanted`,
  `Gifted`, `Dropped`) are enums stored as their names.
- Errors are typed `Error` values with stable codes `gifts.*` (ADR 0003), each with the field it concerns.
- `Category` and `Detail` are small roots with one rule each; they have no events and no services.
- Ids are UUID v7 (`Guid.CreateVersion7`), except the seeded categories (fixed ids from the brief).

## Cross-module interaction

None. No `Contracts` types, no outbox messages: no write has a side effect outside its transaction.

## Risks

- **Lost update** when two phones edit one gift: the `xmin` row version (ADR 0017). A change of variants only (the
  gift's own row unchanged) must still move the version; the module's `DbContext` marks the gift modified when its
  variants change (see [infrastructure](infrastructure.md)).
- **A category deleted while a gift is being added to it**: the foreign key `restrict` refuses the delete; the
  handler checks first and maps the foreign-key refusal of a race to the same `409`.
- **Import vs. app writes**: the import inserts with fixed ids and `ON CONFLICT DO NOTHING`; the app never reuses
  those ids, so the two never collide.
