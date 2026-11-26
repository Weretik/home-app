# Name suggestions — tasks and traceability

Every behaviour task follows the template's cycle: smallest focused test → red → minimal code → green → refactor →
affected regression run.

| ID | Responsibility | Paths | Test level | Covers |
| --- | --- | --- | --- | --- |
| TS-201 | `SearchPattern.StartsWith` / `WordStartsWith` with escaping | `BuildingBlocks.Application/Persistence/` | unit (`SearchPatternTests`) | R-203 |
| TS-202 | Query and validator | `Features/Items/Queries/ListSuggestions/` | unit (`SuggestionsValidatorTests`) | R-201, R-202, SC-206 |
| TS-203 | Handler: match, group, rank, latest spelling and department, `inList` | `Features/Items/Queries/ListSuggestions/` | integration | R-203…R-208, SC-201…SC-205, SC-207 |
| TS-204 | Endpoint, mapping, regenerated `openapi.json` | `Shopping.Api/Endpoints/Items/Reading/`, `ShoppingModule.cs` | architecture (OpenAPI and endpoint metadata tests) + integration | R-209 |
| TS-205 | Integration tests | `tests/IntegrationTests/Api/Shopping/ShoppingSuggestionsApiTests.cs` | integration | SC-201…SC-207 |
| TS-206 | Docs | `docs/modules/shopping.md`, `docs/specs/README.md` | review | — |

## Traceability

| Rule / scenario | Code | Test |
| --- | --- | --- |
| R-201, R-202, SC-206 | `ListSuggestionsValidator` | `SuggestionsValidatorTests`, `ShoppingSuggestionsApiTests.WrongInput_IsAValidationProblem` |
| R-203, SC-205 | `SearchPattern`, `ListSuggestionsHandler` | `SearchPatternTests`, `ShoppingSuggestionsApiTests.Wildcards_AreLiteral` |
| R-204, R-205, SC-202, SC-203 | `ListSuggestionsHandler` (details query) | `ShoppingSuggestionsApiTests.OneSuggestionPerName_InItsLatestSpelling` |
| R-206, R-208, SC-201, SC-207 | `ListSuggestionsHandler` (top query) | `ShoppingSuggestionsApiTests.NamesStartingWithTheQuery_ComeFirst_ThenTheMostBought`, `…Limit_DefaultsToEight` |
| R-207, SC-204 | `CartWindow.InList` in the handler | `ShoppingSuggestionsApiTests.InList_MarksNeededAndCartNames` |
| R-209 | endpoint without rate limit or key | `openapi.json`, architecture tests |

## Readiness

- [x] Rules and scenarios have ids and are testable; out of scope written down.
- [x] Contract per operation with its errors; additive.
- [x] No schema change; no new lasting decision, so no ADR (patterns and `LIKE` search exist since
      [ADR 0031](../../../decisions/0031-shopping-module-model.md)).

## Delivery readiness

- [x] `dotnet build HomeApp.slnx` regenerated `openapi.json`; the web client regenerated in the same change.
- [x] Unit and architecture tests green; integration tests compiled and green against a local PostgreSQL 16
      (temporary local run, nothing of it kept; CI runs them with Docker).
- [x] Code audit done, findings fixed or recorded ([audit](code-audit/audit.md)).
