# Entries and sheet import — data model (as implemented)

## Context

Owned by the Expenses module, schema `expenses`. The full table list is on the
[module page](../../../../modules/expenses.md#database); this page names what this feature relies on.

## Model

```text
expenses.categories        id, name (100, unique ux_categories_name), sort_order — 14 seeded rows
expenses.expenses          id, date, payer, category_id → categories (restrict), amount numeric(12,2),
                           payment_method, comment (500), import_key (100, unique where not null), created_at_utc, xmin
                           indexes ix_expenses_date, ix_expenses_category_id
expenses.incomes           id, date, person, amount numeric(12,2), comment (500), import_key (unique where not null),
                           created_at_utc, xmin; index ix_incomes_date
expenses.create_requests   key (100, primary key), kind (20), fingerprint (64), entity_id, created_at_utc
```

## Identifiers

| Object | ID / key | Where created | Public id? | Reason |
| --- | --- | --- | --- | --- |
| `Expense`, `Income` | `uuid` v7 | domain `Create` | yes (routes) | module strategy, ADR 0013 |
| create request | client `Idempotency-Key` | web client | no | ADR 0007 |
| import key | `sheet:expense:…`, `sheet:income:…` (hash + occurrence) | import handler | no | makes the import repeatable |

## Invariants and integrity

- Money, date and comment rules: `EntryRules` (domain).
- Category exists: handler check (404) plus the foreign key.
- One row per import key: partial unique indexes.
- One result per idempotency key: primary key of `create_requests`.
