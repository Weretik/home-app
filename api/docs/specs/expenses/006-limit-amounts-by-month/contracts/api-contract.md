# API contract

The contract is generated from the code ([ADR 0008](../../../../decisions/0008-openapi-generated-from-code.md));
this lists the intended changes. All are additive for readers; writers may leave the new fields out.

| Operation | Change |
| --- | --- |
| `GET /api/expenses/budget` (`getBudget`) | `BudgetView.monthlyLimit` is the amount of the current month; new `monthlyLimits: AmountStep[]`. `SpendingLimitItem.amount` is the amount of the current period; new `amounts: AmountStep[]`. |
| `PUT /api/expenses/budget/limits/{id}` (`updateSpendingLimit`) | `SpendingLimitRequest.amountFrom?: date` — the period the amount applies from; given, it always replaces the steps from that period on (R-LA-002); left out, the current period, and an amount already in force changes nothing (R-LA-005). Ignored on create. |
| `POST /api/expenses/budget/limits` (`createSpendingLimit`) | no change of meaning; `amountFrom` is not used. |
| `PUT /api/expenses/budget/settings` (`updateBudgetSettings`) | `BudgetSettingsRequest.monthlyLimitFrom?: date` (same rules as `amountFrom`, by month; left out: the current month). |
| `GET /api/expenses/summary/{year}/{month}` | no shape change; `limits[].amount` and `monthlyLimit` are the amounts of that period. |

`AmountStep` = `{ from: date, amount: number }`.

Errors: `amountFrom` / `monthlyLimitFrom` outside 2000-01-01…2100-12-31 → 400 validation problem (field name in
`errors`). Every other answer is unchanged (404 missing limit, 409 `expenses.changed_meanwhile` when the row changed since the
`If-Match` version, 400 validation problem when `If-Match` is missing).
