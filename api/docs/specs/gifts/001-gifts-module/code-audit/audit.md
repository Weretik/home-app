# Code responsibility audit — Gifts module

- **Feature:** `api/docs/specs/gifts/001-gifts-module`
- **Scope:** the whole module `api/src/Modules/Gifts` (five projects, migration), its host registration
  (`Program.cs`, `MediatorSetup.cs`, `Host.Api.csproj`, `HomeApp.slnx`), its tests (`tests/UnitTests/Gifts`,
  `tests/IntegrationTests/Api/Gifts`, the moved `tests/IntegrationTests/Api/Expenses`, the new
  `tests/ArchitectureTests/Conventions/ContractSchemaNameTests.cs`), `api/docs/contracts/openapi.json`, the deploy
  workflow's migration bundles, and the docs this change touched.
- **Stage:** final audit after implementation (skill `backend-code-audit`, run once for the whole feature).
- **Date:** 2026-11-26
- **Verdict:** **pass with open findings.** No blocker inside the feature. Two findings were fixed during the audit
  (AF-002, AF-003). One High finding (AF-001) is repository-wide and outside this feature's scope; the others are Low
  and accepted with a reason.

## Step 0: quality targets

The repository has no quality-targets file (`docs/sdd/architecture/overview/quality-targets.md` or an equivalent under
`api/docs`). Without targets no speed or scaling verdict is given; the feature's own load is stated instead: ~100 gifts
(91 imported), at most a few hundred, ≤ 20 variants each, tens of details, two users. Every read is one or two indexed
queries over that volume. See AF-001 for the proposal.

## Before implementation: actual base

- [x] Files found: the Expenses module as the reference (`src/Modules/Expenses/**`), building blocks
  (`ModuleDbContext`, `UniqueConstraintViolationException`, `ResultHttpExtensions`, `RateLimitPolicies`,
  `LoggingBehavior`), the host's composition, the architecture tests (`Repository.Modules` finds modules by folder).
- [x] Ownership: invariants in Domain aggregates; orchestration in Application handlers; shape validation in
  FluentValidation validators; persistence and mapping in Infrastructure; HTTP binding and mapping in Api endpoints;
  composition in `Host.Api`.
