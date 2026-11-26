# Budget settings and spending limits — domain design (as implemented)

## Responsibility

- `SpendingLimit` (`Domain/Budget/`): `Create`/`Update` check name, amount (`IsMoney && > 0`), categories (non-empty,
  distinct, no empty guid); `PeriodAround(day)` returns the month or quarter start and the day after its end.
  `CategoryIds` is a private list exposed read-only.
- `BudgetSettings`: singleton with a fixed id; `Update(monthlyLimit, savingsGoal)` accepts money ≥ 0.
- `LimitPeriod`: `Month`, `Quarter`.
- `BudgetErrors`: `NameRequired`, `NameTooLong`, `AmountInvalid`, `CategoriesRequired`, `SettingsInvalid`,
  `LimitNotFound`.

## Cross-module interaction

None. Category existence is checked in Application (`BudgetCategories`) because the ids are an array without a
foreign key.

## Risks

- Overlapping limits double-count a category by design (ADR 0019).
- `SortOrder` race (GAP-009).
