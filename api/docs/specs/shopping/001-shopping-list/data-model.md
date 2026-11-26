# Shopping list — data model

## Context

All data belongs to the Shopping module, schema `shopping`. No other module reads it.

## Model

```text
departments
├── id uuid PK, name varchar(40) unique, icon varchar(8), sort_order int, is_fallback bool, xmin (version)
department_keywords
├── id uuid PK, keyword varchar(60) unique (normalised), department_id → departments (restrict),
│   learned bool, created_at_utc timestamptz, xmin
shopping_items
├── id uuid PK, name varchar(80), normalized_name varchar(80), quantity varchar(30) null, note varchar(200) null,
│   department_id → departments (restrict), urgent bool, status varchar(20) ('Needed'|'Bought'),
│   added_by varchar(20), added_at_utc timestamptz, bought_by varchar(20) null, bought_at_utc timestamptz null,
│   cleared_at_utc timestamptz null, import_key varchar(100) null unique, xmin
shopping_settings
├── id uuid PK (singleton), cart_keep_hours int, frequent_days int, xmin
create_requests
└── key varchar(100) PK, kind varchar(20), fingerprint varchar(64), outcome text (JSON ids), created_at_utc
```

## Identifiers

| Object | Key | Where created | Public | Reason |
| --- | --- | --- | --- | --- |
| Shopping item | UUID v7 | domain `Create` | yes (`id`) | as Expenses |
| Department | UUID v7; seed: UUID v5 of `department:<name>` | domain / seed | yes | stable seed, import script can compute |
| Keyword | UUID v7; seed: UUID v5 of `keyword:<keyword>` | domain / seed | yes | same |
| Settings | fixed GUID | seed | no | singleton |
| Create request | client `Idempotency-Key` (≤100) | client | no | ADR 0007 |

## Invariants and integrity

- Exactly one fallback department: seeded, `IsFallback` has no setter path, delete refuses it.
- Department name unique (`ux_departments_name`), keyword unique (`ux_department_keywords_keyword`): handler
  pre-check + unique violation → 409.
- `bought_by` / `bought_at_utc` are set iff status is `Bought` (domain).
- `cleared_at_utc` only set on bought items (clear cart) and reset on restore.
- Item and keyword department FKs are `restrict`; department delete moves rows first in the same transaction.
