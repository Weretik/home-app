# Budget forecast and carry-over — scenario traceability

One row per acceptance scenario. Status: `planned` until the acceptance test passes and its evidence is recorded in
the task, then `verified`.

| Scenario | Rules | Test level | Domain | Application | Infrastructure | API/contract | Acceptance evidence | Status |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| SC-OL-001 | R-OL-001 | domain + integration | TS-001 | TS-005 | EN-001 | TS-013 | an expense created with `outsideLimits: true` is listed with it | planned |
| SC-OL-002 | R-OL-001 | domain + integration | TS-001 | TS-005 | EN-001 | TS-013 | default false; edit sets it | planned |
| SC-OL-003 | R-OL-002, R-OL-003 | integration | — | TS-012 | EN-001 | TS-013 | November: limit spent 6 000, month 7 000, count 1 / 1 000 | planned |
| SC-OL-004 | R-OL-002 | integration | TS-003 | TS-012 | EN-001 | TS-013 | December carriedIn 0 despite 5 000 outside limits | planned |
| SC-OL-005 | R-OL-004 | integration | — | TS-005 | — | TS-013 | same key, other flag → 409 | planned |
| SC-CO-001 | R-CO-001, R-CO-003 | domain + integration | TS-003 | TS-012 | — | TS-013 | 8 000 / 5 000 / 0 table | planned |
| SC-CO-002 | R-CO-001 | domain | TS-003 | TS-012 | — | — | January carriedIn 3 000 | planned |
| SC-CO-003 | R-CO-001 | domain | TS-003 | — | — | — | December carriedIn 0 after 5 000 | planned |
| SC-CO-004 | R-CO-003, R-CO-008 | domain + integration | TS-003 | TS-012 | — | — | October ignored; `The_MonthSummary_AddsUp_LikeTheSheet` green | planned |
| SC-CO-005 | R-CO-004 | domain + integration | TS-003 | TS-012 | — | — | quarter carries 500 into Q1 2027 | planned |
| SC-CO-006 | R-CO-006 | domain + integration | TS-003 | TS-012 | — | TS-013 | total carriesOut 2 000 → December carriedIn 2 000 | planned |
| SC-CO-007 | R-CO-007 | domain + integration | TS-002 | TS-006 | EN-001 | TS-013 | budget shows 2026-11-01; save, keep, 409 | planned |
| SC-CO-008 | R-CO-007 | validator unit + domain | TS-002 | TS-006 | — | — | out-of-range → 400 | planned |
| SC-CO-009 | R-CO-001 | integration (fixed clock) | — | TS-012 | — | — | perDay 90.91 | planned |
| SC-FC-001 | R-FC-001 … 003 | integration | TS-004 | TS-007, TS-008 | EN-001 | TS-013 | two plans by name, total 5 000 | planned |
| SC-FC-002 | R-FC-003 | integration | — | TS-008 | — | TS-013 | repeat → same id; other data → 409 | planned |
| SC-FC-003 | R-FC-001 | domain + integration | TS-004 | TS-008 | — | TS-013 | 400 / 404 cases | planned |
| SC-FC-004 | R-FC-003 | integration | — | TS-009, TS-010 | — | TS-013 | 204, 409 on old version, 404 | planned |
| SC-FC-005 | R-FC-004 | integration | — | TS-011 | — | TS-013 | expense created, plan gone, repeat → 404 | planned |
| SC-FC-006 | R-FC-004 | integration | — | TS-011 | — | TS-013 | stale record → 409, nothing changes | planned |
| SC-FC-007 | R-FC-005 | integration | — | TS-012 | — | TS-013 | forecast 5 000 / 35 000 / 35 000 / 15 000 | planned |
| SC-FC-008 | R-FC-006 | integration | — | TS-012 | — | TS-013 | planned 3 000 and quarterly 1 200 | planned |

## Shared prerequisites

| Task | For scenarios | Reason | Check |
| --- | --- | --- | --- |
| EN-001 | every scenario that stores data | new columns and table | migration applies in the integration host |

## Notes and justified gaps

- TS-014 gathers the integration tests; TS-015 and TS-016 are documentation and verification, not scenario owners.
- SC-CO-009 needs a fixed `TimeProvider` in the integration host; if the host has none, test it on the handler with
  a fake clock instead.
