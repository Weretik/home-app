# Shopping list — traceability

| Scenario | Rules | Test level | Domain | Application | Infrastructure | API | Evidence | Status |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| SC-001 | R-001, R-002 | unit + integration | TS-001 | TS-006 | EN-003 | TS-015 | `ItemTextParserTests`, `ShoppingItemsApiTests.Text_IsSplitIntoItems_InTheirDepartments` | verified (unit), CI (integration) |
| SC-002 | R-002 | unit | TS-001 | TS-006 | EN-003 | TS-015 | `DepartmentMatcherTests` | verified |
| SC-003 | R-003 | unit + integration | TS-002 | TS-006 | — | TS-015 | `ShoppingItemTests`, `ShoppingItemsApiTests.ReAdding_Updates_OrRestores` | verified (unit), CI |
| SC-004 | R-005 | unit + integration | TS-002 | TS-007 | — | TS-015 | `ShoppingItemTests.Status_*`, `ShoppingItemsApiTests.Ticking_*` | verified (unit), CI |
| SC-005 | R-004 | unit | TS-003 | TS-007, TS-010 | — | — | `CartWindowTests` | verified |
| SC-006 | R-006 | integration | — | TS-008 | — | TS-015 | `ShoppingItemsApiTests.Clearing_TheCart_*` | CI |
| SC-007 | R-007, R-015 | unit + integration | TS-002, TS-004 | TS-007 | — | TS-015 | `ShoppingDictionaryApiTests.Moving_AnItem_LearnsAKeyword` | CI |
| SC-008 | R-009, R-010 | integration | — | TS-010 | — | TS-015 | `ShoppingItemsApiTests.History_AndFrequent_*` | CI |
| SC-009 | R-012 | unit + integration | TS-004 | TS-012 | EN-003 | TS-015 | `DepartmentTests`, `ShoppingDictionaryApiTests.Departments_*` | verified (unit), CI |
| SC-010 | R-013 | unit + integration | TS-004 | TS-013 | — | TS-015 | `DepartmentTests.Keywords_*`, `ShoppingDictionaryApiTests.Keywords_*` | verified (unit), CI |
| SC-011 | R-014 | unit + integration | TS-005 | TS-014 | — | TS-015 | `ShoppingSettingsTests`, `ShoppingSettingsApiTests` | verified (unit), CI |
| SC-012 | R-015 | integration | — | EN-002 | — | TS-015 | `ShoppingItemsApiTests.A_Repeated_Add_*` | CI |
| SC-013 | R-011 | integration | — | TS-011 | — | TS-015 | `ShoppingItemsApiTests.Summary_*` | CI |
| SC-014 | R-016 | integration | — | — | EN-003 | — | `ShoppingDictionaryApiTests.The_Dictionary_ComesFromTheMigration` | CI |

"CI": the integration tests compile locally (no Docker in the authoring environment) and run in CI.

## Shared enablers

| Task | Scenarios | Reason | Check |
| --- | --- | --- | --- |
| EN-001 | all | five projects, solution, host registration | `dotnet build HomeApp.slnx`, architecture tests |
| EN-002 | SC-001, SC-009, SC-010, SC-012 | idempotency table and port | integration test of repeats |
| EN-003 | SC-001, SC-002, SC-014 | DbContext, configurations, seed, migration | `dotnet ef migrations has-pending-model-changes` clean |
