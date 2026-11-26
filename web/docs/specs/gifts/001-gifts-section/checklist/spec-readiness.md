# Specification readiness

- [x] Goal and in/out of scope agreed (from the product brief; [overview](../requirements/overview.md)).
- [x] One app variant chosen: React Web.
- [x] Type was given in the request (React 19 + Vite + MUI).
- [x] Business rules have stable `R-001…R-012`.
- [x] Scenarios have stable `SC-001…SC-023` in Given/When/Then.
- [x] Scenarios describe only observable behaviour.
- [x] Actual modules, scripts, tools and existing tests checked (`package.json`, `tools/check-structure.mjs`,
      Expenses and home tests).
- [x] Architecture baseline lists dependency rules, owners, public entry points, data stack and the new size rule.
- [x] `frontend.md` separates the audit of existing files from the target design.
- [x] The planned map names key paths, one role each, and planned/existing state; split seams for large files.
- [x] Theme source, tokens and components checked; light and dark covered.
- [x] Phone and computer behaviour defined (no separate tablet layout in the project).
- [x] No new packages; approach decisions recorded (none new), AF-001 recorded.
- [x] Every architecture mismatch has an id (AF-001, accepted without remediation, reason recorded).
- [x] Test levels chosen by risk; no E2E (no stand), reason recorded.
- [x] Each `TS-*` has one responsibility; the shared prerequisite is `EN-001`.
- [x] Tasks have coverage, dependencies, exact paths, test level, work, evidence and done condition.
- [x] Every task ends with the responsibility and size check.
- [x] `traceability.md` has no orphan scenarios or tasks.
- [x] Open product questions: none; assumptions listed in [frontend.md](../design/frontend.md#assumptions).
- [x] Every operation has a confirmed `operationId` (generated client, EN-001); provider revision: the uncommitted backend working tree of 2026-11-26.
- [x] `data-model.md` added (presentation and form models only).
- [x] Unused phase files removed.
