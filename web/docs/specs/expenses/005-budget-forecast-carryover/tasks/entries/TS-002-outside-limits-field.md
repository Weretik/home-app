# TS-002 — «Не в лимиты» in the expense dialog and the list chip

- **Task ID:** TS-002
- **Covers:** SC-W5-006
- **Depends on:** EN-001
- **Exact paths:** `src/features/expenses/expense-editing/{expense-form.ts,expense-form.test.ts,ExpenseDialog.tsx}`, `src/features/expenses/expense-list/{OutsideLimitsChip.tsx,ExpenseList.tsx,ExpenseTable.tsx,ExpensesPage.test.tsx}` (under `web/`)
- **Test level:** unit + component
- **Architecture basis:** [web rules, Writes](../../../../../rules.md#writes)

## Work

Order: spec file first (Red) → code (Green) → refactor → regression → evidence.

- [ ] Red: form test (new → false, edit keeps the value, request sends it); page test (checkbox with helper text, chip on marked rows only).
- [ ] Green: field, checkbox, chip.
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

SC-W5-006 assertions pass.
