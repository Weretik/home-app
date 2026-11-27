# API contract use

From [api 006 contract](../../../../../../api/docs/specs/expenses/006-limit-amounts-by-month/contracts/api-contract.md),
read through the generated client only.

| Operation                              | Read / sent by the web                                                                          |
| -------------------------------------- | ----------------------------------------------------------------------------------------------- |
| `GET /api/expenses/budget`             | `monthlyLimit`, `monthlyLimits[]`, `limits[].amount`, `limits[].amounts[]` (`{ from, amount }`) |
| `PUT /api/expenses/budget/limits/{id}` | adds `amountFrom` (`YYYY-MM-01`)                                                                |
| `POST /api/expenses/budget/limits`     | unchanged (no `amountFrom`)                                                                     |
| `PUT /api/expenses/budget/settings`    | adds `monthlyLimitFrom` (`YYYY-MM-01`)                                                          |

A 400 with an `amountFrom` / `monthlyLimitFrom` error shows under the month field through the existing problem
mapping.
