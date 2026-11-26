# Budget forecast and carry-over — specification readiness

- [x] Scope and observable behavior are complete and consistent.
- [x] Every changed business rule has a stable `R-*` id.
- [x] Every behavior in scope has a stable `SC-*` id.
- [x] Every rule has a success example and relevant negative or boundary examples.
- [x] Given/When/Then contain no implementation detail.
- [ ] Every `[NEEDS CLARIFICATION]` is resolved or moved out of scope — three open questions in [overview](../requirements/overview.md) (first quarter vs. LimitsFrom, planned vs. left, quarterly planned); the SDD states the assumed answer for each.
- [x] Every requirement and scenario has a design decision or is marked deferred.
- [x] The data model contains no assumed data or relations.
- [x] New identifiers follow the project strategy (UUID v7) with a reason.
- [x] Contract, security, idempotency and integration dependencies are agreed.
- [x] The human-readable contract is defined before the transport work; the machine contract is generated (`api/docs/contracts/openapi.json`).
- [x] `traceability.md` links every scenario to a test level and `TS-*`/`EN-*`; tasks stay inside the scope.
- [x] Every task has exact paths, dependencies, a checkpoint and planned Red/Green/Regression evidence, or a justified exception.
- [x] The planned map names key paths, one role and state each; the split decision for the summary handler is recorded.
- [x] Every behavior task orders the work Red → Green → Refactor → Regression → evidence.
