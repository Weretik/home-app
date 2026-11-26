# TS-005 — «Записать как расход» dialog

- **Task ID:** TS-005
- **Covers:** SC-W5-005
- **Depends on:** TS-003
- **Exact paths:** `src/features/expenses/planned-editing/{record-form.ts,record-form.test.ts,RecordDialog.tsx}` (under `web/`)
- **Test level:** unit + component
- **Architecture basis:** [requirements R-W5-004](../../requirements/behavior.md)

## Work

Order: spec file first (Red) → code (Green) → refactor → regression → evidence.

- [ ] Red: defaults today and `Card`; date and comment checks; request shape; page test for success and for 404/409.
- [ ] Green: form and dialog.
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

SC-W5-005 assertions pass.
