# 002 — Name suggestions for the quick add

- **Module:** `Shopping` (`api/src/Modules/Shopping`), schema `shopping`
- **Type:** feature (a new read use case in an existing module)
- **Architecture source:** [architecture](../../../architecture/README.md), [rules](../../../rules.md), module page
  [Shopping](../../../modules/shopping.md), feature [001](../001-shopping-list/README.md)
- **Status:** implemented, audited
- **Owner:** Vitaliy · **Created:** 2026-11-26 · **Updated:** 2026-11-26
- **Frontend SDD:** [web/docs/specs/shopping/002-name-suggestions](../../../../../web/docs/specs/shopping/002-name-suggestions/README.md)

While Masha or Vitaliy types in the «Покупки» quick-add field, the site suggests names from everything ever added to
the list. The history is already there: bought rows stay in `shopping.shopping_items`. This feature adds one query
that reads it; nothing is stored or changed.

Written from the backend SDD template (Weretik/obsidian `Templates/backend-sdd/feature`), condensed to one page per
part, like [todos 001](../../todos/001-todos-module/README.md).

## State

- Specification: accepted · Implementation: done · Verification: unit and architecture tests green, integration tests
  compiled and run against a local PostgreSQL 16 (CI runs them on the pull request) · Audit:
  [code-audit/audit.md](code-audit/audit.md) · Blockers: none

## Parts

- [Requirements: scope, rules and scenarios](requirements.md)
- [Design: query, contract, data](design.md)
- [Tasks, traceability and readiness](tasks.md)
- [Code audit](code-audit/audit.md)

## File map

Paths under `api/`.

| Path | State | One responsibility | Task |
| --- | --- | --- | --- |
| `src/BuildingBlocks/BuildingBlocks.Application/Persistence/SearchPattern.cs` | changed | adds `StartsWith` and `WordStartsWith` (escaped `LIKE` prefixes) | TS-201 |
| `src/Modules/Shopping/Shopping.Application/Features/Items/Queries/ListSuggestions/` | created | `ListSuggestionsQuery`, its validator, handler and `NameSuggestion` | TS-202, TS-203 |
| `src/Modules/Shopping/Shopping.Api/Endpoints/Items/Reading/ListShoppingNameSuggestionsEndpoint.cs` | created | `GET /api/shopping/items/suggestions` | TS-204 |
| `src/Modules/Shopping/Shopping.Api/ShoppingModule.cs` | changed | maps the endpoint | TS-204 |
| `docs/contracts/openapi.json` | regenerated | the new operation and `NameSuggestion` schema | TS-204 |
| `tests/UnitTests/BuildingBlocks/SearchPatternTests.cs`, `tests/UnitTests/Shopping/Input/SuggestionsValidatorTests.cs` | changed / created | patterns, validation | TS-201, TS-202 |
| `tests/IntegrationTests/Api/Shopping/ShoppingSuggestionsApiTests.cs` | created | end-to-end matching, ranking, `inList`, errors | TS-205 |
| `docs/modules/shopping.md`, `docs/specs/README.md` | changed | module page, specification index | TS-206 |

## Change history

- 2026-11-26: first version (Vitaliy: suggest names from everything ever added while typing in «Покупки»).
