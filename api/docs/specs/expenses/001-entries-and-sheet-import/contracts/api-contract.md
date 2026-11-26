# Entries and sheet import — API contract (as implemented)

**Status:** implemented

Machine contract: `api/docs/contracts/openapi.json`, generated from the code
([ADR 0008](../../../../decisions/0008-openapi-generated-from-code.md)). All under `/api/expenses`, tag `Expenses`,
anonymous; every write has `PublicWrite` and 429.

| operationId | Route | Request | Success | Errors |
| --- | --- | --- | --- | --- |
| `listExpenses` | `GET /api/expenses` | `year`+`month` or `from`+`to`; `categoryId`, `payer`, `page`, `pageSize` | 200 `ExpenseList` (`items` with `version`, `page`, `pageSize`, `totalCount`, `totalAmount`) | 400 |
| `createExpense` | `POST /api/expenses` | `Idempotency-Key`; `ExpenseRequest` `{ date, payer, categoryId, amount, paymentMethod, comment }` | 201 `{ id }`, `Location` | 400, 404, 409, 429 |
| `updateExpense` | `PUT /api/expenses/{id}` | `If-Match`; `ExpenseRequest` | 204 | 400, 404, 409, 429 |
| `deleteExpense` | `DELETE /api/expenses/{id}` | — | 204 | 400, 404, 429 |
| `listIncomes` | `GET /api/expenses/incomes` | `year`, `month`, `person`, `page`, `pageSize` | 200 `IncomeList` | 400 |
| `createIncome` | `POST /api/expenses/incomes` | `Idempotency-Key`; `IncomeRequest` `{ date, person, amount, comment }` | 201 `{ id }` | 400, 409, 429 |
| `updateIncome` | `PUT /api/expenses/incomes/{id}` | `If-Match`; `IncomeRequest` | 204 | 400, 404, 409, 429 |
| `deleteIncome` | `DELETE /api/expenses/incomes/{id}` | — | 204 | 400, 404, 429 |
| `listExpenseCategories` | `GET /api/expenses/categories` | — | 200 categories in `sortOrder` | — |
| (hidden) | `POST /api/expenses/imports/sheet` | `X-Import-Key`; `{ expenseRows, incomeRows, savingsRows? }` (sheet cell text) | 200 import report | 400 `expenses.import_invalid_rows`, 401, 404 (off), 409 (already running), 429 |

Error codes: `expenses.idempotency_key_reused`, `expenses.changed_meanwhile`, `expenses.expense_not_found`,
`expenses.income_not_found`, `expenses.category_not_found`, the `EntryErrors` amount/date/comment codes,
`expenses.import_invalid_rows`.

Repeats: creates by key (R-EN-007); edits by version (R-EN-008); deletes are naturally idempotent in effect (second
call 404); the import by import keys (R-EN-010).
