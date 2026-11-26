# TS-008 — «Вне лимитов» KPI line

- **Task ID:** TS-008
- **Covers:** SC-W5-008
- **Depends on:** EN-001
- **Exact paths:** `src/features/home/expenses-summary/data/{figure-lines.ts,figure-lines.test.ts}`, `src/features/home/expenses-summary/figures/KpiCards.tsx` (was `view/` until the audit) (under `web/`)
- **Test level:** unit + component
- **Architecture basis:** [requirements R-W5-006](../../requirements/behavior.md)

## Work

Order: spec file first (Red) → code (Green) → refactor → regression → evidence.

- [ ] Red: line text with plural forms (1 трата, 2 траты, 5 трат); null at 0.
- [ ] Green: `outsideLimitsLine` and its place in the KPI cards.
- [ ] Check the changed files for one responsibility each; split a file that mixes independent responsibilities along
      the feature's role folders; keep ≤ 6 production files per folder (web rules, Structure).

## Evidence

- Test path and assertion:
- Red command and expected failure:
- Green command and result:
- Refactor note and re-run:
- Regression (`npm test`, `npm run typecheck`, `npm run lint`, `npm run architecture:structure`):
- Manual or visual evidence (phone 360 px, both schemes) if needed:
- Deviation or blocker:

## Done when

SC-W5-008 passes.
