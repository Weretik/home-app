# Savings jar — infrastructure design (as implemented)

## Storage and queries

- `SavingsMovementConfiguration`: table `savings_movements`, `amount numeric(12,2)` signed, `ix_savings_movements_date`,
  partial unique `ux_savings_movements_import_key`, `xmin` version.
- `ListSavingsMovements`: four `SUM` queries (before the month, deposits, withdrawals, all) and one paged projection.
- `ListSavingsMonths`: one query grouped by year and month (deposits and withdrawals by sign), then the running balance
  in memory; not paged (GAP-006).

## Migration and deployment

Migration `Savings` created the table. The sheet's «Копилка» rows were loaded with the import (001) locally and moved
to production as `INSERT … ON CONFLICT DO NOTHING`.
