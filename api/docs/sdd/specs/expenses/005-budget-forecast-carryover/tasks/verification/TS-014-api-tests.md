# TS-014 — Integration test suite for the feature

- **Task ID:** TS-014
- **Covers:** all SC-*
- **Depends on:** TS-013
- **Exact paths:** `tests/IntegrationTests/Api/Expenses/` (new test class for planned expenses and carry-over) (relative to `api/`)
- **Test level:** integration (Docker, run by CI)
- **Architecture basis:** [AGENTS.md, Commands](../../../../../../../../AGENTS.md)

## Work

Order: Red → Green → Refactor → Regression → evidence.

- [ ] One test per scenario row of [traceability](../../traceability.md), named after the behavior.
- [ ] CI run green (locally only on request).
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

Every `SC-*` row in traceability has a passing test name.
