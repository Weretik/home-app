# Gifts — specification readiness

- [x] Scope and observable behaviour are complete and consistent ([overview](../requirements/overview.md)).
- [x] Every business rule has a stable `R-*` id (R-001…R-013).
- [x] Every in-scope behaviour has a stable `SC-*` id (SC-001…SC-011).
- [x] Each rule has a success example and relevant negative or boundary examples (SC-001 table, SC-004, SC-008).
- [x] Given/When/Then contain no implementation details.
- [x] No `[NEEDS CLARIFICATION]` left: open points were decided as assumptions A-1…A-9 (spec and implementation were
  assigned together; the owner asked not to stop for questions).
- [x] Every rule and scenario has a design decision ([domain](../design/domain.md), [infrastructure](../design/infrastructure.md)).
- [x] The data model holds only data from the brief ([data model](../data-model.md)).
- [x] Identifiers chosen with reasons (UUID v7; fixed ids for the seed and the import).
- [x] Contract, access (open, ADR 0015), idempotency and integrations (none) agreed ([contract](../contracts/api-contract.md)).
- [x] The human contract exists and the machine contract is generated into `api/docs/contracts/openapi.json`.
- [x] `traceability.md` links every scenario to test levels and tasks.
- [x] Every task names exact paths, dependencies and evidence ([tasks](../tasks/README.md)).
- [x] The planned map names key paths, one role each, and the split of `IntegrationTests/Api`.
- [~] Red before Green: recorded honestly in the task evidence; the domain tests were written together with the
  aggregates, and only TS-012 has a captured Red run.
