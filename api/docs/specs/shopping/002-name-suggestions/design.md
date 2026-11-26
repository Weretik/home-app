# Name suggestions — design

## Use case

`Features/Items/Queries/ListSuggestions/` in `Shopping.Application`, beside `ListFrequent`:

| Type | Role |
| --- | --- |
| `ListSuggestionsQuery(string? Query, int Limit)` | the request; `Limit` defaults to 8 at the endpoint |
| `ListSuggestionsValidator` | R-201 on the normalised query (property `query`), R-202 |
| `ListSuggestionsHandler` | the reads below |
| `NameSuggestion` | the answer row |

No domain type changes: matching is a `LIKE` on the stored `normalized_name`, normalisation is the existing
`NameNormalizer`, "in the list" the existing `CartWindow.InList` with the cutoff of `ItemWindow`.

### Patterns

`BuildingBlocks.Application.Persistence.SearchPattern` gets two functions next to `ContainsEachWord`, with the same
escaping (`\`, `%`, `_`) and escape character:

- `StartsWith(text)` → `text%` (the whole name starts with it);
- `WordStartsWith(text)` → `% text%` (a later word starts with it; normalised names have single spaces).

Both lower-case the text like `ContainsEachWord`. They are general (any module's prefix search), so they live in the
building block, not in Shopping.

### Queries

1. **Top names** — on `shopping_items`, rows whose `normalized_name` is `LIKE` either pattern, grouped by
   `normalized_name`: `starts` = the key `LIKE` the first pattern, `times` = count of `Bought` rows, `last` = max
   `added_at_utc`; ordered `starts desc, times desc, last desc, key`; `LIMIT limit`.
2. **Details of those names** — for the (at most 20) keys: the latest row per key with its department joined in
   SQL (name, department id, name and icon) through
   `GroupBy(...).Select(g => g.OrderByDescending(added).ThenByDescending(id).First())` (EF Core turns it into a
   `ROW_NUMBER()` window). One statement, so a department deleted meanwhile cannot leave a row without one; a name
   whose rows were deleted between the reads is left out.
3. **In the list** — the keys that have a row in the list (`CartWindow.InList(cutoff)`), `DISTINCT`.

The cutoff is read through `ItemWindow.ReadAsync` (settings and "now"), as every other item query.

### Cost

The `LIKE 'x%'` predicates cannot use the btree index `ix_shopping_items_normalized_name` under the database's
default collation, and `'% x%'` cannot use any btree index: the first query scans `shopping_items`. The table holds
one household's purchases (the imported sheet plus a few dozen rows a week: thousands of rows over years), which PostgreSQL scans in
well under 10 ms. The second query uses `ix_shopping_items_normalized_name` for the at most 20 keys. A trigram index
would serve both patterns but needs `pg_trgm`, which production refuses; if the table ever reaches hundreds of
thousands of rows, a `text_pattern_ops` index on `normalized_name` serves the prefix case. Recorded as accepted
in the [audit](code-audit/audit.md). No migration.

## HTTP contract

| Operation | Route | Request | Answers |
| --- | --- | --- | --- |
| `listShoppingNameSuggestions` | `GET /api/shopping/items/suggestions` | `query` (string, required by validation), `limit` (int, default 8) | `200` `NameSuggestion[]` · `400` validation problem |

`NameSuggestion`:

| Field | Type | Meaning |
| --- | --- | --- |
| `name` | string | the latest spelling (R-205) |
| `departmentId`, `departmentName`, `departmentIcon` | uuid, string, string | the latest row's department |
| `timesBought` | int | R-206 |
| `lastAddedAtUtc` | date-time | R-206 |
| `inList` | bool | R-207 |

The literal segment `suggestions` does not clash with `/items/{id}`: that route has no `GET`, and ASP.NET Core
prefers literal segments. `WithName`, `WithSummary`, `Produces<IReadOnlyList<NameSuggestion>>` and
`ProducesValidationProblem` as every endpoint. Consumer: the web quick add
([web 002](../../../../../web/docs/specs/shopping/002-name-suggestions/README.md)); the web client is regenerated in
the same change. Additive: no existing operation changes.

## Data

No schema change. Reads `shopping.shopping_items` (`normalized_name`, `name`, `department_id`, `status`,
`added_at_utc`, `bought_at_utc`, `cleared_at_utc`, `id`), `shopping.departments`, `shopping.shopping_settings`.
