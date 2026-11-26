# Entries and sheet import — scenario traceability (retroactive)

The behavior is implemented; this table links each scenario to the code and to the existing test that proves it.
`verified` = an existing automated test asserts the scenario; `implemented, untested` = code exists, no test found.
Test names are `Class.Method`; the test files are being moved into subfolders (`IntegrationTests/Api/Expenses/`,
`UnitTests/Expenses/Domain/<Aggregate>/`), the class names stay.

| Scenario | Rules | Test level | Code | Evidence | Status |
| --- | --- | --- | --- | --- | --- |
| SC-EN-001 | R-EN-004, R-EN-006 | integration | `Features/Expenses/Queries/ListExpenses/` | `ExpensesApiTests.An_Import_StoresTheRows_AndARepeat_SkipsThem` (lists the month, filters by payer, checks `totalCount`/`totalAmount`), `ExpensesPeriodApiTests.A_Period_ListsItsDays_BothEndsIncluded_WithTheirTotal` | verified |
| SC-EN-002 | R-EN-004 | integration + unit | `ListExpensesQuery.Period()` | `ExpensesPeriodApiTests.A_Period_ListsItsDays_BothEndsIncluded_WithTheirTotal`, `ListExpensesValidatorTests.The_Period_OfDays_IncludesTheLastDay` | verified |
| SC-EN-003 | R-EN-004, R-EN-005 | unit + integration | `ListExpensesValidator`, `ListIncomesValidator` | `ListExpensesValidatorTests.*`, `ExpensesPeriodApiTests.A_Wrong_Period_IsAValidationProblem`, `ExpensesApiTests.A_Month_OutsideTheCalendar_IsAValidationProblem` | verified |
| SC-EN-004 | R-EN-007 … R-EN-009 | integration | expense commands | `ExpensesEditingApiTests.An_Expense_IsAdded_Changed_AndDeleted` | verified for expenses; income edit/delete implemented, untested (GAP-003) |
| SC-EN-005 | R-EN-007 | integration | `Idempotency/IdempotentCreates.cs` | `ExpensesEditingApiTests.A_Repeated_Create_ReturnsTheFirstId_AndOtherData_IsAConflict` (sends incomes) | verified for incomes; same shared code for expenses, untested (GAP-003) |
| SC-EN-006 | R-EN-007, R-EN-008 | integration | validators, `EntryVersionHeader` | `ExpensesEditingApiTests.A_Create_WithoutAKey_OrAnEdit_WithoutAVersion_IsAValidationProblem` | verified |
| SC-EN-007 | R-EN-001 … R-EN-003 | domain unit | `EntryRules`, `Expense`, `Income` | `ExpenseTests.*`, `IncomeAndCategoryTests.*`, `SavingsMovementTests.An_Expense_Update_RejectsANonPositiveAmount_AndKeepsTheOldValues` | verified in domain; 404 category on create not tested at API level |
| SC-EN-008 | R-EN-010 | integration | `Endpoints/Import/ImportSheetEndpoint.cs` | `ExpensesApiTests.The_Import_NeedsTheKey` | verified |
| SC-EN-009 | R-EN-010 | integration + unit | `ImportSheetHandler`, `ImportKeys` | `ExpensesApiTests.An_Import_StoresTheRows_AndARepeat_SkipsThem`, `ExpensesApiTests.Identical_Rows_AreBoth_Imported`, `ImportKeysTests.*` | verified |
| SC-EN-010 | R-EN-010 | integration + unit | `SheetReader`, `ImportSheetHandler` | `ExpensesApiTests.A_Bad_Row_StopsTheWholeImport_AndNamesTheRow`, `SheetReaderTests.*` | verified |

## Notes

- Categories in sheet order: `ExpensesApiTests.Categories_ComeFromTheMigration_InSheetOrder`.
- Gaps are listed in [overview](requirements/overview.md#open-questions-and-gaps-found-while-writing-this-sdd).
