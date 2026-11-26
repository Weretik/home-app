# Phase 08 — verification

## TS-015 — Docs: architecture, design, specs index

- **Task id:** TS-015
- **Covers:** all (documentation)
- **Depends on:** TS-006…TS-014
- **Exact paths:** `web/docs/architecture.md` (Gifts section with its folder tree, routes table `/gifts`,
  `/gifts/archive`, `/gifts/settings`, home `gifts-summary/`); `web/docs/design.md` («Gift lists», «Person colours»);
  `web/docs/specs/README.md` (status); this SDD's `README.md` and `traceability.md`
- **Test level:** documentation
- **Baseline:** AGENTS.md (docs in the same change), [web rules](../../../../rules.md)

### Work

- [ ] Write the actual folder map (from code, not this plan) into architecture.md; remove `gifts` from the «coming
      soon» route row.
- [ ] Add the «Gift lists» and «Person colours» patterns to design.md (replaced the first «Wish cards»).
- [ ] Update statuses and evidence links.
- [ ] Responsibility check of the docs (one place per fact).

### Evidence

- `web/docs/architecture.md`: Gifts section (Mermaid, tree, rules, Tests), `/gifts*` routes, home `gifts-summary/`.
- `web/docs/design.md`: «Gift lists» and «Person colours» (the «Wish card» pattern was removed after the first review). `web/docs/specs/README.md`: status. Status: done.

### Done when

architecture.md describes the built feature and its routes; no doc says gifts are «скоро».

## TS-016 — Verification and final feature audit

- **Task id:** TS-016
- **Covers:** all
- **Depends on:** all tasks
- **Exact paths:** whole `web/`
- **Test level:** checks
- **Baseline:** [web README, Commands](../../../../../README.md#commands); AGENTS.md commands

### Work

- [ ] `cd web && npm run typecheck && npm run lint && npm test && npm run architecture:structure && npm run format:check`.
- [ ] `npm run contracts:check` after the generated client is committed.
- [ ] Check every new file against the size rule (component ≤150, hook/module ≤200) — by the structure check once the
      separate change lands, by `wc -l` until then.
- [ ] Short audit of the feature against [frontend.md](../design/frontend.md) (boundaries, forbidden shortcuts, theme
      tokens only, no `features/expenses` import); record the result in this task.
- [ ] Browser/visual checks only if the user asks.

### Evidence

- Commands (2026-11-26, in `web/`): `npm run contracts:generate` (re-run at the end: generated file unchanged,
  same md5); `npm run typecheck` 0 errors; `npm run lint` exit 0; `npm run architecture:structure` «Structure is
  fine.»; `npm run format:check` clean; `npm test` 32 files / 162 tests passed; gifts and gifts summary alone
  15 files / 88 tests.
- `npm run contracts:check` not run green: it diffs against the committed client, so it passes once the regenerated
  `src/api-client/generated/api.ts` is committed with this change.
- Sizes: no production file within 30 lines of its limit (`wc -l`).
- Audit: [code-audit/audit.md](../code-audit/audit.md) — ready for review; AF-003…AF-006 fixed and verified, AF-001
  accepted, AF-002 (no project quality targets) open outside the feature.
- No browser or visual check was run.

### Done when

All commands pass and every `SC-*` in [traceability.md](../traceability.md) is `verified`.
