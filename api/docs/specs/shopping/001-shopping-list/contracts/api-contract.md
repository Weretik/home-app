# Shopping list — HTTP contract

**Status:** agreed (product brief of 2026-11-26)

The machine contract is generated from the code into
[`api/docs/contracts/openapi.json`](../../../../contracts/openapi.json) on every build; this page records the
decisions. Group `/api/shopping`, tag `Shopping`, anonymous (ADR 0015 applies the same way), every write opts into
`PublicWrite` and lists 429.

## Conventions

- Validation errors: 400 problem with `errors` by field; unknown ids 404 (`shopping.item_not_found`,
  `shopping.department_not_found`, `shopping.keyword_not_found`); 409 for `shopping.changed_meanwhile`,
  `shopping.idempotency_key_reused`, `shopping.department_name_taken`, `shopping.keyword_taken`,
  `shopping.fallback_department`.
- `If-Match: "<version>"` (quoted or not, `W/` ignored) on PUT/DELETE of items, departments, keywords and settings;
  missing → 400.
- `Idempotency-Key` (≤100) on POST creates; missing → 400; repeat → first result; other data → 409.
- Enums as strings: `status` `Needed|Bought`, `addedBy`/`by`/`boughtBy` `Vitaliy|Maria`.

## Operations

| operationId | Method, path | Request | Response |
| --- | --- | --- | --- |
| listShoppingItems | GET /items | – | 200 `ShoppingList { items }` |
| addShoppingItems | POST /items + Idempotency-Key | `{ text, addedBy, departmentId?, urgent? }` | 201 `AddItemsResult { added, restored, updated }` |
| updateShoppingItem | PUT /items/{id} + If-Match | `{ name, quantity?, note?, departmentId, urgent }` | 200 `ShoppingItemView` |
| setShoppingItemStatus | PUT /items/{id}/status | `{ status, by }` | 200 `ShoppingItemView` |
| deleteShoppingItem | DELETE /items/{id} + If-Match | – | 204 |
| clearShoppingCart | POST /items/clear-cart | – | 200 `ClearCartResult { cleared }` |
| listShoppingHistory | GET /history?page=1&pageSize=50 | – | 200 `ShoppingHistoryPage { items, page, pageSize, totalCount }` |
| listFrequentItems | GET /frequent | – | 200 `FrequentItem[]` |
| getShoppingSummary | GET /summary | – | 200 `ShoppingSummary` |
| listShoppingDepartments | GET /departments | – | 200 `DepartmentItem[]` |
| matchShoppingDepartment | GET /departments/match?name= | – | 200 `DepartmentMatch` |
| createShoppingDepartment | POST /departments + Idempotency-Key | `{ name, icon? }` | 201 `ShoppingCreatedId { id }` |
| updateShoppingDepartment | PUT /departments/{id} + If-Match | `{ name, icon? }` | 204 |
| deleteShoppingDepartment | DELETE /departments/{id} + If-Match | – | 204 |
| reorderShoppingDepartments | PUT /departments/order | `{ departmentIds }` | 204 |
| listShoppingKeywords | GET /keywords?departmentId&search | – | 200 `KeywordItem[]` |
| createShoppingKeyword | POST /keywords + Idempotency-Key | `{ keyword, departmentId }` | 201 `ShoppingCreatedId` |
| updateShoppingKeyword | PUT /keywords/{id} + If-Match | `{ keyword, departmentId }` | 204 |
| deleteShoppingKeyword | DELETE /keywords/{id} + If-Match | – | 204 |
| getShoppingSettings | GET /settings | – | 200 `ShoppingSettingsView { cartKeepHours, frequentDays, version }` |
| updateShoppingSettings | PUT /settings + If-Match | `{ cartKeepHours, frequentDays }` | 204 |

`ShoppingItemView`: `id, name, quantity?, note?, departmentId, departmentName, departmentIcon, departmentSortOrder,
urgent, status, addedBy, addedAtUtc, boughtBy?, boughtAtUtc?, version`.

Deviation from the brief: history paging uses `page`/`pageSize` like `listExpenses` (the brief's "follow the Expenses
list paging pattern") instead of `before`/`limit`. Keyword search splits the input into words, each an escaped
`LIKE` on the keyword column (trigram-indexed).

## Repeats

| Operation | Repeat behaviour |
| --- | --- |
| addShoppingItems | Idempotency-Key; the stored outcome is the ids per group; a repeat re-reads those items |
| createShoppingDepartment / createShoppingKeyword | Idempotency-Key; first id |
| setShoppingItemStatus | target value, no key needed |
| clearShoppingCart | second call clears 0 |
| reorderShoppingDepartments | full target order; concurrent reorders → 409 via row versions |
| updates / deletes | If-Match version; delete of a missing row 404 |

## Compatibility

New paths only; no existing operation changes. The web client is regenerated with `npm run contracts:generate`.
