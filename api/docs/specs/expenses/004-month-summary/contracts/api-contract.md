# Month summary — API contract (as implemented)

**Status:** implemented. Machine contract: `api/docs/contracts/openapi.json`.

| operationId | Route | Request | Success | Errors |
| --- | --- | --- | --- | --- |
| `getMonthSummary` | `GET /api/expenses/summary` | `year` 2000–2100, `month` 1–12 | 200 `MonthSummary` (see [data model](../data-model.md)) | 400 |

Read only, anonymous, no rate limit policy (reads are not limited by ADR 0012). Consumer: the web home page (web 004).
