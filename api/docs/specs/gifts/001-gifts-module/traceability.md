# Gifts — scenario traceability

Status `verified` means the acceptance test below passed on 2026-11-26 (integration tests against a local PostgreSQL
16, since Docker is not available here; CI runs them on Testcontainers).

| Scenario | Rules | Test level | Domain | Application | Infrastructure | API/contract | Acceptance evidence | Status |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| SC-001 | R-001…R-006, R-009 | domain, validator, integration | TS-001 | TS-004, TS-006 | EN-002, EN-003 | TS-010 | `GiftTests`, `GiftInputValidatorTests`, `GiftsApiTests.A_Gift_WithVariants_IsAdded_Edited_AndDeleted`, `Invalid_Input_IsAValidationProblem_AndAnUnknownCategory_IsNotFound` | verified |
| SC-002 | R-009 | integration | — | TS-004 | EN-002 | TS-010 | `GiftsApiTests.A_Repeated_Create_ReturnsTheFirstId_AndOtherData_IsAConflict` | verified |
| SC-003 | R-004, R-006, R-008 | domain, integration | TS-001 | TS-004 | EN-002 | TS-010 | `GiftTests.An_Update_ReplacesTheWholeVariantList_AndKeepsTheStatus`, `GiftsApiTests.Changing_Only_The_Variants_MovesTheVersion` | verified |
| SC-004 | R-008 | integration | — | TS-004 | EN-002 | TS-010 | `GiftsApiTests.A_Gift_WithVariants_IsAdded_Edited_AndDeleted` (stale and missing version) | verified |
| SC-005 | R-006, R-007 | domain, integration | TS-002 | TS-005, TS-006 | EN-002 | TS-010 | `GiftStatusTests`, `GiftsApiTests.A_Gift_MovesToTheArchive_AndBack` | verified |
| SC-006 | R-007 | integration | — | TS-006 | — | TS-010 | `GiftsApiTests.The_ActiveList_IsOrdered_ByPriority_ThenTheNearestOccasion` | verified |
| SC-007 | R-004 | integration | — | TS-004 | EN-002 (cascade) | TS-010 | `GiftsApiTests.A_Gift_WithVariants_IsAdded_Edited_AndDeleted` | verified |
| SC-008 | R-010 | domain, integration | TS-003 | TS-007 | EN-003 | TS-010 | `CategoryAndDetailTests`, `GiftsSettingsApiTests.The_Starting_Categories_AreSeeded_InOrder`, `A_Category_IsAdded_Renamed_AndDeleted_OnlyWhileUnused` | verified |
| SC-009 | R-008, R-011 | domain, validator, integration | TS-003 | TS-008 | EN-002 | TS-010 | `CategoryAndDetailTests`, `SettingsInputValidatorTests`, `GiftsSettingsApiTests.Details_AreAdded_InOrder_Edited_AndDeleted` | verified |
| SC-010 | R-012 | integration | — | TS-009 | — | TS-010 | `GiftsSettingsApiTests.The_Summary_CountsPerRecipient_AndListsTopsOccasionsAndDetails` | verified |
| SC-011 | R-013 | architecture | — | — | — | TS-010, TS-012 | `OpenApiDocumentTests`, `ContractSchemaNameTests`; `429` listed on every Gifts write in `openapi.json` | verified |

## Shared prerequisites

| Task | For scenarios | Reason | Check |
| --- | --- | --- | --- |
| EN-001 | all | the module must exist and be registered | architecture tests (module found, five layers, references) |
| EN-002 | SC-001…SC-010 | mappings and versions | integration tests |
| EN-003 | SC-008 and all others | schema and seed | migrations applied on start in the test host |

## Notes and justified gaps

- No handler unit tests with a fake database: handlers only order load → check → domain → save, and the integration
  tests exercise each of them on PostgreSQL (the project's practice, as in Expenses).
- Races (two copies of one create, a category deleted while a gift is added) are guarded by the unique key and the
  foreign key and mapped to the same answers as the sequential cases (audit AF-006). After the merge with the
  Expenses rule work, `IntegrationTests/Api/Gifts/GiftsRaceApiTests` reproduces the row-version races (gift edit,
  status change, detail edit) and the category delete raced by a new gift deterministically with
  `TestHost/RacingGiftsDb`; two copies of one create are still not reproduced (the unique key guards them).
