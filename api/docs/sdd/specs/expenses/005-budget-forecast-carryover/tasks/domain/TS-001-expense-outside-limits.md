# TS-001 — `Expense.OutsideLimits` on create and update

- **Task ID:** TS-001
- **Covers:** SC-OL-001, SC-OL-002
- **Depends on:** none
- **Exact paths:** `src/Modules/Expenses/Expenses.Domain/Entries/Expense.cs`; `tests/UnitTests/Expenses/Domain/Entries/ExpenseTests.cs` (relative to `api/`)
- **Test level:** domain unit
- **Architecture basis:** [API rules, Structure](../../../../../../rules.md#structure) (no public setters)

## Work

Order: Red → Green → Refactor → Regression → evidence.

- [ ] Red: a test that `Create(..., outsideLimits: true)` keeps the flag and `Update` replaces it; a failed update keeps the old flag.
- [ ] Green: add the property and parameters (default false).
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

Domain tests prove the default, the create value and the update value.
