# Savings jar — API contract (as implemented)

**Status:** implemented. Machine contract: `api/docs/contracts/openapi.json`.

| operationId | Route | Request | Success | Errors |
| --- | --- | --- | --- | --- |
| `listSavingsMovements` | `GET /api/expenses/savings` | `year`, `month`, `page`, `pageSize` | 200 `SavingsMovementList` (`items` with `direction`, `version`; `deposits`, `withdrawals`, `balanceAtStart`, `balanceAtEnd`, `currentBalance`) | 400 |
| `listSavingsMonths` | `GET /api/expenses/savings/months` | — | 200 `SavingsOverview` (`currentBalance`, `months[]` with `deposits`, `withdrawals`, `balanceAtEnd`) | — |
| `createSavingsMovement` | `POST /api/expenses/savings` | `Idempotency-Key`; `{ date, person, amount (signed), comment }` | 201 `{ id }` | 400, 409, 429 |
| `updateSavingsMovement` | `PUT /api/expenses/savings/{id}` | `If-Match`; same body | 204 | 400, 404, 409, 429 |
| `deleteSavingsMovement` | `DELETE /api/expenses/savings/{id}` | — | 204 | 400, 404, 429 |

The goal is part of `getBudget` / `updateBudgetSettings` (003) and `getMonthSummary` (004).
