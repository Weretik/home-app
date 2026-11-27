# Backend rules

Short, stable rules for every change. The ones marked **(tested)** are enforced by `tests/ArchitectureTests` (or
`tests/IntegrationTests` where a database is needed) and fail CI when broken; the [enforcement table](#enforcement)
names the test for each rule and the reason for each rule that is review only
([ADR 0029](decisions/0029-every-rule-checked-by-a-test.md)).

## Structure

- A module is five projects: `Api`, `Application`, `Contracts`, `Domain`, `Infrastructure`. **(tested)**
- A module uses another module only through that module's `Contracts`. **(tested)**
- Only the Identity module reads claims or `HttpContext.User`; other modules learn who is signed in through
  `Identity.Contracts.ICurrentMember` ([ADR 0040](decisions/0040-current-member-through-identity-contracts.md)). **(tested)**
- `Domain` references no EF Core and no ASP.NET Core; `Application` references no `Infrastructure` or `Api`.
  **(tested)**
- `Host.Api` holds composition only and references only module `Api` projects and shared infrastructure. **(tested)**
- Domain properties have no public setters; state changes through methods that check the rules. **(tested)**
- Folders follow features and aggregates, never kinds of classes: `Features/Products/Commands/CreateProduct/`, not
  `Handlers/` or `Services/`. Names such as `Shared`, `Common`, `Helpers`, `Utils`, `Services`, `Entities` are not
  allowed as folders. **(tested)**
- **A folder holds at most six code files.** When a seventh arrives, split the folder by meaning into named
  subfolders of at least two files each. No empty folders, no folder that only wraps one subfolder, no new one-file
  folders. Only generated EF `Migrations/` are exempt. This applies to `src/` and `tests/`. **(tested)**
- One type per file; the file has the type's name, and its file-scoped namespace follows its folders. **(tested)**
- All projects live under `src/` or `tests/`, and solution folders mirror the disk. **(tested)**

## Use cases

- One use case is one folder under `Commands/` or `Queries/` with its command or query, its validator and its
  handler, named after the folder (`CreateExpense/`: `CreateExpenseCommand`, `CreateExpenseHandler`,
  `CreateExpenseValidator`). Every use case returns a `Result`, and every use case with input has a validator.
  **(tested)**
- Commands change state and return little; queries never change state. **(tested: queries)**
- The HTTP request record belongs to the endpoint and is mapped to the command; the command is not the request.
  **(tested)**
- A record that travels over HTTP (request or response) has a name no other module uses: the OpenAPI document names a
  schema by the type's simple name, so two modules' `CategoryItem` would silently become one schema. The same holds
  for a public enum of a module (Notes uses `NotePerson`); only `Person` stays shared by Expenses, Shopping and Todos,
  with the same members in each. **(tested)**
- Every input has a FluentValidation validator. Validators check shape (required, length, range); the domain checks
  business rules.
- A command saves once (`SaveChangesAsync` a single time). There are no repositories that save on their own; the
  module's `DbContext` port is the unit of work. **(tested)**
- Arithmetic that a handler does over what it read (sums, carry-over, forecasts) sits in a pure type beside the
  handler, so it can be unit-tested with fixed input and a fixed day (`GetMonthSummary/SummaryFigures`,
  `GetGiftsSummary/SummaryFigures`, Shopping's `GetSummary/SummaryFigures`).

## Errors

- A broken business rule returns `Result` / `Result<T>` with a typed `Error` (`Validation`, `NotFound`, `Conflict`,
  `Forbidden`, `Unauthorized`) and a stable code such as `catalog.slug_taken`.
- Exceptions are for bugs and outages; `Domain` and `Application` never `throw` for a business rule. **(tested)** One
  global handler logs them and answers 500 without internals.
- A unique index violation becomes a `Conflict` result in the handler, so a race never returns 500. **(tested on
  the races that exist: `ExpensesRaceApiTests`)**

## API

- Minimal APIs, one endpoint per file (`<Name>Endpoint.cs`), grouped by feature. **(tested)**
- Every endpoint has a unique camelCase `WithName`, a `WithSummary`, and `Produces...` for every response it can
  give. **(tested on the generated OpenAPI document)**
- Errors are RFC 9457 problem responses with `code` and, for validation, `errors` by field. **(tested)**
- Every endpoint needs a signed-in member through the Identity module's fallback policy; nothing adds its own
  `RequireAuthorization`. Only the endpoints listed with a reason in `AuthorizationRulesTests` say `AllowAnonymous`
  ([ADR 0037](decisions/0037-google-sign-in-with-server-sessions.md)). **(tested)**
- Every endpoint that writes, and the sign-in, opts into a rate limit policy
  (`RequireRateLimiting(RateLimitPolicies.PublicWrite)`, `SignIn` for the sign-in) and lists `429` in its
  `Produces...`. **(tested)**
- A create (`201`) takes an `Idempotency-Key` header and an edit (`PUT`) takes `If-Match` with the row version; an
  exception is named in the test with its ADR (`recordPlannedExpense`, [ADR 0028](decisions/0028-planned-expenses-for-the-forecast.md)).
  **(tested on the OpenAPI document)**
- The OpenAPI document is generated on build and committed; CI fails when it differs from the code. Never edit it by
  hand. **(tested in CI: `git diff --exit-code`)**

## Data

- One schema per module and no foreign keys across schemas; a module's `DbContext` maps only the module's types.
  **(tested)**
- The committed migrations match the model (no change without a migration). **(tested)**
- Reads use projections or `AsNoTracking` and page in SQL (page size at most 100). **(tested)**
- No query and no `SaveChanges` inside a loop.
- Search splits the input into words; each word is an escaped `LIKE` on a lower-cased,
  `pg_trgm`-indexed column (exceptions: Shopping keywords, about 400 rows, ADR 0031; Notes, hundreds of rows,
  ADR 0039; the production server does not allow `CREATE EXTENSION`).
- Shared state gets a named race guard: unique index, row version, or conditional `UPDATE`. The guard is tested by
  letting a second request in between the read and the save (`TestHost/RacingExpensesDb`).
- Migrations are a separate step before a deploy and must keep the previous release working (add first, remove
  later).

## Repeats and side effects

- A create that moves money or stock, numbers a document, or sends something outside takes an `Idempotency-Key`
  stored in the same transaction under a unique index. A repeat returns the first result; the same key with other
  data answers 409.
- Side effects outside the transaction (notifications, calls to other systems) go through the outbox, and their
  handlers are safe to run twice.
- Work that runs on a schedule is an HTTP endpoint called by an external scheduler, not a background thread.
  **(tested)**
- A client of an outside system (Telegram) is tested as it runs in production: the real client and its `HttpClient`
  registration, with only the network stubbed and the request urls checked. Secrets in tests have the real format (a
  Telegram token has a colon) ([repository ADR 0005](../../docs/decisions/0005-post-deploy-smoke-reads-every-section.md)).
  **(tested: `NotificationsTelegramHttpTests`)**

## Host and configuration

- The host only composes modules: it calls `Add<Module>Module()` and `Map<Module>Module()` for each module. **(tested)**
- The host validates scopes and the container build in every environment, so a singleton that captures a scoped
  service fails at startup. **(tested)**
- Committed settings outside `appsettings.Development.json` hold no secrets (connection strings, keys) and do not
  migrate on startup. **(tested)**

## Process

- Write the test first: domain rules, validators and handler arithmetic in `UnitTests`; handler orchestration over the
  database (repeats, versions, races, not found) in `IntegrationTests` on real PostgreSQL, since an in-memory
  database cannot check `xmin`, unique indexes or `ExecuteDelete`.
- Add an abstraction only when a third case needs it.
- A lasting decision (a module, an external system, a way of running work, hosting) gets an ADR and a C4 update in
  the same change; every ADR has a status and is in the index. **(tested: status and index)**
- A feature starts with its SDD in `docs/specs/` and ends with an audit; every SDD is in the specs index.
  **(tested: index)**
- Package versions only in `Directory.Packages.props`. **(tested)**

## Enforcement

One row per rule. Tests are in `tests/ArchitectureTests` unless the path says otherwise; CI runs all of them on every
push. "Review only" rules are checked by the audit of each feature.

| Rule | Checked by |
| --- | --- |
| Five projects per module | `Boundaries/ModuleBoundaryTests.Every_Module_HasAllFiveLayers` |
| Other modules only through `Contracts`; `Contracts` reference nothing | `ModuleBoundaryTests.A_Module_ReferencesOtherModules_OnlyThroughTheirContracts`, `.Contracts_AreLeaf_Projects…` |
| Only Identity reads claims or `HttpContext.User` | `ModuleBoundaryTests.Only_TheIdentityModule_ReadsClaims` |
| `Domain` without EF Core, ASP.NET Core, Mediator, FluentValidation | `Boundaries/LayerDependencyTests.Domain_DependsOnNoFrameworks` |
| `Application` without the driver, the web, `Infrastructure`, `Api` | `LayerDependencyTests.Application_DoesNotKnowTheDatabaseDriverOrTheWeb` |
| `Infrastructure` without `Api` | `LayerDependencyTests.Infrastructure_DoesNotDependOnApi` |
| Host references only module `Api` projects and shared infrastructure | `LayerDependencyTests.The_Host_OnlyComposes…` |
| Host calls `Add<Module>Module` / `Map<Module>Module` for every module | `Hosting/HostRulesTests.The_Host_AddsAndMapsEveryModule…` |
| Host validates scopes and the build in every environment | `HostRulesTests.The_Host_ValidatesScopesAndTheBuild_InEveryEnvironment`; every integration test boots the host with it |
| No background threads; scheduled work is an HTTP endpoint | `HostRulesTests.Nothing_RunsInTheBackground` (allow-list: `DatabaseStartup`, a development-only startup step) |
| No public setters in `Domain` | `Boundaries/DomainEncapsulationTests.Domain_Properties_HaveNoPublicSetters` |
| One schema per module, only own types, no FK across schemas | `Boundaries/DataOwnershipTests.A_Module_MapsOnlyItsOwnTypes_InItsOwnSchema` |
| Migrations match the model | `DataOwnershipTests.The_Migrations_MatchTheModel` |
| At most six code files per folder | `Structure/FolderRulesTests.A_Folder_HoldsAtMostSixCodeFiles` (`src`, `tests`) |
| No one-file, empty or wrapper folders | `FolderRulesTests.No_Folder_HoldsASingleCodeFile`, `.No_Folder_IsEmpty`, `.No_Folder_OnlyWrapsOneSubfolder` |
| Folders named by meaning (no `Helpers`, `Services`, `Entities` …) | `FolderRulesTests.Folders_AreNamedByMeaning_NotByKindOfClass` |
| One type per file, named like the file; namespace follows the folders | `Structure/SourceFileRulesTests` |
| Projects under `src/`/`tests/`, in the solution, solution folders mirror the disk | `Structure/SolutionLayoutTests` |
| Package versions only in `Directory.Packages.props` | `SolutionLayoutTests.Package_Versions_LiveOnlyInDirectoryPackagesProps` |
| Use case folder, names, handler and validator beside it | `UseCases/UseCaseRulesTests.Every_UseCase_SitsInItsOwnFolder…`, `.Every_UseCase_IsNamedAfterItsFolder…` |
| Every use case returns a `Result`; every use case with input has a validator | `UseCaseRulesTests.Every_UseCase_ReturnsAResult`, `.Every_UseCase_WithInput_HasAValidator` |
| Queries never write; reads without tracking or through a projection | `UseCases/HandlerRulesTests.A_Query_NeverChangesState`, `.A_Query_ReadsWithoutTracking_OrThroughAProjection` |
| A command saves once | `HandlerRulesTests.A_Command_SavesOnce` (allow-list: `DeleteDepartmentHandler`, set-based moves of the history rows and the delete in one explicit transaction, ADR 0031) |
| Page size at most 100 | `HandlerRulesTests.A_PagedList_ServesAtMost100RowsAPage` |
| No `throw` for business rules in `Domain`/`Application` | `HandlerRulesTests.Business_Rules_ReturnResults_InsteadOfThrowing` |
| Validators check shape, the domain checks rules | `UnitTests/<Module>/Input/*`, `UnitTests/<Module>/Domain/*` (per rule); the split itself is review only (meaning) |
| Unique `WithName` (camelCase) and `WithSummary` | `Http/OpenApiDocumentTests.Every_Operation_HasAUniqueCamelCaseId_AndASummary` |
| `Produces…` for every body; errors are problem responses; input lists `400` | `OpenApiDocumentTests.Every_SuccessWithABody_HasASchema`, `.Every_Error_IsAProblemResponse`, `.Every_Operation_WithInput_ListsTheValidationProblem` |
| Closed by default, anonymous only from the list | `Http/AuthorizationRulesTests` (architecture), `Api/Identity/IdentityProtectionApiTests` (integration: every documented `GET` is `401` without a session) |
| Writes rate limited and list `429` | `Http/EndpointRulesTests.Every_Write_OptsIntoARateLimit` (allow-list: the keyed outbox tick, ADR 0006; the keyed notifications tick, ADR 0034; the Telegram webhook, ADR 0033), `OpenApiDocumentTests.Every_Write_IsRateLimited_AndLists429` |
| `Idempotency-Key` on creates, `If-Match` on edits | `OpenApiDocumentTests.Every_Create_TakesAnIdempotencyKey_AndEvery_Edit_TheVersion` (allow-lists: `recordPlannedExpense` without a key, ADR 0028; `renameGiftCategory` without a version, a rename is a target value, ADR 0024; `setShoppingItemStatus` and `reorderShoppingDepartments` without a version, target values, ADR 0031) |
| Request record ≠ command; one endpoint per file | `OpenApiDocumentTests.Request_Bodies_AreEndpointRequestRecords_NotCommands`, `EndpointRulesTests.An_Endpoint_NeverBindsACommandOrAQuery_FromHttp`, `.A_File_MapsOneEndpoint_AndIsNamedAfterIt` |
| HTTP records and enums have names no other module uses | `Http/ContractSchemaNameTests.Http_Records_HaveNames_NoOtherModuleUses`, `Enums_HaveNames_NoOtherModuleUses`, `Shared_EnumNames_KeepTheSameMembers_InEveryModule` |
| Numbers are JSON numbers | `OpenApiDocumentTests.Numbers_AreNotAlsoStrings` |
| OpenAPI committed and current | CI `api.yml`: `git diff --exit-code -- docs/contracts/openapi.json` after the build |
| No secrets in committed settings; production does not migrate on startup | `Hosting/ConfigurationRulesTests` |
| Repeats: same key returns the first result, other data `409` | `IntegrationTests/Api/Expenses/*`, `IntegrationTests/Api/Gifts/GiftsApiTests`, `IntegrationTests/Api/Shopping/ShoppingItemsApiTests` (`A_Repeated_*`, per kind) |
| Race guards (row version, unique index, foreign key) answer `409`, never `500` | `IntegrationTests/Api/Expenses/Entries/ExpensesRaceApiTests`, `IntegrationTests/Api/Gifts/GiftsRaceApiTests`, `IntegrationTests/Api/Shopping/ShoppingRaceApiTests` |
| ADRs have a status and are indexed; SDDs indexed; every module has a page | `Structure/DocumentationIndexTests` |
| No query and no `SaveChanges` in a loop | review only: a loop over an in-memory list is fine, over a query is not; the text cannot tell them apart reliably |
| Search: words, escaped `LIKE`, trigram index | `UnitTests/BuildingBlocks/SearchPatternTests` for the pattern; the index is review only (migration content) |
| Migrations keep the previous release working (add first, remove later) | review only: needs judgement on each migration; the deploy runs them as a bundle before the code (ADR 0016) |
| Outside clients tested for real, secrets in real format | `IntegrationTests/Api/Notifications/NotificationsTelegramHttpTests` (real `TelegramBotClient`, stubbed network, BotFather-format token); post-deploy smoke reads every section (`.github/workflows/api.yml`) |
| Side effects through the outbox; handlers safe twice | integration tests run each handler through `POST /internal/outbox/tick` (gift ↔ to-do flow, Telegram message); handlers are upserts or status checks (gifts/002 R-007) |
| A unique violation becomes `Conflict` in the handler | integration tests of each existing race; a new unique index needs its race test (review) |
| Commands return little; abstraction only for a third case; test first | review only: judgement and process |
| ADR and C4 in the same change as a lasting decision | review only: whether a change is a lasting decision is judgement; the index and status are tested |
