# 04 — API and tests

## TS-015 — Endpoints

- `Shopping.Api/ShoppingModule.cs` (`AddShoppingModule`, `MapShoppingModule`, group `/api/shopping`, tag
  `Shopping`), `Shopping.Api/VersionHeader.cs`, endpoints in `Endpoints/Items/Reading`, `Endpoints/Items/Editing`,
  `Endpoints/Departments`, `Endpoints/Keywords`, `Endpoints/Settings`; request records nested in their endpoint
  (schema `<Endpoint>Request`).
- Every endpoint: `WithName` exactly as the contract table, `WithSummary`, `Produces<T>` / `ProducesProblem`; writes
  `RequireRateLimiting(PublicWrite)` + 429.
- Evidence: `dotnet build HomeApp.slnx` regenerates `docs/contracts/openapi.json` with 21 `/api/shopping` operations;
  `OpenApiDocumentTests` green.

## TS-016 — Integration and architecture tests

- `tests/IntegrationTests/Api/Shopping/{ShoppingItemsApiTests,ShoppingDictionaryApiTests,ShoppingSettingsApiTests}.cs`;
  the `Api/` folder is split into `Expenses/`, `Platform/`, `Shopping/` to keep ≤6 files per folder.
- Tests use unique item names (a GUID suffix) because the list is global and the database is shared; tests that
  change settings or departments restore them or work on their own departments.
- Architecture tests enumerate modules by folder, so Shopping is covered without code changes; the test project
  that loads assemblies references the host, which references `Shopping.Api`.
- Evidence: `dotnet build tests/IntegrationTests` green locally; `dotnet test tests/ArchitectureTests` green; the
  integration run happens in CI (Docker).
