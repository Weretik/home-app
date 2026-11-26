# Budget settings page — traceability (retroactive)

| Scenario  | Rules              | Code                      | Test level | Tests                                                                                         | Status   |
| --------- | ------------------ | ------------------------- | ---------- | --------------------------------------------------------------------------------------------- | -------- |
| SC-W3-001 | R-W3-001, R-W3-002 | `settings-page/`          | component  | `SettingsPage.test.tsx` «shows the monthly limit, the savings goal and the limits»            | verified |
| SC-W3-002 | R-W3-001           | `BudgetSettingsCard.tsx`  | component  | «saves the limit and the goal with the version in If-Match»                                   | verified |
| SC-W3-003 | R-W3-001           | `budget-settings-form.ts` | component  | «checks the amounts before saving the settings»                                               | verified |
| SC-W3-004 | R-W3-003, R-W3-004 | `limit-editing/`          | component  | «adds a limit with an Idempotency-Key and its categories»                                     | verified |
| SC-W3-005 | R-W3-004           | `limit-editing/`          | component  | «changes a limit with its version in If-Match», «deletes a limit only after the confirmation» | verified |
| SC-W3-006 | R-W3-003           | `limit-form.ts`           | unit       | `limit-form.test.ts` (all three tests)                                                        | verified |
