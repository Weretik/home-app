# Month summary — delivery readiness (retroactive check at 8378e4a)

- [x] Every `SC-*` row is verified (GAP-015 resolved 2026-11-26).
- [x] The read model is computed with a fixed number of grouped queries (no query per limit).
- [x] API matches the contract; OpenAPI generated and checked.
- [x] Documentation (module page "Summary rules", ADR 0020) agrees with the code.
- [ ] One source for every summary rule — the «ИТОГО» row is computed in the web (GAP-014).
