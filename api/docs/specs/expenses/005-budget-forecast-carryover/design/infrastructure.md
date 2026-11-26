# Budget forecast and carry-over — infrastructure design

## Storage and queries

- `ExpenseConfiguration`: `outside_limits boolean not null default false`.
- `BudgetSettingsConfiguration`: `limits_from date not null` (seeded 2026-11-01 for the existing row).
- `PlannedExpenseConfiguration` (new, `Persistence/Entries/`): table `expenses.planned_expenses`, `Version` mapped to
  `xmin` as a row version, enums as names, `numeric(12,2)` for the amount, foreign key to `expenses.categories`
  (same schema, `restrict`), index `ix_planned_expenses_month` on `month` (list and summary filter by month).
- `create_requests` is reused for plan creates with kind `planned`; its primary key on `key` is the race guard.
- Month summary reads (one `SaveChanges` is not involved — it is a query):
  - limit spending is read once, grouped by category and month, over `[first counted period start, end of the
    requested month's widest period)` and filtered `outside_limits = false`; the carry iteration then runs in
    memory per limit — no query per period or per limit (rule "no query inside a loop");
  - totals (`expenses`, by payer, top categories) keep counting all expenses;
  - outside-limits count and sum: one grouped query for the month;
  - plans: one query grouped by category and month for the widest period of the month.
- Record: load plan (tracked) → version check → `Expense.Create` → `Add(expense)`, `Remove(plan)` → one
  `SaveChangesAsync`; `DbUpdateConcurrencyException` → 409 `expenses.changed_meanwhile`.

## Migration and deployment

- One migration (`ForecastAndCarryOver`): add `expenses.outside_limits` with default false, add
  `budget_settings.limits_from` with default `2026-11-01` (then the row holds it), create `planned_expenses`.
  Additive only, so the previous release keeps working (rules, "Data").
- Applied as an EF bundle in the deploy job before the app ([ADR 0016](../../../../decisions/0016-migrations-as-bundles-in-deploy.md)).
- Check locally with `dotnet ef migrations script` and the integration tests (Testcontainers) before merge.
- Rollback: redeploy the previous app (it ignores the new columns and table); the migration's `Down` drops them and is
  run only if the feature is abandoned (it loses plans and flags).
