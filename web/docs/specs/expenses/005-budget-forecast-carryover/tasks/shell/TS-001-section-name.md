# TS-001 — Rename the section to «Бюджет»

- **Task ID:** TS-001
- **Covers:** SC-W5-001
- **Depends on:** none
- **Exact paths:** `src/app/shell/sections.ts`, `src/app/shell/AppLayout.test.tsx`, `src/features/expenses/section/ExpensesSection.tsx`, `src/features/home/page/summary-sections.ts`, `src/features/home/page/HomePage.test.tsx` (under `web/`)
- **Test level:** component
- **Architecture basis:** [web architecture, Routes](../../../../../architecture.md#routes)

## Work

Order: spec file first (Red) → code (Green) → refactor → regression → evidence.

- [ ] Red: tests expect «Бюджет» in the menu, the section title and the switcher, and «Расходы» as the first tab.
- [ ] Green: change the labels.
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

Shell and home tests pass with the new name.
