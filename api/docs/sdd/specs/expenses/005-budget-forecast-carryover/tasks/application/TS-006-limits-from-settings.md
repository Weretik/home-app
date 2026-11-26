# TS-006 — Read and change `limitsFrom`

- **Task ID:** TS-006
- **Covers:** SC-CO-007, SC-CO-008
- **Depends on:** TS-002, EN-001
- **Exact paths:** `src/Modules/Expenses/Expenses.Application/Features/Budget/Commands/UpdateBudgetSettings/`, `.../Queries/GetBudget/`; `src/Modules/Expenses/Expenses.Api/Endpoints/Budget/Overview/BudgetSettingsRequest.cs`; `tests/UnitTests/Expenses/Input/BudgetInputValidatorTests.cs` (relative to `api/`)
- **Test level:** validator unit + integration
- **Architecture basis:** [ADR 0017](../../../../../../decisions/0017-row-version-via-xmin-for-edits.md)

## Work

Order: Red → Green → Refactor → Regression → evidence.

- [ ] Red: budget returns 2026-11-01; save with `limitsFrom` changes it; omitted keeps it; out-of-range → 400; old version → 409.
- [ ] Green: optional field in request and command; validator range rule.
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

SC-CO-007 and SC-CO-008 pass.
