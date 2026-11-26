# Savings jar — scenario traceability (retroactive)

| Scenario | Rules | Test level | Code | Evidence | Status |
| --- | --- | --- | --- | --- | --- |
| SC-SV-001 | R-SV-001 … R-SV-004 | integration + domain | `ListSavingsMovementsHandler`, `ListSavingsMonthsHandler` | `ExpensesSavingsApiTests.Savings_Movements_AddUp_ByMonth`, `SavingsMovementTests.The_Sign_TellsADeposit_FromAWithdrawal` | verified |
| SC-SV-002 | R-SV-001 | integration + domain | `EntryRules.CheckSigned` | `Savings_Movements_AddUp_ByMonth` (zero → 400), `SavingsMovementTests.A_Movement_NeedsAnAmount_ThatFitsTheRules` | verified |
| SC-SV-003 | R-SV-005 | integration + domain | `UpdateSavingsMovementHandler` | `Savings_Movements_AddUp_ByMonth` (edit), `SavingsMovementTests.An_Update_ChangesTheFields_AndKeepsTheImportKey` | verified |
| SC-SV-004 | R-SV-005 | integration + validator unit | create/update/delete handlers | `ExpensesSavingsApiTests.A_Movement_IsRepeatedOnce_RefusedWhenStale_AndDeleted`, `EntryInputValidatorTests.A_SavingsMovement_IsSigned_ButNeverZero` | verified |
| SC-SV-005 | R-SV-006 | integration | `GetBudgetHandler` | `ExpensesBudgetApiTests.The_Settings_AreSeeded_AndAnEdit_WithAnOldVersion_IsAConflict` | verified |
