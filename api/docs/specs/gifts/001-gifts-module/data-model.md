# Gifts — data model

## Context

All data belongs to the `Gifts` module and lives in the PostgreSQL schema `gifts`. Table and column names are fixed by
the product brief because the one-time import SQL inserts into them ([ADR 0026](../../../decisions/0026-gifts-import-as-sql-script.md)).

## Model

```text
gifts.categories                   Category (root)
├── id uuid pk                     fixed for the 11 seeded rows
├── name varchar(100) unique       ux_categories_name
└── sort_order int

gifts.gifts                        Gift (root), version = xmin
├── id uuid pk
├── recipient varchar(16)          Maria | Vitaliy | Together
├── name varchar(200)
├── category_id uuid → categories  fk_gifts_categories, ON DELETE RESTRICT; ix_gifts_category_id
├── priority varchar(16)           High | Normal | Someday
├── price numeric(12,2) null       null = unknown
├── link varchar(2000) null
├── status varchar(16)             Wanted | Gifted | Dropped; ix_gifts_status
├── gifted_on date null            set only while status = Gifted
├── occasion varchar(100) null
├── occasion_date date null
├── notes varchar(2000) null
└── created_at_utc timestamptz

gifts.gift_variants                GiftVariant (child of Gift, owned)
├── id uuid pk
├── gift_id uuid → gifts           fk_gift_variants_gifts, ON DELETE CASCADE; ix_gift_variants_gift_id
├── title varchar(200) null
├── link varchar(2000) not null
├── price numeric(12,2) null
└── sort_order int                 position in the gift's list, from 0

gifts.details                      Detail (root), version = xmin
├── id uuid pk
├── recipient varchar(16)          Maria | Vitaliy | Together
├── label varchar(100) not null
├── value varchar(200) not null
└── sort_order int                 ix_details_recipient_sort_order (recipient, sort_order)

gifts.create_requests              idempotency of creates (ADR 0007)
├── key varchar(100) pk
├── kind varchar(20)               gift | category | detail
├── fingerprint varchar(64)        hex SHA-256 of the request data
├── entity_id uuid
└── created_at_utc timestamptz
```

## Identifiers

| Object | ID / key | Where created | Public id? | Reason |
| --- | --- | --- | --- | --- |
| Gift | `uuid` v7 | domain (`Guid.CreateVersion7(now)`); fixed by the import for the sheet's rows | yes, in routes | project strategy (Expenses); time-ordered inserts |
| GiftVariant | `uuid` v7 | domain on create and on every update (A-1) | yes, in the list, informational | the import needs a key per row; the client never addresses a variant by id |
| Category | `uuid`; fixed for the seed, v7 for new ones | migration seed / domain | yes | the import references seeded ids |
| Detail | `uuid` v7 | domain; fixed by the import | yes, in routes | as Gift |
| Create request | the client's `Idempotency-Key` (≤ 100) | client | no | ADR 0007 |

## Invariants and integrity

- Name, price, links, variants, occasion, notes, status/day rules: in `Gift` (domain), tested in
  `tests/UnitTests/Gifts/Domain`.
- At most 20 variants: domain; not a database constraint (the import respects it; the sheet has fewer than 10 per row).
- `gifted_on` only with `Gifted`: domain; no check constraint, so an import or a hand edit stays possible to correct in
  the app.
- A gift's category exists: foreign key (same schema) plus a handler check for a readable 404.
- A used category cannot be deleted: foreign key `restrict` plus a handler check for a readable 409.
- Category names are unique: unique index.
- No foreign keys to other schemas (none needed).
