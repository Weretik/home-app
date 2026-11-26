# TS-006 — «Прогноз» tab and route

- **Task ID:** TS-006
- **Covers:** SC-W5-002 … SC-W5-005, SC-W5-010
- **Depends on:** TS-004, TS-005
- **Exact paths:** `src/features/expenses/forecast-page/{ForecastPage.tsx,PlannedList.tsx,PlannedTable.tsx,planned-rows.tsx,ForecastPage.test.tsx}`, `src/features/expenses/section/expenses-pages.ts`, `src/app/root/App.tsx` (under `web/`)
- **Test level:** component
- **Architecture basis:** [web ADR 0006](../../../../../decisions/0006-section-pages-as-tabs-with-state-in-the-url.md)

## Work

Order: spec file first (Red) → code (Green) → refactor → regression → evidence.

- [ ] Red: page test lists plans and «Запланировано», empty state, add with key reuse on retry, edit with `If-Match`, delete after confirmation, record.
- [ ] Green: page, list, table, tab entry, nested route.
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

`ForecastPage.test.tsx` covers SC-W5-002 … 005.
