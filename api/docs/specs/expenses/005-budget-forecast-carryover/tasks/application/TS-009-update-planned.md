# TS-009 — `UpdatePlannedExpense` with `If-Match`

- **Task ID:** TS-009
- **Covers:** SC-FC-004
- **Depends on:** TS-004, EN-001
- **Exact paths:** `src/Modules/Expenses/Expenses.Application/Features/Planned/Commands/UpdatePlannedExpense/` (relative to `api/`)
- **Test level:** integration
- **Architecture basis:** [ADR 0017](../../../../../decisions/0017-row-version-via-xmin-for-edits.md)

## Work

Order: Red → Green → Refactor → Regression → evidence.

- [ ] Red: current version → 204; old version → 409; unknown id → 404; no header → 400.
- [ ] Green: load, compare version, update, save; `DbUpdateConcurrencyException` → 409.
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

SC-FC-004 edit part passes.
