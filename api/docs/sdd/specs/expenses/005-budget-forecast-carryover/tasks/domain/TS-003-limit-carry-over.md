# TS-003 — `LimitCarryOver`: carry arithmetic by month and quarter

- **Task ID:** TS-003
- **Covers:** SC-CO-001 … SC-CO-006
- **Depends on:** none
- **Exact paths:** `src/Modules/Expenses/Expenses.Domain/Budget/LimitCarryOver.cs`; `tests/UnitTests/Expenses/Domain/Budget/LimitCarryOverTests.cs` (relative to `api/`)
- **Test level:** domain unit
- **Architecture basis:** [ADR 0019](../../../../../../decisions/0019-spending-limits-as-category-sets.md), [requirements](../../requirements/carry-over.md)

## Work

Order: Red → Green → Refactor → Regression → evidence.

- [ ] Red: the tables of SC-CO-001 (8 000 / 5 000 / 0 with limit 7 000), SC-CO-002 (3 000 into January), SC-CO-003, SC-CO-004 (October ignored), SC-CO-005 (quarter, 500 carried), amount 0 → (0, 0).
- [ ] Green: first period, period start, `(CarriedIn, CarriesOut)` via a spending function.
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

Every example table of [carry-over](../../requirements/carry-over.md) is a passing unit test.
