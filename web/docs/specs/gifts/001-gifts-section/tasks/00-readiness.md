# Phase 00 — readiness

**Purpose:** scope, tools and prerequisites before behaviour tasks.

Done in this specification: `R-001…R-012`, `SC-001…SC-023`, the architecture baseline and file map
([frontend.md](../design/frontend.md)), traceability. Remaining prerequisite:

## EN-001 — Confirm the Gifts contract and regenerate the client

- **Task id:** EN-001
- **Enables:** every `SC-*`
- **Depends on:** the backend Gifts endpoints in `api/docs/contracts/openapi.json` (API half, in progress)
- **Exact paths:** `web/src/api-client/generated/api.ts` (generated); `web/docs/specs/gifts/001-gifts-section/contracts/api-contract.md`
- **Test level:** generation check
- **Baseline:** [web ADR 0003](../../../../decisions/0003-typed-client-from-openapi.md); AGENTS.md merge order (API → client → web)

### Work

- [ ] Run `cd web && npm run contracts:generate` on the branch that has the Gifts endpoints.
- [ ] Replace every «to confirm» in `contracts/api-contract.md` with the real `operationId`s, schema names, list
      shape (array or wrapper), `If-Match` on status and category writes, and the `gifts.*` error codes (read them
      from the backend's `*Errors.cs` and `api/docs/modules/gifts.md`); update assumptions 2–7 in
      `design/frontend.md` if any differ, and the affected `SC-*` texts only if behaviour changes.
- [ ] Record the provider commit SHA.
- [ ] Responsibility check of changed files (generated file exempt).

### Evidence

- Why no behavioural Red: generation only.
- Tool: `openapi-typescript` via `npm run contracts:generate`.
- Check: `npm run typecheck` passes; `npm run contracts:check` clean after commit.
- Result: —
- Unblocks: TS-002, TS-003, TS-014.
- Deviation: —

### Done when

The generated client contains every Gifts operation and the contract document names them exactly.
