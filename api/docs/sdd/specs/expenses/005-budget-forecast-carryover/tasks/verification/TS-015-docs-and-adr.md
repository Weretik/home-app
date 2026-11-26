# TS-015 — Module page, ADR and indexes

- **Task ID:** TS-015
- **Covers:** —
- **Depends on:** TS-013
- **Exact paths:** `docs/modules/expenses.md`, `docs/decisions/0021-<carry-over-and-planned-expenses>.md`, `docs/decisions/README.md`, `docs/architecture/c4.md`, `docs/sdd/specs/expenses/README.md` (relative to `api/`)
- **Test level:** documentation
- **Architecture basis:** [AGENTS.md](../../../../../../../../AGENTS.md) ("a new lasting decision gets an ADR")

## Work

Order: Red → Green → Refactor → Regression → evidence.

- [ ] ADR: carry-over replaces "overspending is not carried" of ADR 0019; planned expenses as their own aggregate; `LimitsFrom`.
- [ ] Module page: model, rules, use cases, tables, HTTP table, pages (Бюджет / Прогноз).
- [ ] C4: no new container; check and note.
- [ ] Status of this SDD and traceability updated.
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

Docs describe the code as built; `backend-docs-sync` finds no gap.
