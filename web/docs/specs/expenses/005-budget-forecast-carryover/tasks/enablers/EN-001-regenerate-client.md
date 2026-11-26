# EN-001 — Regenerate the typed client from the new contract

- **Task ID:** EN-001
- **Covers:** SC-W5-002 … SC-W5-009
- **Depends on:** backend TS-013 (api 005)
- **Exact paths:** `src/api-client/generated/api.ts` (under `web/`)
- **Test level:** contract check (typecheck)
- **Architecture basis:** [web ADR 0003](../../../../../decisions/0003-typed-client-from-openapi.md)

## Work

Order: spec file first (Red) → code (Green) → refactor → regression → evidence.

- [ ] `npm run contracts:generate` after the backend contract is committed; commit the generated file in the same change.
- [ ] Replacement check (no behavioral Red): `npm run typecheck` shows the new fields.
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

The generated file matches `api/docs/contracts/openapi.json`; typecheck passes.
