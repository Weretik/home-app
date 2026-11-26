# 001 — Shopping list

- **Module:** Shopping (new)
- **Type:** feature (new module)
- **Status:** implemented, verified (unit + architecture tests; integration tests compiled, run in CI)
- **Owner:** Vitaliy
- **Created:** 2026-11-26

Moves the shared shopping list of the Google Sheet «🛒 Список покупок» and its Telegram bot to the site, and makes it
better: quick add from free text with automatic departments, a cart that empties itself, a history, frequent items,
editable departments and keywords, and a summary for the home page. Source of scope and contract:
`/mnt/project-files/home-app/specs/shopping-module.md` (product brief of 2026-11-26).

## Requirements

- [Overview and scope](requirements/overview.md)
- [Behaviour, rules and scenarios](requirements/behaviour.md)

## Technical design

- [Domain](design/domain.md)
- [Infrastructure](design/infrastructure.md)
- [Data model](data-model.md)
- [HTTP contract](contracts/api-contract.md)

## File map

Paths under `api/`. All planned at the start of the feature; all created by it.

| Path | State | One responsibility | Task |
| --- | --- | --- | --- |
| `src/Modules/Shopping/Shopping.Domain/Items/` | created | `ShoppingItem`, `ShoppingItemStatus`, `Person`, `CartWindow` (list/cart/history rule), `ShoppingItemErrors` | TS-002, TS-003 |
| `src/Modules/Shopping/Shopping.Domain/Departments/` | created | `Department`, `DepartmentKeyword`, `DepartmentErrors` | TS-004 |
| `src/Modules/Shopping/Shopping.Domain/Settings/` | created | `ShoppingSettings` singleton, its errors | TS-005 |
| `src/Modules/Shopping/Shopping.Domain/Parsing/` | created | `NameNormalizer`, `ItemTextParser`, `ParsedItem` (port of `parseItems_`, `norm_`) | TS-001 |
| `src/Modules/Shopping/Shopping.Domain/Matching/` | created | `DepartmentMatcher`, `KeywordRule` (port of `deptOf_`) | TS-001 |
| `src/Modules/Shopping/Shopping.Application/IShoppingDbContext.cs` | created | unit of work port | EN-002 |
| `src/Modules/Shopping/Shopping.Application/Idempotency/` | created | `CreateRequest`, `IdempotentCreates`, `ShoppingCreatedId` (ADR 0007, copied from Expenses) | EN-002 |
| `src/Modules/Shopping/Shopping.Application/Features/Items/Commands/*` | created | `AddItems`, `UpdateItem`, `SetItemStatus`, `DeleteItem`, `ClearCart` | TS-006..TS-009 |
| `src/Modules/Shopping/Shopping.Application/Features/Items/Queries/*` | created | `ListItems`, `ListHistory`, `ListFrequent`, `GetSummary` | TS-010, TS-011 |
| `src/Modules/Shopping/Shopping.Application/Features/Items/Views/` | created | `ShoppingItemView`, projection, frequent-name statistics | TS-010 |
| `src/Modules/Shopping/Shopping.Application/Features/Dictionary/` | created | loads the matcher from the tables; learns a keyword | TS-006, TS-007 |
| `src/Modules/Shopping/Shopping.Application/Features/Departments/*` | created | department CRUD, reorder, list, match | TS-012 |
| `src/Modules/Shopping/Shopping.Application/Features/Keywords/*` | created | keyword CRUD and list | TS-013 |
| `src/Modules/Shopping/Shopping.Application/Features/Settings/*` | created | get / update settings | TS-014 |
| `src/Modules/Shopping/Shopping.Infrastructure/` | created | `ShoppingDbContext` (schema `shopping`), configurations, seed, migration `Initial`, initializer | EN-003 |
| `src/Modules/Shopping/Shopping.Api/` | created | `ShoppingModule`, `VersionHeader`, `Endpoints/Items/{Reading,Editing}`, `Endpoints/Departments`, `Endpoints/Keywords`, `Endpoints/Settings` | TS-015 |
| `src/Modules/Shopping/Shopping.Contracts/` | created | empty: no other module uses Shopping yet | EN-001 |
| `src/Host/Host.Api/{Program.cs,Host.Api.csproj,Composition/MediatorSetup.cs}` | existing | register the module and its Application assembly | EN-001 |
| `HomeApp.slnx` | existing | solution folder `/src/Modules/Shopping/` | EN-001 |
| `tests/UnitTests/Shopping/{Text,Domain,Input}/` | created | parser, normaliser, matcher, item/department/settings rules, validators | TS-001..TS-005 |
| `tests/IntegrationTests/Api/Shopping/` | created | end-to-end API tests; `Api/` split into `Expenses/`, `Platform/`, `Shopping/` (folder rule) | TS-016 |
| `docs/modules/shopping.md`, `docs/decisions/0031..0032`, `docs/architecture/c4.md` | created | module page, ADRs, C4 | TS-017 |

## Planning and delivery

- [Traceability](traceability.md)
- [Task graph](tasks/README.md)
- [Specification readiness](checklist/spec-readiness.md)
- [Delivery readiness](checklist/delivery-readiness.md)
- [Code audit](code-audit/audit.md)

Every behaviour task `TS-*` follows Red → Green → Refactor → Regression → evidence; `EN-*` tasks name their
substitute check.

## History

- 2026-11-26 — created from the product brief; implemented in the same change.
