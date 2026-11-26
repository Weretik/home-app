# Name suggestions — final backend code audit

- **Date:** 2026-11-26
- **Kind:** final feature audit (skill `architecture:backend-code-audit`)
- **Branch:** `claude/project-thread-g4hajn`, uncommitted working tree
- **Result:** no open blocker. 2 findings fixed, 5 accepted with a reason.

## Scope

- `src/Modules/Shopping/Shopping.Application/Features/Items/Queries/ListSuggestions/*` (query, validator, handler,
  `NameSuggestion`), `Shopping.Api/Endpoints/Items/Reading/ListShoppingNameSuggestionsEndpoint.cs`,
  `ShoppingModule.cs`.
- `src/BuildingBlocks/BuildingBlocks.Application/Persistence/SearchPattern.cs` (two new functions).
- `docs/contracts/openapi.json` (generated), tests (`SearchPatternTests`, `SuggestionsValidatorTests`,
  `ShoppingSuggestionsApiTests`), docs (`modules/shopping.md`, `specs/README.md`, this SDD).
- Not in scope: `web/` (audited in [web 002](../../../../../../web/docs/specs/shopping/002-name-suggestions/code-audit/audit.md)).

## Method

1. Read the SDD (R-201…R-209, SC-201…SC-207, design, tasks), `AGENTS.md`, `docs/rules.md`, the module page and the
   existing `ListFrequent` / `ListKeywords` queries as the reference.
2. Read every changed file line by line; checked layers, folder sizes, validation, error mapping and the contract.
3. Ran the integration tests against a local PostgreSQL 16 (Docker is not available here) by pointing `ApiFactory`
   at it for the run only; the change was reverted afterwards (`git status` shows `ApiFactory.cs` unchanged).
   `EXPLAIN ANALYZE` of the top-names statement on that database.
4. Went through the checklists of the skill: new feature, endpoint gate, database, idempotency, folder axiom.

## Step 0: quality targets

[`docs/architecture/quality-targets.md`](../../../../architecture/quality-targets.md): read P95 / P99 300 / 800 ms
(proposed, not measured), peak under 1 request per second for 2 people, data volume stated for Expenses only.
This feature is on the quick-add path: at most one request per 200 ms typing pause per person, so a few requests a
second at the very peak. Expected work per request: four short statements (top names, latest rows, settings,
in-list keys). On the local database the top-names statement took 0.15 ms over 66 rows with a sequential scan;
at the household's few thousand rows it stays in the low milliseconds, far inside the read target. No verdict on
production P95 (not measured; AF-SG-005).

## Findings

| Id | Severity | Rule | Evidence | Status |
| --- | --- | --- | --- | --- |
| AF-SG-001 | Medium | Database checklist: no read that can fail half-way between two statements; errors are problems, never `500` | The first version read the latest rows, then all departments in a separate query and indexed `departments[row.DepartmentId]` and `latest[t.Key]`: a department deleted (its rows move to the fallback) or a name's rows deleted between the statements threw `KeyNotFoundException` → `500`. | **Fixed.** The latest-row query joins `departments` in SQL (one statement, one snapshot), and a name whose rows vanished between the top-names and the details read is left out (`ListSuggestionsHandler.cs`, the `Where(latest.ContainsKey)`). Integration tests green after the change (97/97 on local PostgreSQL 16). |
| AF-SG-002 | Low | Reads: fewest round trips | The first version made five statements per request (top names, latest rows, settings, in-list, departments). | **Fixed** with AF-SG-001: four statements; the settings read stays (`ItemWindow`, shared by every item query). |
| AF-SG-003 | Low | Database rules: filtered lists use an index | `LIKE 'x%'` under the default collation and `LIKE '% x%'` cannot use `ix_shopping_items_normalized_name`; the top-names statement is a sequential scan (`EXPLAIN ANALYZE`: Seq Scan, 0.15 ms on 66 rows). | **Accepted.** One household's purchases, thousands of rows over years (production row count not checked here); a trigram index needs `pg_trgm`, which production refuses ([ADR 0031](../../../../decisions/0031-shopping-module-model.md)); a `text_pattern_ops` index serves the prefix case if the table ever grows by orders of magnitude. Recorded in the design and the module page. |
| AF-SG-004 | Low | Endpoint gate: a rate limit decision for every anonymous endpoint | `GET /items/suggestions` has no rate limit policy. | **Accepted.** No read in the project has one (only `PublicWrite` and `SignIn` exist, no global limiter); the read is cheap and side-effect free; the web asks once per 200 ms pause. Revisit with sign-in ([ADR 0015](../../../../decisions/0015-expenses-open-until-identity.md)). |
| AF-SG-005 | Info (project) | Step 0: measured P95 / P99 | No current value in `quality-targets.md`. | **Accepted for this feature.** Project-wide, recorded there as "not measured"; measure with the App Service metrics after the release. |
| AF-SG-006 | Info | R-206 «times bought» | Re-adding a name that is in the cart turns the same row back to `Needed` (rule «Re-add» of the module), so that purchase no longer counts in `timesBought`. | **Accepted.** The same count as «⭐ Частые»; the order of suggestions, not an exact statistic. Written in the module page («counts its `Bought` rows»). |
| AF-SG-007 | Info | Building blocks hold no module rules | `SearchPattern.StartsWith` / `WordStartsWith` added to `BuildingBlocks.Application/Persistence`. | **Accepted.** General escaped-`LIKE` helpers beside `ContainsEachWord`, no Shopping knowledge; unit tested. |

