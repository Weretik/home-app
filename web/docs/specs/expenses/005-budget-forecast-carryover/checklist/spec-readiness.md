# Budget forecast and carry-over (web) — specification readiness

- [x] Goal and in/out of scope agreed (requirements note of 2026-11-26).
- [x] One application variant chosen: React Web.
- [x] Business rules have stable `R-*`; scenarios have stable `SC-*` in Given/When/Then.
- [x] Scenarios describe observable behavior only.
- [x] Actual folders, scripts, tools and existing tests checked (`web/package.json`, `web/docs/*`).
- [x] Architecture baseline names dependency rules, owners, entry points, data stack and checks with sources.
- [x] `design/frontend.md` separates the audit of current code from the target design.
- [x] The planned map names key paths, roles and existing/planned state; split decisions recorded (`write-data/`, `limits/`).
- [x] Theme source, tokens and both schemes are named; phone (360 px) and computer layouts described.
- [x] No new package.
- [x] Test levels chosen by risk; no E2E duplication (no harness).
- [x] Every `TS-*` has one responsibility; the shared prerequisite is `EN-001`.
- [x] Tasks have covers, dependencies, exact paths, test level, work, evidence and a done condition.
- [x] `traceability.md` has no orphan scenarios or tasks.
- [ ] Open product questions have an owner — `limitsFrom` field and plan-delete `If-Match` (owner Vitaliy).
- [x] Every operation has a stable operationId and source; schemas are not copied.
- [x] No `data-model.md` (no client model beyond the contract).
