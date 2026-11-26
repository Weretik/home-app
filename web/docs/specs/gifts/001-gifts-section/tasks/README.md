# Gifts section — task graph

The phase files group small tasks; work follows the dependencies, not the phases. Every task is a section with the
fields of the TS/EN template (ids, coverage, dependencies, exact paths, test level, baseline, work, evidence, done
condition). Tests are written before the code they cover (Red → Green → Refactor → Regression,
[web rules, Tests](../../../../rules.md#tests-and-checks)).

Every `TS-*`/`EN-*` ends its work with the responsibility check: a changed file that mixes independent
responsibilities, or passes **150 lines (component) / 200 lines (hook or module)**, is split along the seams in
[frontend.md](../design/frontend.md#planned-feature-file-map), keeping every folder at 2–6 production files.

## Ids

- `TS-*` — one implementation or verification responsibility;
- `EN-*` — one shared prerequisite.

## Phases

- [00 — Readiness](00-readiness.md)
- [02 — State and data](02-state-and-data.md)
- [03 — React Web UI](03-react-web-ui.md)
- [06 — Navigation and platform](06-navigation-and-platform.md)
- [07 — API integration](07-api-integration.md)
- [08 — Verification](08-verification.md)

(01 shared behaviour, 04 React Native, 05 Angular: not applicable, one web client.)

## Task index

| ID     | Responsibility                                                           | Covers / enables                       | Depends on                     | File                                                                               | Status |
| ------ | ------------------------------------------------------------------------ | -------------------------------------- | ------------------------------ | ---------------------------------------------------------------------------------- | ------ |
| EN-001 | confirm the Gifts contract, regenerate the client, fix names in this SDD | all                                    | backend Gifts endpoints        | [00](00-readiness.md#en-001--confirm-the-gifts-contract-and-regenerate-the-client) | done   |
| TS-001 | labels and formatting                                                    | SC-001, SC-016, SC-017                 | —                              | [02](02-state-and-data.md#ts-001--labels-and-formatting)                           | done   |
| TS-002 | reads and query keys                                                     | SC-001, SC-006, SC-016, SC-019, SC-020 | EN-001                         | [07](07-api-integration.md#ts-002--reads-and-query-keys)                           | done   |
| TS-003 | writes, invalidation, idempotency, error text                            | SC-008, SC-011, SC-020, SC-023         | EN-001                         | [07](07-api-integration.md#ts-003--writes-invalidation-idempotency-error-text)     | done   |
| TS-004 | filters in the URL, filtering and grouping                               | SC-002, SC-003, SC-004                 | TS-001                         | [02](02-state-and-data.md#ts-004--filters-in-the-url-filtering-and-grouping)       | done   |
| TS-005 | editing frame and field checks                                           | SC-009, SC-012                         | TS-003                         | [03](03-react-web-ui.md#ts-005--editing-frame-and-field-checks)                    | done   |
| TS-006 | section frame, tabs, routes                                              | SC-018                                 | —                              | [06](06-navigation-and-platform.md#ts-006--section-frame-tabs-routes)              | done   |
| TS-007 | gift card                                                                | SC-001, SC-014, SC-016, SC-017         | TS-001                         | [03](03-react-web-ui.md#ts-007--gift-card)                                         | done   |
| TS-008 | «Список» page                                                            | SC-001…SC-006                          | TS-002, TS-004, TS-006, TS-007 | [03](03-react-web-ui.md#ts-008--список-page)                                       | done   |
| TS-009 | gift add / edit / delete with variants                                   | SC-007…SC-012                          | TS-003, TS-005, TS-008         | [03](03-react-web-ui.md#ts-009--gift-add--edit--delete-with-variants)              | done   |
| TS-010 | quick status actions                                                     | SC-013, SC-014, SC-023                 | TS-003, TS-008                 | [03](03-react-web-ui.md#ts-010--quick-status-actions)                              | done   |
| TS-011 | «Подаренные» page                                                        | SC-015, SC-016                         | TS-008, TS-010                 | [03](03-react-web-ui.md#ts-011--подаренные-page)                                   | done   |
| TS-012 | settings: «Нюансы»                                                       | SC-019                                 | TS-002, TS-003, TS-005, TS-006 | [03](03-react-web-ui.md#ts-012--settings-нюансы)                                   | done   |
| TS-013 | settings: categories                                                     | SC-020                                 | TS-012                         | [03](03-react-web-ui.md#ts-013--settings-categories)                               | done   |
| TS-014 | home Gifts summary                                                       | SC-021, SC-022                         | EN-001                         | [06](06-navigation-and-platform.md#ts-014--home-gifts-summary)                     | done   |
| TS-015 | docs: architecture, design, specs index                                  | all                                    | TS-006…TS-014                  | [08](08-verification.md#ts-015--docs-architecture-design-specs-index)              | done   |
| TS-016 | verification and final audit of the feature                              | all                                    | all                            | [08](08-verification.md#ts-016--verification-and-final-feature-audit)              | done   |
