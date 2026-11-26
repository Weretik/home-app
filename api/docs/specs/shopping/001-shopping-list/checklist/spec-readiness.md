# Shopping list — specification readiness

- [x] Scope and observable behaviour are complete and consistent with the product brief.
- [x] Every business rule has a stable `R-*` id (R-001..R-016).
- [x] Every behaviour in scope has a stable `SC-*` id (SC-001..SC-014).
- [x] Rules have success and negative or boundary examples (parser table, bounds of names, hours, days).
- [x] Given/When/Then hold no implementation details.
- [x] No `[NEEDS CLARIFICATION]` left; the paging deviation is recorded in the contract.
- [x] Every rule and scenario maps to a design decision.
- [x] The data model holds only data the brief names (plus `outcome` of create requests, needed for replay).
- [x] Identifiers chosen: UUID v7, seed UUID v5, fixed singleton id.
- [x] Contract, access (open, rate limited), idempotency and integrations (none) agreed.
- [x] The human contract is written before the endpoints; the machine contract is the generated `openapi.json`.
- [x] `traceability.md` links every scenario to tests and tasks.
- [x] Tasks name paths, dependencies, checks and evidence; `EN-*` exceptions name substitute checks.
- [x] The planned file map names paths, roles and states; no existing file needs splitting except the
      integration `Api/` folder (split by area).
- [x] Behaviour tasks order Red → Green → Refactor → Regression → evidence.
