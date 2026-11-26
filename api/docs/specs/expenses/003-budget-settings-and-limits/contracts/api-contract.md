# Budget settings and spending limits — API contract (as implemented)

**Status:** implemented. Machine contract: `api/docs/contracts/openapi.json`.

| operationId | Route | Request | Success | Errors |
| --- | --- | --- | --- | --- |
| `getBudget` | `GET /api/expenses/budget` | — | 200 `BudgetView` `{ version, monthlyLimit, savingsGoal, limits[] { id, version, name, period, amount, categoryIds } }` | — |
| `updateBudgetSettings` | `PUT /api/expenses/budget/settings` | `If-Match`; `{ monthlyLimit, savingsGoal }` | 204 | 400, 404, 409, 429 |
| `createSpendingLimit` | `POST /api/expenses/budget/limits` | `Idempotency-Key`; `{ name, period, amount, categoryIds }` | 201 `{ id }` | 400, 404, 409, 429 |
| `updateSpendingLimit` | `PUT /api/expenses/budget/limits/{id}` | `If-Match`; same body | 204 | 400, 404, 409, 429 |
| `deleteSpendingLimit` | `DELETE /api/expenses/budget/limits/{id}` | — | 204 | 400, 404, 429 |

Error codes: `expenses.limit_not_found`, `expenses.category_not_found`, `expenses.budget_settings_invalid`,
`expenses.changed_meanwhile`, `expenses.idempotency_key_reused`, and the name/amount/category validation codes.
