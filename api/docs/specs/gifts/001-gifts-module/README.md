# 001 — Gifts: the gift wishlist (backend)

- **Module:** `Gifts` (new; see [ADR 0021](../../../decisions/0021-gifts-module-model.md))
- **Type:** feature
- **Status:** implemented, audited (see [code audit](code-audit/audit.md))
- **Owner:** Vitaliy (product), backend agent (delivery)
- **Created:** 2026-11-26

Masha and Vitaliy keep their gift ideas in the Google Sheet «🎁 Список подарков»: who it is for, what, category,
priority, price, a link, other variants in the notes, an occasion, and a «Куплено» mark that greys the row out. This
feature moves the list to the site as a new `Gifts` module with its own schema `gifts`: gifts with any number of
variants (each with its own link), an active list and an archive («Подаренные») that a gifted or dropped item moves
to, editable gift categories, the personal details («нюансы»: ring size, shoe size …) kept in settings, and one
computed summary for the home page. The web half (`web/src/features/gifts`, the home summary card) is a separate
frontend SDD and is out of this backend scope; this feature delivers the HTTP contract it consumes. The sheet's 91
rows arrive once through a hand-run SQL script ([ADR 0026](../../../decisions/0026-gifts-import-as-sql-script.md)),
which fixes the table and column names below.

## Requirements

- [Overview and scope](requirements/overview.md)
- [Wishlist, archive, categories, details and summary](requirements/wishlist.md)

## Technical design

- [Domain](design/domain.md)
- [Infrastructure](design/infrastructure.md)
- [Data model](data-model.md)
- [HTTP contract](contracts/api-contract.md)

## Planned file map

Paths are relative to `api/`. All entries were planned before the code was written and now exist; the actual tree
and its explanation live on the module page [docs/modules/gifts.md](../../../modules/gifts.md).

| Path | State | One responsibility | Task |
| --- | --- | --- | --- |
| `src/Modules/Gifts/Gifts.Domain/Wishlist/` | planned → existing | `Gift` aggregate, `GiftVariant` child, `VariantDraft`, `GiftStatus`, `Priority`, `GiftErrors` | TS-001, TS-002 |
| `src/Modules/Gifts/Gifts.Domain/Categories/` | planned → existing | `Category` aggregate and its errors | TS-003 |
| `src/Modules/Gifts/Gifts.Domain/People/` | planned → existing | `Recipient`, the `Detail` aggregate and its errors | TS-003 |
| `src/Modules/Gifts/Gifts.Application/IGiftsDbContext.cs` | planned → existing | the module's unit of work port | EN-001 |
| `src/Modules/Gifts/Gifts.Application/HouseholdDay.cs` | planned → existing | "today" in `Europe/Kyiv` | TS-005 |
| `src/Modules/Gifts/Gifts.Application/Idempotency/` | planned → existing | `Idempotency-Key` repeat rules for creates (ADR 0007) | TS-004 |
| `src/Modules/Gifts/Gifts.Application/Features/Items/Commands/*` | planned → existing | `CreateGift`, `UpdateGift`, `DeleteGift`, `ChangeGiftStatus` | TS-004, TS-005 |
| `src/Modules/Gifts/Gifts.Application/Features/Items/Queries/ListGifts/` | planned → existing | active list and archive with variants | TS-006 |
| `src/Modules/Gifts/Gifts.Application/Features/Items/VariantDraftValidator.cs` | planned → existing | shape checks of one variant, shared by create and update | TS-004 |
| `src/Modules/Gifts/Gifts.Application/Features/Categories/*` | planned → existing | `ListCategories`, `CreateCategory`, `RenameCategory`, `DeleteCategory` | TS-007 |
| `src/Modules/Gifts/Gifts.Application/Features/Details/*` | planned → existing | `ListDetails`, `CreateDetail`, `UpdateDetail`, `DeleteDetail` | TS-008 |
| `src/Modules/Gifts/Gifts.Application/Features/Summary/*` | planned → existing | `GetGiftsSummary` and its response records in `Report/` | TS-009 |
| `src/Modules/Gifts/Gifts.Infrastructure/` | planned → existing | `GiftsDbContext` (schema `gifts`), registration, dev initializer | EN-001 |
| `src/Modules/Gifts/Gifts.Infrastructure/Persistence/Wishlist/` | planned → existing | mapping of gifts with owned variants, and of create requests | EN-002 |
| `src/Modules/Gifts/Gifts.Infrastructure/Persistence/Reference/` | planned → existing | mapping of categories (with `CategorySeed`) and details | EN-002 |
| `src/Modules/Gifts/Gifts.Infrastructure/Persistence/Migrations/` | planned → existing | `Initial` migration: schema, tables, seed | EN-003 |
| `src/Modules/Gifts/Gifts.Api/` | planned → existing | `GiftsModule`, `GiftVersionHeader`, endpoints in `Endpoints/Items`, `Endpoints/Status`, `Endpoints/Categories`, `Endpoints/Details` | TS-010 |
| `src/Modules/Gifts/Gifts.Contracts/` | planned → existing | empty: no other module uses Gifts | EN-001 |
| `src/Host/Host.Api/Program.cs`, `Composition/MediatorSetup.cs`, `Host.Api.csproj`, `HomeApp.slnx` | existing, changed | register the module | EN-001 |
| `tests/UnitTests/Gifts/Domain/`, `tests/UnitTests/Gifts/Input/` | planned → existing | domain rules and validators | TS-001…TS-009 |
| `tests/IntegrationTests/Api/Gifts/` | planned → existing | end-to-end API checks on PostgreSQL | TS-011 |
| `tests/ArchitectureTests/Conventions/ContractSchemaNameTests.cs` | added during delivery | HTTP record names unique across modules (OpenAPI schema clash) | TS-012 |
| `tests/IntegrationTests/Api/Expenses/` | existing, moved | the four Expenses API test classes, moved out of `Api/` so that no folder holds more than six code files | TS-011 |
| `.github/workflows/api.yml` | existing, changed | the `gifts` migration bundle | EN-003 |

No existing file is overloaded by this feature: the host files gain one line each, `IntegrationTests/Api/` is split by
module (the seventh file would break the folder rule).

## Planning and delivery

- [Scenario traceability](traceability.md)
- [Task graph](tasks/README.md)
- [Specification readiness](checklist/spec-readiness.md)
- [Delivery readiness](checklist/delivery-readiness.md)
- [Code audit](code-audit/audit.md)

`TS-*` tasks that implement behaviour follow `Red → Green → Refactor → Regression → evidence`. `Depends on` in the task
graph sets the order.

## Change history

- 2026-11-26 — created from the product brief (fixed names of tables, columns and routes); specification and
  implementation assigned together, so the open product points were decided and recorded as assumptions in
  [overview](requirements/overview.md#assumptions-decided-without-asking).
- 2026-11-26 — implemented; final audit recorded; ADRs 0021–0026 added.
