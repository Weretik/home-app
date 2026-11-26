# Gifts — delivery readiness

- [x] Every in-scope `SC-*` row in `traceability.md` is verified.
- [x] Domain invariants tested (`tests/UnitTests/Gifts/Domain`, 25 tests).
- [x] Validation tested (`tests/UnitTests/Gifts/Input`, 7 tests); handler orchestration through integration tests.
- [x] Storage constraints, migration and seed verified on PostgreSQL 16 (migration applied by the test host; the table
  shapes checked with `\d`). Rollback described in [infrastructure](../design/infrastructure.md).
- [x] The API matches the contract, including errors, `If-Match`, `Idempotency-Key` and `429`; `openapi.json`
  regenerated and compared with `HEAD` (Expenses unchanged).
- [~] Red/Green/Refactor/Regression evidence: Green and Regression recorded for every task; Red captured only for
  TS-012 (see the task evidence).
- [x] `EN-*` exceptions name their substitute check.
- [x] Narrowest checks run: build, unit, architecture, integration (36/36 on local PostgreSQL; Docker missing here, CI
  runs Testcontainers).
- [x] Documentation, contract, code and test names agree (module page, ADRs 0021–0026, C4, READMEs, AGENTS.md, deploy).
- [x] New files have coherent roles and folders; the planned map was compared with the tree (README file map).
- [x] Module page with diagram, tree and "Tests" section; specs index row; C4 and the architecture module table updated.
- [x] A FluentValidation validator for every command and query with input; architecture docs updated for the new module.
- [x] Full audit against the project's rules done ([code audit](../code-audit/audit.md)); confirmed findings fixed, the
  rest recorded with owner and decision.
- [x] Open items have an owner: AF-001 (quality targets, Vitaliy), the import run (coordinator).
