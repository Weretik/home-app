# Gifts — task graph

Stack names below (EF Core, Mediator, FluentValidation, xUnit) are the ones this repository uses. IDs: `R-*` rules and
`SC-*` scenarios in [requirements](../requirements/wishlist.md); `TS-*` one implementation responsibility; `EN-*` a
shared prerequisite. Phases order the planning; `Depends on` orders the work.

Every `TS-*` with behaviour follows: smallest focused test → behavioural Red → minimal Green → refactor → affected
regression set → evidence. `EN-*` tasks without behaviour of their own name a substitute check.

## Tasks

| ID | Phase | Responsibility | Covers | Depends on | Exact paths (under `api/`) | Test level |
| --- | --- | --- | --- | --- | --- | --- |
| EN-001 | 00/02 | Scaffold the five projects, `IGiftsDbContext`, `GiftsDbContext` (schema `gifts`, own history), registration in `Program.cs`, `MediatorSetup.cs`, `Host.Api.csproj`, `HomeApp.slnx`, `UnitTests.csproj` | all | — | `src/Modules/Gifts/*`, `src/Host/Host.Api/*`, `HomeApp.slnx` | architecture tests (substitute check: they find the module by folder) |
| TS-001 | 01 | `Gift` aggregate: fields, money/link/text rules, variants ≤ 20 replaced as a whole | SC-001, SC-003 | EN-001 | `Gifts.Domain/Wishlist/*`; `tests/UnitTests/Gifts/Domain/GiftTests.cs` | domain |
| TS-002 | 01 | `Gift.ChangeStatus`: gifted day rules; `HouseholdDay` (Kyiv) | SC-005 | TS-001 | `Gift.cs`, `Gifts.Application/HouseholdDay.cs`; `GiftStatusTests.cs` | domain, application |
| TS-003 | 01 | `Category` and `Detail` aggregates, `Recipient` | SC-008, SC-009 | EN-001 | `Gifts.Domain/Categories/*`, `Gifts.Domain/People/*`; `CategoryAndDetailTests.cs` | domain |
| EN-002 | 02 | EF mappings: gifts with owned variants (cascade), categories (seed), details, create requests; xmin versions; variant-only change marks the gift modified | SC-001…SC-010 | TS-001, TS-003 | `Gifts.Infrastructure/Persistence/**`, `GiftsDbContext.cs` | integration (substitute: migration diff reviewed) |
| EN-003 | 02 | `Initial` migration (schema, tables, indexes, 11 categories) and the `gifts` bundle in the deploy workflow | SC-008 | EN-002 | `Persistence/Migrations/*`, `.github/workflows/api.yml` | integration (migrations applied on start) |
| TS-004 | 03 | `CreateGift`, `UpdateGift`, `DeleteGift` with validators, `Idempotency/`, `VariantDraftValidator` | SC-001…SC-004, SC-007 | TS-001, EN-002 | `Gifts.Application/Features/Items/Commands/{CreateGift,UpdateGift,DeleteGift}`, `Idempotency/*`; `tests/UnitTests/Gifts/Input/GiftInputValidatorTests.cs` | application, integration |
| TS-005 | 03 | `ChangeGiftStatus` | SC-005 | TS-002, EN-002 | `Features/Items/Commands/ChangeGiftStatus/*` | application, integration |
| TS-006 | 03 | `ListGifts` (active/archive, order, variants, category name) | SC-001, SC-005, SC-006 | EN-002 | `Features/Items/Queries/ListGifts/*` | integration |
| TS-007 | 03 | Category use cases (list, create, rename, delete with in-use check) | SC-008 | TS-003, EN-003 | `Features/Categories/**` | application, integration |
| TS-008 | 03 | Detail use cases (list, create, update, delete) | SC-009 | TS-003, EN-002 | `Features/Details/**`; `SettingsInputValidatorTests.cs` | application, integration |
| TS-009 | 03 | `GetGiftsSummary` and `Report/` records | SC-010 | TS-006, TS-008 | `Features/Summary/**` | integration |
| TS-010 | 04 | Endpoints with `WithName`, `WithSummary`, `Produces*`, `PublicWrite`, `If-Match`, `Idempotency-Key`; `GiftsModule`; OpenAPI regenerated | SC-011 | TS-004…TS-009 | `Gifts.Api/**`, `docs/contracts/openapi.json` | architecture (OpenAPI tests), integration |
| TS-011 | 04 | Integration tests; split `IntegrationTests/Api` by module (folder rule) | SC-001…SC-010 | TS-010 | `tests/IntegrationTests/Api/Gifts/*`, `tests/IntegrationTests/Api/Expenses/*` | integration |
| TS-012 | 05 | Contract schema names unique across modules (found during delivery: `CategoryItem` and `CreatedEntry` merged with Expenses' schemas) | SC-011 | TS-010 | `tests/ArchitectureTests/Http/ContractSchemaNameTests.cs` | architecture |
| EN-004 | 05 | Docs: module page, ADRs 0021–0026, C4, architecture README, API README, AGENTS.md, deploy.md, specs index | — | TS-010 | `docs/**`, `../AGENTS.md`, `../docs/deploy.md` | review |

## Evidence

Behaviour was written test-first per layer where a failing test could be meaningful; the domain and validator tests
were written against the domain API before the handlers existed. Recorded commands and results (2026-11-26):

| Task(s) | Red | Green / regression |
| --- | --- | --- |
| TS-001…TS-003 | the domain tests were added with the aggregates in one step; no separate failing run was recorded (honest gap: Red not captured) | `dotnet test tests/UnitTests --filter "FullyQualifiedName~UnitTests.Gifts"` → 32 passed |
| TS-004, TS-008 (validators) | as above | included in the 32 above; full `dotnet test tests/UnitTests` → 131 passed |
| TS-004…TS-011 | integration tests written after the endpoints; Docker is not available in this environment | run against a local PostgreSQL 16 (the test host temporarily pointed at it, then restored): `dotnet test tests/IntegrationTests` → 36 passed (10 Gifts, 26 existing) |
| TS-012 | a temporary record `Gifts.Api.ExpenseRequest` made `ContractSchemaNameTests` fail with `ExpenseRequest: Expenses, Gifts`; removed | `dotnet test tests/ArchitectureTests` → 23 passed |
| TS-010 | — | `dotnet build HomeApp.slnx` → 0 warnings, 0 errors; `openapi.json` regenerated with 14 Gifts operations; Expenses paths and schemas unchanged (compared with `HEAD`) |
| EN-003 | — | migration applied on a fresh database by the test host; `\d gifts.gifts` matches the brief |

Refactor notes: the summary handler's private nested row type was replaced by an anonymous projection and a value
tuple (one type per file); the Gifts response and request records were renamed with a `Gift`/`Gifts` prefix after the
schema clash (TS-012).
