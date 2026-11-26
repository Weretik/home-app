# Delivery readiness

Also check where applicable: architecture.md Gifts section and routes updated; specs index and `traceability.md`
current; generated client committed after the backend change; loading, empty and error states; light and dark
themes, phone layout and Russian text; the feature audit against the architecture rules done and findings fixed.

- [x] Every in-scope `SC-*` is `verified`.
- [x] Every `TS-*`/`EN-*` met its done condition.
- [x] New testable behaviour has Red → Green → Refactor → Regression evidence.
- [x] Contract, docs and code agree (EN-001 names, architecture.md, design.md «Wish cards»).
- [x] Boundaries reviewed after the last change: no `features/expenses` import, API only from `*-api.ts`.
- [x] Every changed file has one responsibility and fits the size rule (component ≤150, hook/module ≤200 lines);
      folders 2–6 production files.
- [x] Planned file map compared with the real files; architecture.md shows the real map.
- [ ] `npm run contracts:check` passes with the committed generated client (after commit; generation re-run, no change).
- [x] `npm run typecheck`, `lint`, `test`, `architecture:structure`, `format:check` pass.
- [x] No browser/visual checks were run (none requested).
- [x] Theme tokens only, both schemes, phone breakpoints checked in code.
- [x] `traceability.md` points to real tests and evidence.
- [x] Unfinished checks have a reason and owner.
- [x] Residual risks recorded (AF-001 duplication with Expenses).
- [x] Unrelated working tree changes kept.
