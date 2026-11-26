# TS-007 — `ListPlannedExpenses`

- **Task ID:** TS-007
- **Covers:** SC-FC-001
- **Depends on:** TS-004, EN-001
- **Exact paths:** `src/Modules/Expenses/Expenses.Application/Features/Planned/Queries/ListPlannedExpenses/` (relative to `api/`)
- **Test level:** integration
- **Architecture basis:** [API rules, Data](../../../../../../rules.md#data) (projection, `AsNoTracking`)

## Work

Order: Red → Green → Refactor → Regression → evidence.

- [ ] Red: two plans listed by name with `categoryName`, `version`, `totalAmount`.
- [ ] Green: validator (year, month), projection joined to categories, ordered by name. No paging needed (≤ 30 rows a month); record the decision.
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

SC-FC-001 list part passes.
