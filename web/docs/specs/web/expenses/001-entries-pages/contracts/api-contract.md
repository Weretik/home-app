# Expenses and incomes pages — API contract (as implemented)

## Source

- Provider: `api/` (Expenses module); canonical contract `api/docs/contracts/openapi.json`.
- Consumer snapshot: `web/src/api-client/generated/api.ts` (`npm run contracts:generate`, [web ADR 0003](../../../../../decisions/0003-typed-client-from-openapi.md)).
- Revision: `8378e4a`.

| operationId                                       | Method / path                                  | Scenarios                | Consumer                               |
| ------------------------------------------------- | ---------------------------------------------- | ------------------------ | -------------------------------------- |
| `listExpenses`                                    | `GET /api/expenses`                            | SC-W1-001, 003, 004, 008 | `data/expenses-api.ts` (`useExpenses`) |
| `listIncomes`                                     | `GET /api/expenses/incomes`                    | SC-W1-009                | `useIncomes`                           |
| `listExpenseCategories`                           | `GET /api/expenses/categories`                 | SC-W1-003                | `useExpenseCategories`                 |
| `createExpense`, `updateExpense`, `deleteExpense` | `POST/PUT/DELETE /api/expenses[/{id}]`         | SC-W1-005 … 007          | `data/writes-api.ts`                   |
| `createIncome`, `updateIncome`, `deleteIncome`    | `POST/PUT/DELETE /api/expenses/incomes[/{id}]` | SC-W1-009                | `data/writes-api.ts`                   |

## Client projection

- Requests from form values (`toExpenseRequest`, `toIncomeRequest`): amount parsed from text, comment trimmed or null.
- `Idempotency-Key` header on creates, `If-Match: <version>` on edits, nothing on deletes.
- Paging `page`/`pageSize` 50, `nextPage` from `totalCount`.
- Errors via `ApiError` (status, problem `code`, field errors by camelCase name).
