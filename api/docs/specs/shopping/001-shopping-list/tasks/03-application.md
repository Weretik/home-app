# 03 — Application

Folder per use case under `Shopping.Application/Features/<Area>/Commands|Queries/<UseCase>/` with command or query,
validator (when it has input) and handler. Handlers take `TimeProvider` for "now". Commands save once (department
delete: one explicit transaction). Test level: validator unit tests in `tests/UnitTests/Shopping/Input`; handler
behaviour end to end in `tests/IntegrationTests/Api/Shopping` (the Expenses pattern: handlers depend on EF Core
queries, so their tests run against PostgreSQL).

| Task | Use cases | Validators (shape) | Repeat |
| --- | --- | --- | --- |
| TS-006 | `Items/Commands/AddItems` | key 1..100, text 1..1000 and 1..50 parsed items, addedBy enum, departmentId not empty when given | Idempotency-Key |
| TS-007 | `Items/Commands/UpdateItem`, `Items/Commands/SetItemStatus` | id, version ≠ 0, name 1..80, quantity ≤30, note ≤200, departmentId; status/by enums | version / target value |
| TS-008 | `Items/Commands/DeleteItem`, `Items/Commands/ClearCart` | id, version ≠ 0 | version / no-op |
| TS-010 | `Items/Queries/ListItems`, `ListHistory`, `ListFrequent` | page ≥1, pageSize 1..100 | — |
| TS-011 | `Items/Queries/GetSummary` | — | — |
| TS-012 | `Departments/Commands/{Create,Update,Delete,Reorder}Department(s)`, `Departments/Queries/{ListDepartments,MatchDepartment}` | name 1..40, icon ≤8, key, version; ids non-empty and distinct; match name 1..80 | key / version / full order |
| TS-013 | `Keywords/Commands/{Create,Update,Delete}Keyword`, `Keywords/Queries/ListKeywords` | keyword 1..60 raw, departmentId, key, version; search ≤100 | key / version |
| TS-014 | `Settings/Queries/GetSettings`, `Settings/Commands/UpdateSettings` | version ≠ 0, hours 1..72, days 30..730 | version |

Shared helpers: `Features/Items/Views/` (`ShoppingItemView`, `ItemViews` projection, `FrequentStatistics`),
`Features/Dictionary/` (`DepartmentDictionary` builds the matcher in store order, `KeywordLearning`).
