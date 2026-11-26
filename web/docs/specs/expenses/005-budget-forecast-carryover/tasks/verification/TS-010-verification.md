# TS-010 — Phone layout and final checks

- **Task ID:** TS-010
- **Covers:** SC-W5-010, all
- **Depends on:** TS-001 … TS-009
- **Exact paths:** whole `web/` change (under `web/`)
- **Test level:** component + manual
- **Architecture basis:** [web rules, Tests and checks](../../../../../rules.md#tests-and-checks)

## Work

Order: spec file first (Red) → code (Green) → refactor → regression → evidence.

- [ ] Page tests at phone width; manual check at 360 px in light and dark.
- [x] `npm run typecheck && npm run lint && npm test && npm run architecture:structure` (2026-11-26, audit).
- [x] Update `web/docs/architecture.md` (routes, folders, «Бюджет») and this SDD's status; audit with `frontend-code-audit`
      ([code-audit/audit.md](../../code-audit/audit.md)).
- [x] Check the changed files for one responsibility each (audit AF-05 … AF-09); split a file that mixes independent responsibilities along
      the feature's role folders; keep ≤ 6 production files per folder (web rules, Structure).

## Evidence

- Test path and assertion:
- Red command and expected failure:
- Green command and result:
- Refactor note and re-run:
- Regression (`npm test`, `npm run typecheck`, `npm run lint`, `npm run architecture:structure`): all green on
  2026-11-26 after the audit fixes, see [code-audit/audit.md](../../code-audit/audit.md#verification).
- Manual or visual evidence (phone 360 px, both schemes) if needed:
- Deviation or blocker:

## Done when

All checks green, docs match, audit has no open finding.
