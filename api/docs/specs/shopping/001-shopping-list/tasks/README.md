# Task graph

IDs: `R-*` and `SC-*` in [requirements](../requirements/behaviour.md); `TS-*` one implementation responsibility;
`EN-*` a shared prerequisite. Behaviour tasks follow Red → Green → Refactor → Regression → evidence.

| ID | Responsibility | Depends on | File |
| --- | --- | --- | --- |
| EN-001 | Module skeleton: five projects, solution folder, host registration, Mediator assembly | — | [00-readiness](00-readiness.md) |
| TS-001 | `NameNormalizer`, `ItemTextParser`, `DepartmentMatcher` | EN-001 | [01-domain](01-domain.md) |
| TS-002 | `ShoppingItem` create / update / status / restore rules | EN-001 | [01-domain](01-domain.md) |
| TS-003 | `CartWindow` (list / cart / history) | TS-002 | [01-domain](01-domain.md) |
| TS-004 | `Department`, `DepartmentKeyword` rules | EN-001 | [01-domain](01-domain.md) |
| TS-005 | `ShoppingSettings` rules | EN-001 | [01-domain](01-domain.md) |
| EN-002 | `IShoppingDbContext`, idempotency (`CreateRequest`, `IdempotentCreates`) | TS-002 | [02-infrastructure](02-infrastructure.md) |
| EN-003 | `ShoppingDbContext`, configurations, seed from «Справочник», migration `Initial`, initializer | EN-002, TS-004, TS-005 | [02-infrastructure](02-infrastructure.md) |
| TS-006 | `AddItems` (parse, match, re-add, idempotent) | TS-001..TS-003, EN-002 | [03-application](03-application.md) |
| TS-007 | `UpdateItem` (+ learning), `SetItemStatus` | TS-006 | [03-application](03-application.md) |
| TS-008 | `DeleteItem`, `ClearCart` | TS-003 | [03-application](03-application.md) |
| TS-010 | `ListItems`, `ListHistory`, `ListFrequent` | TS-003 | [03-application](03-application.md) |
| TS-011 | `GetSummary` | TS-010 | [03-application](03-application.md) |
| TS-012 | Departments: list, match, create, update, delete → fallback, reorder | TS-004 | [03-application](03-application.md) |
| TS-013 | Keywords: list, create, update, delete | TS-004 | [03-application](03-application.md) |
| TS-014 | Settings: get, update | TS-005 | [03-application](03-application.md) |
| TS-015 | Endpoints, `ShoppingModule`, `VersionHeader`, OpenAPI regenerated | TS-006..TS-014 | [04-api](04-api.md) |
| TS-016 | Integration tests (`tests/IntegrationTests/Api/Shopping/`), architecture tests cover the module | TS-015, EN-003 | [04-api](04-api.md) |
| TS-017 | Docs: module page, ADR 0031–0032, decisions index, C4, README, AGENTS.md | TS-015 | [05-verification](05-verification.md) |
| TS-018 | Verification and code audit | all | [05-verification](05-verification.md) |
