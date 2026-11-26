# Budget settings page — API contract (as implemented)

Source: `api/docs/contracts/openapi.json` at `8378e4a`.

| operationId                                  | Method / path                                         | Scenarios      | Consumer                           |
| -------------------------------------------- | ----------------------------------------------------- | -------------- | ---------------------------------- |
| `getBudget`                                  | `GET /api/expenses/budget`                            | SC-W3-001      | `budget-data/budget-api.ts`        |
| `updateBudgetSettings`                       | `PUT /api/expenses/budget/settings`, `If-Match`       | SC-W3-002      | `budget-data/budget-writes-api.ts` |
| `createSpendingLimit`                        | `POST /api/expenses/budget/limits`, `Idempotency-Key` | SC-W3-004      | same                               |
| `updateSpendingLimit`, `deleteSpendingLimit` | `PUT/DELETE /api/expenses/budget/limits/{id}`         | SC-W3-005      | same                               |
| `listExpenseCategories`                      | `GET /api/expenses/categories`                        | SC-W3-001, 004 | `data/expenses-api.ts`             |
