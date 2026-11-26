# 005 — Budget: forecast, carry-over, expenses outside limits (web)

- **Application type:** React Web (Vite SPA, MUI, React Query)
- **Application:** `web/`
- **Project architecture source:** [web architecture](../../../architecture.md), [web rules](../../../rules.md), [design rules](../../../design.md)
- **Domain:** expenses (section «Бюджет»), home summary
- **Status:** accepted for implementation (requirements note of 2026-11-26)
- **Owner:** Vitaliy
- **Created:** 2026-11-26
- **Updated:** 2026-11-26
- **Backend counterpart:** [api 005](../../../../../api/docs/specs/expenses/005-budget-forecast-carryover/README.md)

The «Расходы» section becomes «Бюджет»; a new «Прогноз» tab lists the month's planned expenses and turns one into a
real expense; the expense dialog gets «Не в лимиты»; the home summary shows carry-over per limit, the outside-limits
line and a «Прогноз» card. Everything reads and writes through the generated client; no business arithmetic of
carry-over or forecast happens in the browser.

## State

- Specification: accepted
- Implementation: done in the working tree (reported by the implementing agent on 2026-11-26), not yet verified against this SDD
- Verification: not started
- Blockers: none; backend contract must be regenerated first (EN-001)

## Requirements

- [Scope and outcome](requirements/overview.md)
- [Rules and scenarios](requirements/behavior.md)

## Technical context

- [Frontend design](design/frontend.md)
- [React Web design](design/react.md)
- [API contract](contracts/api-contract.md)

## Planning and delivery

- [Traceability](traceability.md)
- [Task graph](tasks/README.md)
- [Specification readiness](checklist/spec-readiness.md)
- [Code audit of 2026-11-26](code-audit/audit.md)
- [React readiness](checklist/react-readiness.md)
- [Delivery readiness](checklist/delivery-readiness.md)

## Change history

- 2026-11-26 — created from the phase 4 requirements note while the implementation was in progress; the file map
  follows the paths in the working tree.
