# TS-007 — Carry-over and plan notes on the limit rows

- **Task ID:** TS-007
- **Covers:** SC-W5-007
- **Depends on:** EN-001
- **Exact paths:** `src/features/home/expenses-summary/limits/{limit-rows.ts,limit-rows.test.ts,LimitNotes.tsx,LimitList.tsx,LimitTable.tsx,LimitsCard.tsx,LimitFill.tsx}` (under `web/`)
- **Test level:** unit + component
- **Architecture basis:** [web architecture, Home](../../../../../architecture.md#home-srcfeatureshome)

## Work

Order: spec file first (Red) → code (Green) → refactor → regression → evidence.

- [ ] Red: rows carry `carriedIn`, `carriesOut`, `planned` from the API (and the month's for «ИТОГО»); notes only when > 0.
- [ ] Green: rows and notes; move the limit files into `limits/`.
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

SC-W5-007 passes; no carry arithmetic in the web.
