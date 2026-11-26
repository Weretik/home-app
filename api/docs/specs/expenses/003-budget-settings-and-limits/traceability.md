# Budget settings and spending limits — scenario traceability (retroactive)

| Scenario | Rules | Test level | Code | Evidence | Status |
| --- | --- | --- | --- | --- | --- |
| SC-BG-001 | R-BG-006, R-BG-003 | integration | `Budget` migration, `GetBudgetHandler` | `ExpensesBudgetApiTests.The_Migration_Seeds_SevenLimits_InSheetOrder` | verified |
| SC-BG-002 | R-BG-004 | integration + unit | `UpdateBudgetSettings*`, `BudgetSettings` | `ExpensesBudgetApiTests.The_Settings_AreSeeded_AndAnEdit_WithAnOldVersion_IsAConflict`, `BudgetSettingsTests.*`, `BudgetInputValidatorTests.Settings_NeedTheVersion_AndAmountsFromZero` | verified |
| SC-BG-003 | R-BG-001, R-BG-003, R-BG-005 | integration | limit commands | `ExpensesBudgetApiTests.A_Limit_IsAdded_Changed_AndDeleted` | verified |
| SC-BG-004 | R-BG-005 | integration | `CreateSpendingLimitHandler` | `ExpensesBudgetApiTests.A_Repeated_Create_ReturnsTheFirstId_AndOtherData_IsAConflict` | verified |
| SC-BG-005 | R-BG-001 | integration + unit | `BudgetCategories`, validators, `SpendingLimit` | `ExpensesBudgetApiTests.A_Limit_WithAnUnknownCategory_IsNotFound_AndWithoutCategories_IsAValidationProblem`, `SpendingLimitTests.*`, `BudgetInputValidatorTests.A_Create_NeedsAKey_AName_APositiveAmount_AndCategories` | verified |
| SC-BG-006 | R-BG-002 | domain unit | `SpendingLimit.PeriodAround` | `SpendingLimitTests.A_Monthly_Limit_CoversTheCalendarMonth`, `SpendingLimitTests.A_Quarterly_Limit_CoversTheCalendarQuarter` | verified |
