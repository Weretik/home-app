# Budget forecast and carry-over — delivery readiness

- [ ] Every `SC-*` row in `traceability.md` is verified or explicitly deferred.
- [ ] Domain invariants (`Expense`, `BudgetSettings`, `PlannedExpense`, `LimitCarryOver`) are covered by unit tests.
- [ ] Application orchestration, validation and result behavior are covered by tests.
- [ ] The migration, its rollback and the storage constraints are checked.
- [ ] The API matches the [contract](../contracts/api-contract.md), including errors; the generated `openapi.json` is committed.
- [ ] Every behavior `TS-*` has current Red, Green, Refactor and Regression evidence.
- [ ] Task records show the narrow Red test ran before production code; every exception is an `EN-*` with a replacement check.
- [ ] The narrowest build and test commands for the change ran (`dotnet build HomeApp.slnx`, unit and architecture tests; integration in CI).
- [ ] Documentation, contract, implementation and test names agree (module page, ADR, this SDD).
- [ ] New and changed files have one role and the right folder (≤ 6 code files per folder); the planned map matches the tree.
- [ ] The module page and the spec index are updated; no new container, so C4 is checked and unchanged.
- [ ] Every client input has a FluentValidation validator (plan create/update/delete/record, list, settings).
- [ ] A full audit against the project rules ran after the whole feature; findings fixed.
- [ ] All task ids are closed; every blocker has an owner and a next step.
