# Scenario traceability

| Scenario | Rules | Task | Test |
| --- | --- | --- | --- |
| SC-LA-001 | R-LA-002, R-LA-003, R-LA-007 | TS-002, TS-005, TS-007 | summary unit test; budget integration test |
| SC-LA-002 | R-LA-002 | TS-001 | `AmountHistoryTests` |
| SC-LA-003 | R-LA-002 | TS-001 | `AmountHistoryTests` |
| SC-LA-004 | R-LA-002 | TS-002 | `SpendingLimitTests` |
| SC-LA-005 | R-LA-004 | TS-002 | `SpendingLimitTests` |
| SC-LA-006 | R-LA-006 | TS-003 | `LimitCarryOverTests` |
| SC-LA-007 | R-LA-006 | TS-003 | `LimitCarryOverTests` |
| SC-LA-008 | R-LA-003 | TS-004 | validator unit test |
| SC-LA-009 | R-LA-001, R-LA-009 | EN-001 | migration SQL reviewed and run up/down by hand on PostgreSQL 17; integration tests run on the migrated schema; `AmountHistoryColumnTests` (stored shape) |
| SC-LA-010 | R-LA-002, R-LA-005 | TS-002 | `SpendingLimitTests`, `BudgetSettingsTests` (explicit day cancels a later step; no day keeps it) |
