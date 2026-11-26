# Savings page — traceability (retroactive)

| Scenario  | Rules              | Code               | Test level       | Tests                                                                                                                                       | Status   |
| --------- | ------------------ | ------------------ | ---------------- | ------------------------------------------------------------------------------------------------------------------------------------------- | -------- |
| SC-W2-001 | R-W2-001, R-W2-002 | `savings-page/`    | component        | `SavingsPage.test.tsx` «shows the balance in the jar and signed, coloured movements»                                                        | verified |
| SC-W2-002 | R-W2-002, R-W2-003 | `savings-form.ts`  | unit + component | «takes money out with a minus sign and Виталий by default», `savings-form.test.ts` «signs the amount by the direction»                      | verified |
| SC-W2-003 | R-W2-004           | `savings-editing/` | component        | «edits a movement with If-Match and deletes after the confirmation»                                                                         | verified |
| SC-W2-004 | R-W2-005           | `savings-goal/`    | unit + component | «shows how far the jar is towards its goal», «says the goal is reached», «offers to set a goal when there is none», `goal-progress.test.ts` | verified |
| SC-W2-005 | R-W2-002           | `savings-form.ts`  | unit             | «edits a withdrawal as a positive amount taken out»                                                                                         | verified |

R-W2-006 (months overview expanding and opening a month) has no dedicated test.
