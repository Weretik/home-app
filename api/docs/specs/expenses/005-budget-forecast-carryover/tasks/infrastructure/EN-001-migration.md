# EN-001 — Migration ForecastAndCarryOver and the plan mapping

- **Task ID:** EN-001
- **Covers:** SC-OL-001, SC-CO-007, SC-FC-001
- **Depends on:** none
- **Exact paths:** `src/Modules/Expenses/Expenses.Infrastructure/Persistence/Entries/{ExpenseConfiguration,PlannedExpenseConfiguration}.cs`, `.../Persistence/Reference/BudgetSettingsConfiguration.cs`, `.../Persistence/Migrations/<ts>_ForecastAndCarryOver.cs`, `.../ExpensesDbContext.cs`, `src/Modules/Expenses/Expenses.Application/IExpensesDbContext.cs` (relative to `api/`)
- **Test level:** integration (schema through the API); enabler: no behavioral Red before the columns exist
- **Architecture basis:** [API rules, Data](../../../../../rules.md#data), [ADR 0016](../../../../../decisions/0016-migrations-as-bundles-in-deploy.md)

## Work

Order: Red → Green → Refactor → Regression → evidence.

- [ ] Map `OutsideLimits`, `LimitsFrom`, `PlannedExpense` (`xmin` version, enums as names, FK to categories, `ix_planned_expenses_month`).
- [ ] Add `DbSet<PlannedExpense>` to the port and the context.
- [ ] Generate the migration; `limits_from` default 2026-11-01 for the existing row; additive only.
- [ ] Replacement check: `dotnet ef migrations script` reviewed; integration test host applies it.
- [ ] Check the changed files for one responsibility each; split a file that mixes independent responsibilities
      along the module's folders (Domain by aggregate, Application `Features/<Feature>/Commands|Queries/<UseCase>/`,
      Api by feature); keep the six-files-per-folder rule.

## Evidence

- Test and assertion:
- Red command and expected behavioral failure:
- Green command and result:
- Refactor note and re-run:
- Regression command and result:
- Deviation or blocker:

## Done when

The migration applies on an empty database and on the current production shape; the previous release still runs against it.
