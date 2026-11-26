# TS-004 — Plan form and add / edit / delete dialogs

- **Task ID:** TS-004
- **Covers:** SC-W5-003, SC-W5-004
- **Depends on:** TS-003
- **Exact paths:** `src/features/expenses/planned-editing/{planned-form.ts,planned-form.test.ts,PlannedDialog.tsx,PlannedEditor.tsx}`, `src/features/expenses/entry-dialogs/CategoryField.tsx` (under `web/`)
- **Test level:** unit + component
- **Architecture basis:** [web architecture, Expenses](../../../../../architecture.md#expenses-srcfeaturesexpenses)

## Work

Order: spec file first (Red) → code (Green) → refactor → regression → evidence.

- [ ] Red: form test (defaults: empty name, payer `Shared`; checks name 1–100, category, amount; request trims the name).
- [ ] Green: form, dialog, delete confirmation, shared category field.
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

Form tests pass; dialogs disable submit while pending.
