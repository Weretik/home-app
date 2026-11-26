# TS-004 — `PlannedExpense` aggregate

- **Task ID:** TS-004
- **Covers:** SC-FC-001, SC-FC-003
- **Depends on:** none
- **Exact paths:** `src/Modules/Expenses/Expenses.Domain/Planning/{PlannedExpense,PlannedExpenseErrors}.cs`; `tests/UnitTests/Expenses/Domain/Entries/PlannedExpenseTests.cs` (move to `Domain/Planning/` when that test folder gets a second file) (relative to `api/`)
- **Test level:** domain unit
- **Architecture basis:** [API rules, Structure](../../../../../rules.md#structure)

## Work

Order: Red → Green → Refactor → Regression → evidence.

- [ ] Red: trimmed name 1–100, month is the first day 2000–2100, amount money rules, failed update keeps old values.
- [ ] Green: `Create`, `Update`.
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

Domain tests cover each rule of R-FC-001.
