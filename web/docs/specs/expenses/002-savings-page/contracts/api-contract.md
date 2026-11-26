# Savings page — API contract (as implemented)

Source: `api/docs/contracts/openapi.json` at `8378e4a`, generated client `web/src/api-client/generated/api.ts`.

| operationId                                                               | Method / path                                  | Scenarios      | Consumer                    |
| ------------------------------------------------------------------------- | ---------------------------------------------- | -------------- | --------------------------- |
| `listSavingsMovements`                                                    | `GET /api/expenses/savings`                    | SC-W2-001      | `data/savings-api.ts`       |
| `listSavingsMonths`                                                       | `GET /api/expenses/savings/months`             | SC-W2-001      | `data/savings-api.ts`       |
| `createSavingsMovement`, `updateSavingsMovement`, `deleteSavingsMovement` | `POST/PUT/DELETE /api/expenses/savings[/{id}]` | SC-W2-002, 003 | `data/writes-api.ts`        |
| `getBudget`                                                               | `GET /api/expenses/budget` (`savingsGoal`)     | SC-W2-004      | `budget-data/budget-api.ts` |

Mapping: `toSavingsRequest` signs the amount by direction; `savingsFormValues` unsigns it for editing.
