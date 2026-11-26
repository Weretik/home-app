# TS-011 — `RecordPlannedExpense`: plan → expense in one transaction

- **Task ID:** TS-011
- **Covers:** SC-FC-005, SC-FC-006
- **Depends on:** TS-001, TS-004, EN-001
- **Exact paths:** `src/Modules/Expenses/Expenses.Application/Features/Planned/Commands/RecordPlannedExpense/` (relative to `api/`)
- **Test level:** integration
- **Architecture basis:** [API rules, Use cases](../../../../../rules.md#use-cases) (one `SaveChanges`)

## Work

Order: Red → Green → Refactor → Regression → evidence.

- [ ] Red: record → 201 with expense id, expense listed with plan values and `outsideLimits` false, plan gone; repeat → 404 and one expense; stale version → 409, no expense, plan stays.
- [ ] Green: load plan tracked, version check, `Expense.Create`, add + remove, one `SaveChanges`.
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

SC-FC-005 and SC-FC-006 pass.
