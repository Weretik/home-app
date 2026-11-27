# Code audit — Notes module

- **Feature:** [notes/001-notes-module](README.md)
- **Scope:** the new module `src/Modules/Notes` (five projects, `Initial` migration, eight endpoints); `ICurrentMember`,
  `SignedInMember`, `HouseholdPerson` in `Identity.Contracts` and `CurrentMember` in `Identity.Api`; the host
  registration (`Program.cs`, `MediatorSetup`, `Host.Api.csproj`); `api.yml` (bundle, smoke reads); the new unit,
  architecture and integration tests; the docs this change touched (module page, ADRs 0039 and 0040, C4, rules, deploy
  guide, README, AGENTS.md).
- **Stage:** final audit after implementation (skill `backend-code-audit`, obsidian `Architecture/Checklists`).
- **Date:** 2026-11-27
- **Verdict:** **pass with open findings.** No blocker. One Medium finding (an OpenAPI schema shared by accident) is
  fixed; the rest are Low and fixed or accepted with a reason.

## Step 0: quality targets

[quality-targets.md](../../../architecture/quality-targets.md) exists; every value is still *proposed, to confirm*
and *not measured* (recorded there, not repeated here). The module is not on a hot path: hundreds of notes, about ten
categories, two users. The file had no line for Notes; one was added (AF-004). No speed verdict beyond that.

## Boundaries and layers

- [x] `Notes.Application` references its Domain, its Contracts, `BuildingBlocks.Application` and only
  `Identity.Contracts` from another module (ADR 0040). No other module references Notes. `ModuleBoundaryTests`,
  `LayerDependencyTests` pass; the new `ModuleBoundaryTests.Only_TheIdentityModule_ReadsClaims` keeps
  `System.Security.Claims` and `.User` out of every module but Identity.
- [x] `HouseholdPerson` is mapped to the module's own enum in `Features/Notes/NoteAuthors` (Application); Domain knows
  nothing of Identity.
- [x] Domain (`Note`, `NoteCategory`, `NoteKind`, `NotePerson`, errors) has no EF Core or ASP.NET Core; both roots keep
  their rules (trim, lengths, empty text as none, author kept on edit) behind `Create` / `Update` / `Rename` returning
  `Result`, with private setters; unit tested. Ids are created as in the reference module Todos (`Guid.CreateVersion7`).
- [x] Schema `notes` with its own history table; the only foreign key (`notes.category_id → categories.id`, `ON DELETE
  SET NULL`) stays inside the schema. `DataOwnershipTests` and the migration check pass; `NotesSchemaTests` checks the
  unique `lower(name)` index and the list indexes.
- [x] Use cases `Features/Notes|Categories/Commands|Queries/<UseCase>/` with command or query, handler, validator;
  endpoints map a request record to the command and the result through `ToHttpResult` / `ToCreatedHttpResult`. Command
  handlers load, check what needs outside data (the category exists), call the aggregate and save once.
- [x] `CurrentMember` is scoped and reads the request through `IHttpContextAccessor`; no singleton holds it
  (`ValidateScopes` passes in the test host). It answers `null` for the scheduler key and outside a request.
- [x] Host only composes (`AddNotesModule`, `MapNotesModule`, the Application assembly for Mediator).
- [x] Folder axiom: no folder over six code files, no empty, wrapper or one-file folder (`FolderRulesTests` pass;
  `Queries/ListNotes` holds exactly six).

## Contract

