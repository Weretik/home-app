# TS-010 — `DeletePlannedExpense` with `If-Match`

- **Task ID:** TS-010
- **Covers:** SC-FC-004
- **Depends on:** TS-004, EN-001
- **Exact paths:** `src/Modules/Expenses/Expenses.Application/Features/Planned/Commands/DeletePlannedExpense/` (relative to `api/`)
- **Test level:** integration
- **Architecture basis:** [requirements R-FC-003](../../requirements/forecast.md)

## Work

Order: Red → Green → Refactor → Regression → evidence.

- [ ] Red: delete with an old version → 409 and the plan stays; current version → 204; no header → 400; unknown → 404.
- [ ] Green: command carries the version; delete only when it matches (conditional delete or load + remove).
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

SC-FC-004 delete part passes, including the stale-version case.
