# TS-005 — Create, update and list expenses with `outsideLimits`

- **Task ID:** TS-005
- **Covers:** SC-OL-001, SC-OL-002, SC-OL-005
- **Depends on:** TS-001, EN-001
- **Exact paths:** `src/Modules/Expenses/Expenses.Application/Features/Expenses/Commands/{CreateExpense,UpdateExpense}/`, `.../Queries/ListExpenses/ExpenseItem.cs`, `ListExpensesHandler.cs`; `src/Modules/Expenses/Expenses.Api/Endpoints/Expenses/{ExpenseRequest,CreateExpenseEndpoint,UpdateExpenseEndpoint}.cs` (relative to `api/`)
- **Test level:** integration (API)
- **Architecture basis:** [API rules, Use cases](../../../../../../rules.md#use-cases)

## Work

Order: Red → Green → Refactor → Regression → evidence.

- [ ] Red: integration test creates with the flag, lists it, edits it; same key with another flag → 409.
- [ ] Green: request → command → domain; fingerprint includes the flag; item projection.
- [ ] Check the changed files for one responsibility each; split a file that mixes independent responsibilities
      along the module's folders (Domain by aggregate, Application `Features/<Feature>/Commands|Queries/<UseCase>/`,
      Api by feature); keep the six-files-per-folder rule.

## Evidence

- Test and assertion:
- Red command and expected behavioral failure:
- Green command and result:
- Refactor note and re-run:
- Regression command and result:
- Deviation or blocker:

## Done when

SC-OL-001, SC-OL-002, SC-OL-005 pass in `IntegrationTests`.
