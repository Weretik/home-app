# Budget forecast and carry-over (web) — task graph

Phases only group the tasks; work follows `Depends on`.

## Identifiers

- `TS-*` — one implementation or verification responsibility;
- `EN-*` — one shared prerequisite.

Rules and tools come from `AGENTS.md`, [web architecture](../../../../../architecture.md), [web rules](../../../../../rules.md)
and the actual configuration (`web/package.json`).

## Planning phases

- 00 — Readiness: this SDD and [traceability](../traceability.md).
- 02 — State and data: TS-003.
- 03 — React Web UI: TS-002, TS-004, TS-005, TS-006, TS-007, TS-008, TS-009.
- 06 — Navigation and platform: TS-001 (name), route part of TS-006.
- 07 — API integration: EN-001.
- 08 — Verification: TS-010.

Phases 01 (shared behavior), 04 (React Native) and 05 (Angular) do not apply.

## Task index

| ID     | Responsibility                                        | Covers                           | Depends on               | File                                             | Status  |
| ------ | ----------------------------------------------------- | -------------------------------- | ------------------------ | ------------------------------------------------ | ------- |
| EN-001 | Regenerate the typed client from the new contract     | SC-W5-002 … SC-W5-009            | backend TS-013 (api 005) | [EN-001](enablers/EN-001-regenerate-client.md)   | planned |
| TS-001 | Rename the section to «Бюджет»                        | SC-W5-001                        | none                     | [TS-001](shell/TS-001-section-name.md)           | planned |
| TS-002 | «Не в лимиты» in the expense dialog and the list chip | SC-W5-006                        | EN-001                   | [TS-002](entries/TS-002-outside-limits-field.md) | planned |
| TS-003 | Plan reads and writes; shared write hooks             | SC-W5-002 … SC-W5-005            | EN-001                   | [TS-003](forecast/TS-003-planned-data.md)        | planned |
| TS-004 | Plan form and add / edit / delete dialogs             | SC-W5-003, SC-W5-004             | TS-003                   | [TS-004](forecast/TS-004-plan-dialogs.md)        | planned |
| TS-005 | «Записать как расход» dialog                          | SC-W5-005                        | TS-003                   | [TS-005](forecast/TS-005-record-dialog.md)       | planned |
| TS-006 | «Прогноз» tab and route                               | SC-W5-002 … SC-W5-005, SC-W5-010 | TS-004, TS-005           | [TS-006](forecast/TS-006-forecast-page.md)       | planned |
| TS-007 | Carry-over and plan notes on the limit rows           | SC-W5-007                        | EN-001                   | [TS-007](home/TS-007-limit-notes.md)             | planned |
| TS-008 | «Вне лимитов» KPI line                                | SC-W5-008                        | EN-001                   | [TS-008](home/TS-008-outside-limits-line.md)     | planned |
| TS-009 | «Прогноз» card on the home summary                    | SC-W5-009                        | EN-001                   | [TS-009](home/TS-009-forecast-card.md)           | planned |
| TS-010 | Phone layout and final checks                         | SC-W5-010, all                   | TS-001 … TS-009          | [TS-010](verification/TS-010-verification.md)    | planned |
