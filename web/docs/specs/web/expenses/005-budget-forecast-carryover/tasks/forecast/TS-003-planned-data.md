# TS-003 — Plan reads and writes; shared write hooks

- **Task ID:** TS-003
- **Covers:** SC-W5-002 … SC-W5-005
- **Depends on:** EN-001
- **Exact paths:** `src/features/expenses/planned-data/{planned-api.ts,planned-writes-api.ts}`, `src/features/expenses/write-data/` (under `web/`)
- **Test level:** component (through the page tests)
- **Architecture basis:** [web rules, API](../../../../../../rules.md#api), [web ADR 0007](../../../../../../decisions/0007-editing-in-dialogs-with-idempotency-and-versions.md)

## Work

Order: spec file first (Red) → code (Green) → refactor → regression → evidence.

- [ ] Red: page tests in TS-006 expect the requests (key, `If-Match`, record path).
- [ ] Green: `usePlannedExpenses(month)`; writers for plans and record; record invalidates `['expenses']` (since the audit every write does, ADR 0009).
- [ ] Refactor: move the generic save/remove hooks out of `data/writes-api.ts` into `write-data/` so `data/` stays ≤ 6 files.
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

Only `*-api.ts` call the client; architecture check passes.
