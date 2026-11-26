# Budget forecast and carry-over — API contract

**Status:** accepted

The machine contract is `api/docs/contracts/openapi.json`, generated from the code on every build
([ADR 0008](../../../../../decisions/0008-openapi-generated-from-code.md)); this page records the decisions only. Every
endpoint has a unique camelCase `WithName`, a `WithSummary`, `Produces…` for each response and `ProducesProblem`;
every write uses `RequireRateLimiting(RateLimitPolicies.PublicWrite)` and lists 429. All under `/api/expenses`, tag
`Expenses`, anonymous until the Identity module ([ADR 0015](../../../../../decisions/0015-expenses-open-until-identity.md)).

## Changed operations

| operationId | Route | Change |
| --- | --- | --- |
| `createExpense` | `POST /api/expenses` | body `ExpenseRequest` gains optional `outsideLimits` (bool, default false); fingerprint includes it |
| `updateExpense` | `PUT /api/expenses/{id}` | same body change |
| `listExpenses` | `GET /api/expenses` | `ExpenseItem` gains `outsideLimits` |
| `getBudget` | `GET /api/expenses/budget` | `BudgetView` gains `limitsFrom` (date) |
| `updateBudgetSettings` | `PUT /api/expenses/budget/settings` | `BudgetSettingsRequest` gains optional `limitsFrom` (date); omitted keeps the stored value |
| `getMonthSummary` | `GET /api/expenses/summary?year&month` | `MonthSummary` gains `carriedIn`, `carriesOut`, `outsideLimitsCount`, `outsideLimitsAmount`, `forecast` (`MonthForecast`: `planned`, `expectedExpenses`, `expectedResult`, `expectedTotalLeft`); `LimitUsage` gains `carriedIn`, `carriesOut`, `planned` |

All additions are new fields; older clients ignore them (backward compatible). The web regenerates its client in the
same change ([web ADR 0003](../../../../../../../web/docs/decisions/0003-typed-client-from-openapi.md)).

## New operations

| operationId | Route | Request | Success | Errors |
| --- | --- | --- | --- | --- |
| `listPlannedExpenses` | `GET /api/expenses/planned?year&month` | `year` 2000–2100, `month` 1–12 | 200 `PlannedExpenseList` `{ items, totalAmount }`; item `{ id, year, month, name, categoryId, categoryName, payer, amount, version }`, ordered by name | 400 |
| `createPlannedExpense` | `POST /api/expenses/planned` | header `Idempotency-Key`; body `PlannedExpenseRequest` `{ year, month, name, categoryId, payer, amount }` | 201 `{ id }`, `Location: /api/expenses/planned/{id}` | 400, 404 category, 409 key reused, 429 |
| `updatePlannedExpense` | `PUT /api/expenses/planned/{id}` | header `If-Match: <version>`; same body | 204 | 400 (no version), 404, 409 changed meanwhile, 429 |
| `deletePlannedExpense` | `DELETE /api/expenses/planned/{id}` | header `If-Match: <version>` | 204 | 400 (no version), 404, 409 changed meanwhile, 429 |
| `recordPlannedExpense` | `POST /api/expenses/planned/{id}/record` | header `If-Match: <plan version>`; body `RecordPlannedExpenseRequest` `{ date, paymentMethod, comment? }` | 201 `{ id }` of the new expense, `Location: /api/expenses/{id}` | 400, 404 plan, 409 changed meanwhile, 429 |

`If-Match` takes the version as is, quoted or not, `W/` ignored (`EntryVersionHeader`); a missing header becomes 0
and the validator answers 400.

## Idempotency and repeats

| Operation | Repeat behaviour |
| --- | --- |
| create plan | `Idempotency-Key` stored in `create_requests` (kind `planned`) in the same transaction; same key + same data → first id; other data → 409 `expenses.idempotency_key_reused`; racing copy hits the primary key and is answered like a repeat |
| update plan, update settings, update expense | target values plus version check; a repeat with the old version → 409 |
| delete plan | version check; repeat → 404 |
| record plan | state check: the plan must exist with that version; a repeat → 404, never a second expense |

## Error codes

Existing: `expenses.category_not_found`, `expenses.changed_meanwhile`, `expenses.idempotency_key_reused`,
`expenses.budget_settings_invalid`, amount and date codes of `EntryErrors`. New: `expenses.planned_name_required`,
`expenses.planned_name_too_long`, `expenses.planned_month_invalid`, and a not-found code for plans
(`expenses.planned_expense_not_found`, proposed).