## Checked and in order

- **Module and layers:** the query lives in Shopping, which owns `shopping_items`; Application uses only
  `IShoppingDbContext`, `NameNormalizer`, `CartWindow`, `ItemWindow`; no HTTP type in Application; the endpoint maps
  query string to `ListSuggestionsQuery` and the result through `ToHttpResult` (the one error mapper). No other
  module touched; `Shopping.Contracts` still empty.
- **Folders:** `Features/Items/Queries/ListSuggestions/` holds 4 files (request, handler, validator, response, flat);
  `Endpoints/Items/Reading/` 5 files; `tests/UnitTests/Shopping/Input/` 3; `tests/IntegrationTests/Api/Shopping/` 6.
  The folder architecture test is green.
- **Validation:** `ListSuggestionsValidator` checks the normalised query (1–80) and the limit (1–20) — shape only, no
  business state; `400` with field errors `query` / `limit` (integration test).
- **Reads:** `AsNoTracking`; grouping, ranking and `LIMIT` in SQL (the full order `starts desc, times desc, last
  desc, key`); the details read touches at most 20 keys; no query in a loop; no whole table loaded into memory.
- **Escaping:** `%`, `_` and `\` in the query are literal (unit and integration tests).
- **Writes, transactions, idempotency, outbox:** none; a pure read (R-209), so the idempotency checklist has no item.
- **Contract:** `listShoppingNameSuggestions`, unique camelCase name, `WithSummary`,
  `Produces<IReadOnlyList<NameSuggestion>>`, `ProducesValidationProblem`; additive; `openapi.json` regenerated by the
  build and the web client regenerated in the same change (architecture OpenAPI tests green).
- **Route:** `/items/suggestions` is a literal segment; `/items/{id}` has no `GET`, so no ambiguity (integration
  tests call both).
- **ADR and C4:** no new module, container, external system or way of running work; the module's component view is
  unchanged in shape (one more query), so no ADR or C4 change.
- **Docs:** module page (rules, use cases, HTTP table, database note, tests) and the specs index match the code.

## Verification (2026-11-26, after the fixes)

| Check | Result |
| --- | --- |
| `cd api && dotnet build HomeApp.slnx` | succeeded, 0 warnings, 0 errors |
| `dotnet test tests/UnitTests` | 336 passed |
| `dotnet test tests/ArchitectureTests` | 72 passed |
| `dotnet test tests/IntegrationTests` against local PostgreSQL 16 (temporary `ApiFactory` switch, reverted) | 97 passed (`ShoppingSuggestionsApiTests`: 10) |
| CI with Docker | runs on the pull request |
