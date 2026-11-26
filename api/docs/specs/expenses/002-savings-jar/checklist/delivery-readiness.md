# Savings jar — delivery readiness (retroactive check at 8378e4a)

- [x] Every `SC-*` row is verified (GAP-007 resolved 2026-11-26).
- [x] Domain invariants are covered (`SavingsMovementTests`).
- [x] Application validation is unit-tested (`EntryInputValidatorTests`); handlers in integration tests (001 GAP-002).
- [x] Storage and migration in place.
- [x] API matches the contract; OpenAPI generated and checked.
- [ ] Reads page in SQL — the months overview does not (GAP-006).
- [x] Documentation (module page, ADR 0018) agrees with the code.
- [x] Folder and validator rules hold (architecture tests).
