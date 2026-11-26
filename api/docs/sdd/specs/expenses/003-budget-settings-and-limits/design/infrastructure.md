# Budget settings and spending limits — infrastructure design (as implemented)

## Storage

- `spending_limits`: `category_ids uuid[]` (EF primitive collection), `period` as text, `xmin` version.
- `budget_settings`: one row with a fixed id, `xmin` version.
- Seeded by the `Budget` migration with plain SQL (EF seed data cannot hold the array without reporting a model change
  on every build; values mirrored in `BudgetSeed`).
- Writes: one `SaveChangesAsync`; limit delete with `ExecuteDeleteAsync`.

## Migration and deployment

Migration `Budget` (additive), applied as an EF bundle in the deploy job. Later edits happen on «Настройки».
