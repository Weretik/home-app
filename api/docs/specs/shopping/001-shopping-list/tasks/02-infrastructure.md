# 02 — Infrastructure and enablers

## EN-002 — Unit of work and idempotency

- `Shopping.Application/IShoppingDbContext.cs`, `Shopping.Application/Idempotency/{CreateRequest,IdempotentCreates,ShoppingCreatedId}.cs`.
- Copy of the Expenses approach (ADR 0007) with one change: the stored `outcome` is JSON, so an add of many items can
  replay its grouped ids. Test-first exception: no behaviour without a database; substitute check is the
  integration test `A_Repeated_Add_ReturnsTheSameItems_AndOtherData_IsAConflict`.

## EN-003 — Persistence, seed and migration

- `Shopping.Infrastructure/{ShoppingDbContext,ShoppingInfrastructureRegistration,ShoppingDatabaseInitializer}.cs`,
  `Persistence/Items/*Configuration.cs`, `Persistence/Reference/{DepartmentConfiguration,DepartmentKeywordConfiguration,ShoppingSettingsConfiguration,DictionarySeed,SeedIds}.cs`,
  `Persistence/Migrations/<ts>_Initial.cs`.
- Seed read from «Справочник!A1:D600» of the sheet 1WGoaY6LzZB_J5BRImLXAXHqg3DHErDNykHaDpmPZC9Y on 2026-11-26.
- Migration: `dotnet ef migrations add Initial --project src/Modules/Shopping/Shopping.Infrastructure --startup-project src/Host/Host.Api --context ShoppingDbContext --output-dir Persistence/Migrations`.
- Checks: `dotnet ef migrations has-pending-model-changes --context ShoppingDbContext` reports none; the seed
  unit test (distinct normalised keywords, 13 departments, one fallback) passes; integration test
  `The_Dictionary_ComesFromTheMigration` in CI.