- [x] The planned map is in the SDD [README](../README.md#planned-file-map); this audit refers to it.

## After implementation: whole scope

- [x] All feature code reviewed, not only the diff (including the moved Expenses integration tests).
- [x] Dependencies: `Gifts.Domain` → `BuildingBlocks.Domain` only; `Gifts.Application` → own Domain, own Contracts,
  `BuildingBlocks.Application`; `Gifts.Infrastructure` → own Application, `BuildingBlocks.Infrastructure`; `Gifts.Api`
  → own Application and Infrastructure, `BuildingBlocks.Api`; host references only `Gifts.Api`. No reference to
  Expenses anywhere in the module (`grep` empty). `ModuleBoundaryTests`, `LayerDependencyTests` pass.
- [x] Every use case is a folder `Features/<Area>/Commands|Queries/<UseCase>/` with its message, handler and validator
  (validators for every message with properties; `UseCaseRulesTests` pass). Responses sit next to their query, or in
  `Summary/Report/` like Expenses.
- [x] Handlers follow load → outside check (category exists) → aggregate method → one `SaveChanges` → id/DTO; no
  entity is returned. Deletes are a single `ExecuteDelete` (gift, detail) or one `SaveChanges` (category, to catch the
  foreign key).
- [x] Business rules live in `Gift`, `Category`, `Detail` (private setters, static factories returning `Result`;
  `DomainEncapsulationTests` pass). `GiftVariant` has no `IAggregateRoot` and no `DbSet`.
- [x] Persistence: own schema `gifts` and history table; foreign keys only inside the schema; no query or save in a
  loop; reads `AsNoTracking` with projections.
- [x] Folder axiom: largest folders hold 6 code files (`Gifts.Domain/Wishlist`, `Gifts.Api/Endpoints/Items`); no
  empty, wrapper or one-file folder; `tests/IntegrationTests/Api` was split by module because the Gifts tests would
  have made it 8 files. `FolderRulesTests` pass.
- [x] Tests rerun after the structural changes (commands below).

## Decisions by path

| Path or area | Responsibility and evidence | Decision and reason | New paths / boundaries | Check |
| --- | --- | --- | --- | --- |
| `Gifts.Domain/Wishlist/Gift.cs` | all gift rules incl. variants and status (≈190 lines) | keep whole: one aggregate, one set of invariants; splitting the private checks out would add a type without a second user | — | `GiftTests`, `GiftStatusTests` |
| `Gifts.Domain/People/` | `Recipient` shared by gifts and details, plus `Detail` | keep: `Recipient` is about people, `Detail` is per person; a one-file `Parties/` folder is not allowed | — | build, `FolderRulesTests` |
| `Gifts.Application/HouseholdDay.cs` | Kyiv "today" | keep in the module: Expenses has the same few lines inline; extract to building blocks at the third user (rule of three) | — | `GiftStatusTests.Today_IsTheDateInKyiv` |
| `Gifts.Api/GiftVersionHeader.cs` | `If-Match` parsing, a copy of Expenses' `EntryVersionHeader` | keep for now (AF-005) | — | integration tests |
| `Gifts.Infrastructure/GiftsDbContext.cs` | marks a gift modified when only its variants changed | keep in Infrastructure: it is an EF concern that protects the aggregate's version | — | `GiftsApiTests.Changing_Only_The_Variants_MovesTheVersion` |
| `Gifts.Application/Features/Categories/CategoryNames.cs` | name of the unique index, shared by handler and mapping | keep: one source of the constraint name | — | `GiftsSettingsApiTests` (409 name taken) |
| `GetGiftsSummaryHandler.cs` | summary in memory over one projection | changed during audit: nested private record removed (AF-002) | — | integration summary test |
| response and request records | schema names in OpenAPI | changed during audit: prefixed with `Gift`/`Gifts` (AF-003) | `ContractSchemaNameTests` | architecture tests |
| `tests/IntegrationTests/Api/` | 6 files + Gifts would exceed the folder limit | split by module: `Api/Expenses/` (4 moved, namespace `IntegrationTests.Api.Expenses`), `Api/Gifts/` (3 new), `Cors`/`Health` stay | — | `FolderRulesTests`, all 36 integration tests |

## Database checklist

| Use case | Commits | Race guard | If it stops half-way |
| --- | --- | --- | --- |
| `CreateGift` | 1 (`gifts`, `gift_variants`, `create_requests` together) | PK on `create_requests.key` → answered as a repeat | nothing stored |
| `UpdateGift` | 1 (gift row + variant delete/insert) | `xmin` (version check before and in the `UPDATE`) | nothing changed |
| `ChangeGiftStatus` | 1 | `xmin` | nothing changed |
| `DeleteGift` | 1 `DELETE` (variants by cascade) | none needed | — |
| `CreateCategory` | 1 | PK on key; unique `ux_categories_name` → 409 | nothing stored |
| `RenameCategory` | 1 | unique name → 409 | nothing changed |
| `DeleteCategory` | 1 | FK `restrict` → 409 | nothing changed |
| `CreateDetail` / `UpdateDetail` / `DeleteDetail` | 1 each | PK on key / `xmin` / — | nothing stored / changed |

Indexes: `ix_gifts_status` (both lists), `ix_gifts_category_id` (FK and in-use check), `ix_gift_variants_gift_id`,
`ux_categories_name`, `ix_details_recipient_sort_order`. Lists are unpaged by a recorded decision (volume above). No
search box, so no trigram index. Migration `Initial` only adds objects (expand only).

## Idempotency checklist

| Write | Repeat today |
| --- | --- |
| `POST /items`, `/categories`, `/details` | `Idempotency-Key` stored in the same transaction under the PK; same data → first id; other data → 409; race → first id |
| `PUT /items/{id}`, `/details/{id}` | target values; a repeat with the old version → 409 (data already as wanted) |
| `PUT /items/{id}/status` | target value; a repeated «Подарено» keeps the gifted day |
| `PUT /categories/{id}` | target value, no version |
| `DELETE …` | second call → 404, no other effect |
| outbox, webhooks, jobs | none in this module |

## Findings

| ID | Severity | Status | Path | Evidence | Impact | Owner | Decision / target change | Verification |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| AF-001 | High | open (outside feature scope) | repository (`api/docs`) | no quality-targets file | no number to judge latency, peak, availability, cost against | Vitaliy | add `api/docs/architecture/quality-targets.md`. Proposed, to confirm: P95 < 300 ms and P99 < 1 s for reads, P95 < 500 ms for writes (measured with App Service logs); peak 2 concurrent users, 5 req/s; data ≤ 10 000 rows per table; availability best effort on F1; RPO 24 h (provider backups), RTO 1 day; cost 0 €/month extra | the file exists with measured current values |
| AF-002 | Low | fixed | `Gifts.Application/Features/Summary/Queries/GetGiftsSummary/GetGiftsSummaryHandler.cs` | a private nested record in the handler file | broke "one type per file" | backend | replaced with an anonymous projection and a value tuple | build; integration summary test passed after the change |
| AF-003 | Medium | fixed | Gifts request/response records; `openapi.json` | first build produced schemas `CategoryItem` and `CreatedEntry` shared with Expenses (the OpenAPI generator names schemas by simple type name and merged them) | a later change to either module's record would silently change the other module's contract | backend | records renamed (`GiftCategoryItem`, `GiftDetailItem`, `GiftVariantItem`, `CreatedGiftsEntry`, `GiftCategoryRequest`, `GiftDetailRequest`, `GiftVariantRequest`); new `ContractSchemaNameTests` fails on any cross-module clash (Red shown with a temporary clashing record) | `dotnet test tests/ArchitectureTests` 23 passed; Expenses schemas equal to `HEAD` |
| AF-004 | Low | accepted | `DeleteCategoryHandler.cs` | any `DbUpdateException` other than a concurrency one is answered as `gifts.category_in_use` | a different database failure on this single-row delete would read as "in use" instead of 500 | backend | accepted: a single-row delete without other constraints can only fail on the foreign key; Application must not know Npgsql error codes. Revisit if the table gets more constraints | review |
| AF-005 | Low | accepted | `Gifts.Api/GiftVersionHeader.cs`, `Gifts.Application/HouseholdDay.cs` | the same logic exists in Expenses | two copies to keep in step | backend | rule of three: move to `BuildingBlocks.Api` / `BuildingBlocks.Application` when a third module needs them | review |
| AF-006 | Low | accepted | integration tests | no concurrent test for two racing creates or a delete racing a new gift | the race paths (`UniqueConstraintViolationException`, FK refusal) run only in production | backend | accepted for a two-person app; the guards are database constraints and the catch blocks mirror Expenses, which has the same coverage | review |
| AF-007 | Low | accepted | `CreateDetailHandler.cs`, `CreateCategoryHandler.cs` | "last sort order + 10" read and insert are not atomic | two racing creates can get the same position; order then falls back to the label/name | backend | accepted: harmless tie, no reordering feature yet | review |
| AF-008 | Info | open (handoff) | `gifts.*` tables | columns have no database defaults (`status`, `sort_order`, `created_at_utc`, variant `id` are required) | the import SQL must supply every non-null column | coordinator | noted in the final report and ADR 0026 | import dry run on a local database |

New-feature review list (items the repository rules do not already cover): non-functional numbers → AF-001;
permission per endpoint → ➖ open site (ADR 0015), owner check not applicable (both people own all data); personal
data → details (body sizes), kept until deleted, not logged (ADR 0025); rate limit per endpoint → every write
`PublicWrite`, reads global default; action journal → ➖ none in the project; monthly cost → ➖ none added.

## Verification summary

| Command or test | Result | What it confirms |
| --- | --- | --- |
| `cd api && dotnet build HomeApp.slnx` | success, 0 warnings, 0 errors | code compiles with warnings as errors; `openapi.json` regenerated (14 Gifts operations; Expenses unchanged vs `HEAD`) |
| `dotnet test tests/UnitTests` | 131 passed (32 Gifts) | domain rules and validators |
| `dotnet test tests/ArchitectureTests` | 23 passed | boundaries, layers, encapsulation, folders, use-case rules, OpenAPI rules, schema-name uniqueness |
| `dotnet test tests/IntegrationTests` (Testcontainers) | 36 skipped | Docker is not available in this environment |
| `dotnet test tests/IntegrationTests` against a local PostgreSQL 16 (test host pointed at it for the run, file restored afterwards) | 36 passed (10 Gifts) | migrations, seed, every Gifts scenario end to end, Expenses unaffected by the move |
| `\d gifts.gifts` etc. on the migrated database | matches the brief | fixed names and types for the import |
