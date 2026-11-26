# TS-008 — `CreatePlannedExpense` with `Idempotency-Key`

- **Task ID:** TS-008
- **Covers:** SC-FC-001, SC-FC-002, SC-FC-003
- **Depends on:** TS-004, EN-001
- **Exact paths:** `src/Modules/Expenses/Expenses.Application/Features/Planned/Commands/CreatePlannedExpense/` (relative to `api/`)
- **Test level:** integration + validator unit
- **Architecture basis:** [ADR 0007](../../../../../../decisions/0007-idempotency-key-for-creates.md)

## Work

Order: Red → Green → Refactor → Regression → evidence.

- [ ] Red: create → 201; repeat → same id; other data → 409; invalid → 400; unknown category → 404.
- [ ] Green: `IdempotentCreates` with kind `planned`, category check, one `SaveChanges`, unique violation answered like a repeat.
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

SC-FC-002 and SC-FC-003 pass; the race path is covered by the shared `IdempotentCreates` behavior.
