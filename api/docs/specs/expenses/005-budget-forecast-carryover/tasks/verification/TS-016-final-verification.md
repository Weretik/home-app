# TS-016 — Final verification and audit

- **Task ID:** TS-016
- **Covers:** all
- **Depends on:** TS-014, TS-015
- **Exact paths:** whole `api/` change (relative to `api/`)
- **Test level:** build, unit, architecture, integration (CI)
- **Architecture basis:** [backend-change-verification], [backend-code-audit]

## Work

Order: Red → Green → Refactor → Regression → evidence.

- [ ] `dotnet build HomeApp.slnx`, `dotnet test api/tests/UnitTests`, `dotnet test api/tests/ArchitectureTests`; integration in CI.
- [ ] Audit against this SDD and the API rules (module boundaries, folders ≤ 6, validators, one `SaveChanges`, OpenAPI conventions).
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

All checks green and the audit has no open finding.