| Check | Evidence |
| --- | --- |
| Every endpoint has a unique camelCase `WithName`, a `WithSummary`, `Produces<T>` for each body, `ProducesProblem` for 401, 404, 409, 429 and `ProducesValidationProblem` | `Notes.Api/Endpoints/*`; `EndpointRulesTests`, `OpenApiDocumentTests` |
| Writes use `PublicWrite` and list `429` | `NotesCategoriesApiTests.Every_Write_OfTheModule_IsRateLimited`, `OpenApiDocumentTests.Every_Write_IsRateLimited_AndLists429` |
| `createNoteCategory` without `Idempotency-Key` is listed with a reason | `OpenApiDocumentTests` allow-list (ADR 0039) |
| The author is not a request field | `NoteRequest` has no `createdBy`; `NotesItemsApiTests.The_Author_ComesFromTheSession_NotFromTheRequest` |
| Schema names do not merge with another module's | fixed in AF-001: `NotePerson`; `ContractSchemaNameTests` now covers enums |
| Error texts in Ukrainian | `NoteErrors`, `CategoryErrors`, `IdempotentCreates.KeyReused`, the validators' `If-Match` messages; FluentValidation runs with culture `uk` (`MediatorSetup`); `NotesItemsApiTests.Bad_Input_IsRefused_WithUkrainianProblems` |

## Data and idempotency

| Write | Commits | Repeat or race |
| --- | --- | --- |
| `createNote` | one `SaveChanges` (note + `create_requests` row) | `Idempotency-Key` with a fingerprint under the primary key on `key`: a repeat returns the first id, other data with the same key `409`, a racing copy's unique violation answers like a repeat (ADR 0007). Tested. |
| `updateNote` | one `SaveChanges`, guarded by `xmin` | a stale `If-Match` answers `409` before the save, a concurrent save `409` from `DbUpdateConcurrencyException`. Tested. |
| `deleteNote` | one conditional `DELETE … WHERE id AND xmin` | a stale version deletes nothing and answers `409`; a repeat answers `404`. Tested. |
| `createNoteCategory` | one `SaveChanges` | no key on purpose: a repeat hits the unique `lower(name)` index and answers `409`; the race is caught as `UniqueConstraintViolationException`. Tested. |
| `updateNoteCategory` | one `SaveChanges`, guarded by `xmin` | `409` for a stale version or a taken name, both checked and caught at the save. Tested. |
| `deleteNoteCategory` | one conditional `DELETE`; the database clears `category_id` of its notes | stale `409`, repeat `404`; notes stay without a category (SC-005). Tested. |

No side effect leaves the transaction, so no outbox. Reads are `AsNoTracking` projections; the list is one query
(left join, filters and order in SQL, `Take(1000)`), the category list one query with a correlated count; no N+1.

## Findings

| ID | Severity | Finding | Evidence | Status |
| --- | --- | --- | --- | --- |
| AF-001 | Medium | The Notes enum `Person` gave the OpenAPI schema `Person`, which Expenses, Shopping and Todos already produce: four types silently became one schema with Expenses' description, and a later change to any one of them would change the others' contract. `ContractSchemaNameTests` checked only records. | `Notes.Domain/Notes/Person.cs`, `openapi.json` `#/components/schemas/Person` | fixed: renamed to `NotePerson` (schema `NotePerson`, same values); `ContractSchemaNameTests` gained `Enums_HaveNames_NoOtherModuleUses` and `Shared_EnumNames_KeepTheSameMembers_InEveryModule`; a probe enum `Person` in Notes makes the test fail. Docs: design, module page, rules. The web client must be regenerated (web half). |
| AF-002 | Low | Expenses, Shopping and Todos still share the schema `Person` (identical members `Vitaliy`, `Maria`). | `Expenses.Domain/Parties/Person.cs`, `Shopping.Domain/Items/Person.cs`, `Todos.Domain/Parties/Person.cs` | accepted: outside this feature; renaming changes three modules' contract and the web client. The new test allows exactly these three modules and fails if their members ever differ or a fourth module joins. |
| AF-003 | Low | A category deleted between the "category exists" check and the save of `createNote` / `updateNote` makes the foreign key fail; `ModuleDbContext` maps only unique violations, so the answer is `500`, not `404`. | `CreateNoteHandler`, `UpdateNoteHandler`, `BuildingBlocks.Infrastructure/Persistence/ModuleDbContext.cs` | accepted: a window of milliseconds between two people; a retry answers `404`; Todos has the same shape. A fix belongs to BuildingBlocks (map foreign-key violations), not to this module. |
| AF-004 | Low | `quality-targets.md` had no line on the Notes load and volume. | `architecture/quality-targets.md` | fixed: a Notes note (volumes, one query each, measure before an index or paging). |
| AF-005 | Low | The design and ADR 0039 said the search is `ILIKE`; the code runs `LIKE` on lower-cased columns with lower-cased patterns (same result). | `ListNotesHandler`, `SearchPattern` | fixed: design and ADR 0039 now describe the code. |
| AF-006 | Low | Personal data: preferences and facts about two named people, kept without a retention period. | `notes.notes` | accepted: the purpose of the module (ADR 0039); visible only to the two members (closed by default, `NotesAccessApiTests`); a note is deleted for good on request. |
| AF-007 | Low | No action journal beyond `created_by`, `updated_by`, `updated_at_utc`; a deleted note leaves no trace. | `Note` | accepted: two users editing shared notes; who wrote and who changed last is what the list shows (R-008). |

