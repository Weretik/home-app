# Data model

Schema `expenses`.

| Table | Column | Before | After |
| --- | --- | --- | --- |
| `spending_limits` | `amount numeric(12,2) not null` | the one amount | kept in this release, no longer mapped, `default 0`; dropped by the follow-up migration |
| `spending_limits` | `amounts jsonb not null default '[]'` | — | the steps, oldest first: `[{"from":"2000-01-01","amount":7000.00}, …]` |
| `budget_settings` | `monthly_limit numeric(12,2) not null` | the one amount | kept in this release, no longer mapped, `default 0`; dropped by the follow-up migration |
| `budget_settings` | `monthly_limits jsonb not null default '[]'` | — | the steps of the monthly limit (amount 0 = no limit) |

The stored step is an Infrastructure record with fixed JSON names `from` and `amount` (not the domain `AmountStep`),
so renaming the domain type changes neither stored rows nor the contract; a step without those names fails the read
instead of being read as 0.

The `xmin` row version is unchanged, so an edit based on an older version is still refused with `409`
`expenses.changed_meanwhile`.

## Migration order: add first, remove later

The deploy runs the migration bundle before the new code (ADR 0016), so `LimitAmountsByMonth` only adds
(rules.md "add first, remove later"):

- `amounts` / `monthly_limits` are added with the default `'[]'` and filled with one step from 2000-01-01 per row
  (R-LA-009). The default stays, so the previous release can still insert a limit (it writes only `amount`).
- `amount` / `monthly_limit` stay with their values and get `default 0`, so the new code (which no longer maps them)
  can insert. The previous release keeps reading and writing them while it runs.
- The new code reads `[]` as one step of 0 from 2000-01-01 ("no limit" by every rule) instead of failing; such a row
  can only come from the previous release inserting a limit between the migration and the new code going live, and
  the follow-up migration repairs it. Chosen over a trigger that syncs both columns: no database code to maintain,
  and the window is minutes for two users.
- Rollback: the previous build alone works on the expanded schema (it sees each limit's amount as of the migration,
  not later changes); `Down` additionally writes back the amount in force in the current month (the last step that
  starts on or before `date_trunc('month', now())`, else the first step) and drops the new columns.

## Follow-up migration (after this release is live)

Not part of this change (task EN-002): a migration `DropLimitSingleAmounts` that

1. fills rows still holding `'[]'` from the old column:
   `UPDATE … SET amounts = jsonb_build_array(jsonb_build_object('from','2000-01-01','amount', amount)) WHERE amounts = '[]'`
   (and `monthly_limits` from `monthly_limit`);
2. drops the defaults of `amounts` / `monthly_limits`;
3. drops `amount` and `monthly_limit`; its `Down` adds them back from the step in force in the current month.
