# TS-009 — «Прогноз» card on the home summary

- **Task ID:** TS-009
- **Covers:** SC-W5-009
- **Depends on:** EN-001
- **Exact paths:** `src/features/home/expenses-summary/data/figure-lines.ts`, `src/features/home/expenses-summary/{figures/ForecastCard.tsx,overview/ExpensesSummary.tsx}` (was `view/` until the audit), `src/features/home/page/HomePage.test.tsx` (under `web/`)
- **Test level:** unit + component
- **Architecture basis:** [design rules](../../../../../design.md)

## Work

Order: spec file first (Red) → code (Green) → refactor → regression → evidence.

- [ ] Red: `forecastLines` with and without a monthly limit, tones; page shows the card.
- [ ] Green: card.
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

SC-W5-009 passes.
