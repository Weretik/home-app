# Budget settings and spending limits — data model (as implemented)

## Model

```text
expenses.spending_limits  id, name (100), period (text enum, 20), amount numeric(12,2), category_ids uuid[],
                          sort_order, xmin — 7 seeded rows with fixed ids
expenses.budget_settings  id (fixed singleton), monthly_limit numeric(12,2), savings_goal numeric(12,2), xmin
```

## Identifiers

| Object | ID / key | Where created | Public id? | Reason |
| --- | --- | --- | --- | --- |
| `SpendingLimit` | `uuid` v7 (seeded rows fixed) | domain `Create` / migration | yes | module strategy |
| `BudgetSettings` | fixed `5b0f3a2e-1c4d-4e8a-9f6b-2d7c8e9a0b1c` | migration | no (singleton route) | one row |
| create request | `Idempotency-Key`, kind `limit` | client | no | ADR 0007 |

## Invariants and integrity

- Name, amount, categories: domain. Categories exist: handler (no FK on the array, GAP-011).
- Settings ≥ 0 money: domain.
