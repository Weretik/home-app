# Budget forecast and carry-over — task graph

## Identifiers

- `R-*`: business rule in `requirements/` (prefixes `R-OL` outside limits, `R-CO` carry-over, `R-FC` forecast);
- `SC-*`: acceptance scenario in `requirements/`;
- `TS-*`: one implementation or verification responsibility;
- `EN-*`: one shared prerequisite.

## Planning phases

The phases only group tasks; work follows `Depends on`, scenario by scenario.

- 00 — Readiness: this SDD, [traceability](../traceability.md), [spec readiness](../checklist/spec-readiness.md). Rate
  limits: every new write uses the existing `PublicWrite` policy (anonymous writes, ADR 0012); the list read needs none.
- 01 — Domain: TS-001 … TS-004.
- 02 — Infrastructure: EN-001.
- 03 — Application: TS-005 … TS-012.
- 04 — API and contract: TS-013.
- 05 — Verification and delivery: TS-014 … TS-016.

## Mandatory cycle for behavior tasks

Write the smallest focused test → run it and see the behavioral Red → implement the minimum for Green → run again →
refactor keeping Green → run the affected regression suite → record Red, Green, refactor and regression evidence. A
compile error or broken fixture is not Red. `EN-001` may skip Red (nothing to test before the schema exists); its
replacement check is in the task.

## Task index

| ID | Responsibility | Covers | Depends on | File | Status |
| --- | --- | --- | --- | --- | --- |
| EN-001 | Migration ForecastAndCarryOver and the plan mapping | SC-OL-001, SC-CO-007, SC-FC-001 | none | [EN-001](infrastructure/EN-001-migration.md) | planned |
| TS-001 | `Expense.OutsideLimits` on create and update | SC-OL-001, SC-OL-002 | none | [TS-001](domain/TS-001-expense-outside-limits.md) | planned |
| TS-002 | `BudgetSettings.LimitsFrom` and its range rule | SC-CO-007, SC-CO-008 | none | [TS-002](domain/TS-002-limits-from-setting.md) | planned |
| TS-003 | `LimitCarryOver`: carry arithmetic by month and quarter | SC-CO-001 … SC-CO-006 | none | [TS-003](domain/TS-003-limit-carry-over.md) | planned |
| TS-004 | `PlannedExpense` aggregate | SC-FC-001, SC-FC-003 | none | [TS-004](domain/TS-004-planned-expense.md) | planned |
| TS-005 | Create, update and list expenses with `outsideLimits` | SC-OL-001, SC-OL-002, SC-OL-005 | TS-001, EN-001 | [TS-005](application/TS-005-expense-flag-use-cases.md) | planned |
| TS-006 | Read and change `limitsFrom` | SC-CO-007, SC-CO-008 | TS-002, EN-001 | [TS-006](application/TS-006-limits-from-settings.md) | planned |
| TS-007 | `ListPlannedExpenses` | SC-FC-001 | TS-004, EN-001 | [TS-007](application/TS-007-list-planned.md) | planned |
| TS-008 | `CreatePlannedExpense` with `Idempotency-Key` | SC-FC-001, SC-FC-002, SC-FC-003 | TS-004, EN-001 | [TS-008](application/TS-008-create-planned.md) | planned |
| TS-009 | `UpdatePlannedExpense` with `If-Match` | SC-FC-004 | TS-004, EN-001 | [TS-009](application/TS-009-update-planned.md) | planned |
| TS-010 | `DeletePlannedExpense` with `If-Match` | SC-FC-004 | TS-004, EN-001 | [TS-010](application/TS-010-delete-planned.md) | planned |
| TS-011 | `RecordPlannedExpense`: plan → expense in one transaction | SC-FC-005, SC-FC-006 | TS-001, TS-004, EN-001 | [TS-011](application/TS-011-record-planned.md) | planned |
| TS-012 | Month summary: carry-over, outside limits, planned, forecast | SC-OL-003, SC-OL-004, SC-CO-001 … SC-CO-006, SC-CO-009, SC-FC-007, SC-FC-008 | TS-003, TS-005, TS-007, EN-001 | [TS-012](application/TS-012-month-summary.md) | planned |
| TS-013 | Plan endpoints, request fields, regenerated OpenAPI | all SC-FC-*, SC-OL-001, SC-CO-007 | TS-005 … TS-012 | [TS-013](api/TS-013-endpoints-and-contract.md) | planned |
| TS-014 | Integration test suite for the feature | all SC-* | TS-013 | [TS-014](verification/TS-014-api-tests.md) | planned |
| TS-015 | Module page, ADR and indexes | — | TS-013 | [TS-015](verification/TS-015-docs-and-adr.md) | planned |
| TS-016 | Final verification and audit | all | TS-014, TS-015 | [TS-016](verification/TS-016-final-verification.md) | planned |

Execution follows the `backend-feature-delivery` skill and [API README, add a use case](../../../../../../README.md#add-a-use-case).
