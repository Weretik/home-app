# TS-013 — Plan endpoints, request fields, regenerated OpenAPI

- **Task ID:** TS-013
- **Covers:** all SC-FC-*, SC-OL-001, SC-CO-007
- **Depends on:** TS-005 … TS-012
- **Exact paths:** `src/Modules/Expenses/Expenses.Api/Endpoints/Planned/Plans/*`, `.../Endpoints/Planned/Recording/*`, `src/Modules/Expenses/Expenses.Api/ExpensesModule.cs`, `docs/contracts/openapi.json` (relative to `api/`)
- **Test level:** architecture tests (OpenAPI conventions) + integration
- **Architecture basis:** [AGENTS.md, API contract](../../../../../../../../AGENTS.md), [ADR 0008](../../../../../../decisions/0008-openapi-generated-from-code.md)

## Work

Order: Red → Green → Refactor → Regression → evidence.

- [ ] One endpoint per file; `WithName` as in [contract](../../contracts/api-contract.md), `WithSummary`, `Produces…`, `ProducesProblem`, `PublicWrite`, 429.
- [ ] `cd api && dotnet build HomeApp.slnx` regenerates the contract; commit it; hand off to web (`npm run contracts:generate`).
- [ ] Check the changed files for one responsibility each; split a file that mixes independent responsibilities
      along the module's folders (Domain by aggregate, Application `Features/<Feature>/Commands|Queries/<UseCase>/`,
      Api by feature); keep the six-files-per-folder rule.

## Evidence

- Test and assertion:
- Red command and expected behavioral failure:
- Green command and result:
- Refactor note and re-run:
- Regression command and result:
- Deviation or blocker:

## Done when

`dotnet test api/tests/ArchitectureTests` passes and the committed `openapi.json` equals the build output.
