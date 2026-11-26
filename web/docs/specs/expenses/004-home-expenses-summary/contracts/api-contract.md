# Home expenses summary — API contract (as implemented)

Source: `api/docs/contracts/openapi.json` at `8378e4a`.

| operationId       | Method / path                          | Scenarios       | Consumer                               |
| ----------------- | -------------------------------------- | --------------- | -------------------------------------- |
| `getMonthSummary` | `GET /api/expenses/summary?year&month` | SC-W4-002 … 006 | `expenses-summary/data/summary-api.ts` |

Mapping: `limitRows(summary)` turns `limits` into rows and adds «ИТОГО» from `expenses`, `monthlyLimit`, `daysLeft`
(computed in the web, GAP-W09).