## Architecture records

- [x] ADR 0039 (module and model) and ADR 0040 (current member through `Identity.Contracts`) accepted, with sources;
  no accepted ADR contradicts the code (ADR 0039's search wording fixed, AF-005).
- [x] C4: the Notes module in the component diagram with its `Contracts` arrow to Identity.
- [x] Module page `modules/notes.md`, `modules/identity.md` (`ICurrentMember`), architecture README, rules (search
  exception, schema names), deploy guide (bundle, smoke reads), API README, AGENTS.md, README.
- [x] Deploy: `api.yml` builds a `notes` migrations bundle (run by the `migrations/*` loop before the app deploy) and
  the smoke step reads `GET /api/notes/items` and `/api/notes/categories` with the scheduler key. The `Initial`
  migration only adds a schema, so the previous release keeps working.

## Traceability

| Scenario or rule | Test |
| --- | --- |
| SC-001, R-008 | `NotesItemsApiTests.Vitaliy_WritesAboutMasha_AndTheListSaysWhoWroteIt`, `The_Author_ComesFromTheSession_NotFromTheRequest`; `NoteAuthorsTests`; `CurrentMemberTests` |
| SC-002, R-013 | `NotesItemsApiTests` (Masha's note), `NotesListApiTests.Filters_Combine_Person_Kinds_AndCategory` |
| SC-003 | `NotesItemsApiTests.Masha_Edits_VitaliysNote_AndBecomesTheLastEditor`; `NoteTests.Update_…` |
| SC-004, R-009 | `NotesItemsApiTests.A_Stale_Version_AnswersConflict_ForAnEdit_AndADelete` |
| SC-005, R-006, R-011 | `NotesCategoriesApiTests.A_Category_IsCreated_Renamed_AndDeleted_ItsNotesStay_WithoutACategory`; `NoteCategoryTests` |
| SC-006 | `NotesListApiTests.Search_FindsEveryWord_InTheTitle_TheDescription_OrThePlace` |
| SC-007, R-014 | `NotesAccessApiTests`; `IdentityProtectionApiTests` sweeps |
| R-003…R-005, R-007 | `NoteTests`, `NoteValidatorTests`, `NotesItemsApiTests.Bad_Input_IsRefused_WithUkrainianProblems` |
| R-010 | `NotesItemsApiTests.A_Repeat_ReturnsTheFirstNote_AndTheSameKey_WithOtherData_IsAConflict` |
| R-012 | `NotesListApiTests.The_List_IsOrdered_ByCategory_ThenImportant_ThenKind_ThenTitle` |

## Verification

- `dotnet build HomeApp.slnx`: success, 0 warnings; `openapi.json` regenerated (schema `Person` → `NotePerson` for the
  Notes operations; `Person` stays for Expenses, Shopping, Todos).
- Unit tests: 429 passed.
- Architecture tests: 93 passed (2 new in `ContractSchemaNameTests`).
- Integration tests, filter `Notes|Identity` (Testcontainers, PostgreSQL 17): 93 passed, 0 skipped.
