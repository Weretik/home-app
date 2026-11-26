# Budget settings and spending limits — delivery readiness (retroactive check at 8378e4a)

- [x] Every `SC-*` row is verified by an existing test.
- [x] Domain invariants are covered (`SpendingLimitTests`, `BudgetSettingsTests`).
- [x] Validators are unit-tested (`BudgetInputValidatorTests`); handlers only through integration tests (001 GAP-002).
- [x] Migration and seed are exercised by the integration host.
- [x] API matches the contract; OpenAPI generated and checked.
- [x] Every piece of shared state has a named race guard — `SortOrder`: unique index (GAP-009 resolved 2026-11-26).
- [x] Documentation (module page, ADR 0019) agrees with the code at `8378e4a`.
