# TS-002 — `BudgetSettings.LimitsFrom` and its range rule

- **Task ID:** TS-002
- **Covers:** SC-CO-007, SC-CO-008
- **Depends on:** none
- **Exact paths:** `src/Modules/Expenses/Expenses.Domain/Budget/BudgetSettings.cs`; `tests/UnitTests/Expenses/Domain/Budget/BudgetSettingsTests.cs` (relative to `api/`)
- **Test level:** domain unit
- **Architecture basis:** [API rules](../../../../../../rules.md)

## Work

Order: Red → Green → Refactor → Regression → evidence.

- [ ] Red: dates 1999-12-31 and 2101-01-01 are rejected with `expenses.budget_settings_invalid` and the old values stay; 2026-12-01 is stored.
- [ ] Green: extend `Update`.
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

`BudgetSettingsTests` cover valid, invalid and unchanged values.
