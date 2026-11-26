# 002 — Savings jar page «Копилка» and the savings goal card (web)

- **Application type:** React Web (Vite SPA, MUI, React Query)
- **Application:** `web/`
- **Project architecture source:** [web architecture](../../../../architecture.md), [web rules](../../../../rules.md), [design rules](../../../../design.md)
- **Domain:** expenses
- **Status:** **retroactive, describes implemented behavior** — implemented in PR #8 `12ba66c`, goal card in PR #9 `b6fb5d7`
- **Owner:** Vitaliy
- **Created:** 2026-11-26
- **Updated:** 2026-11-26
- **Backend counterpart:** [api 002](../../../../../../api/docs/sdd/specs/expenses/002-savings-jar/README.md)

The «Копилка» tab: the jar's balance now, the open month's start / in / out / end, its movements (signed and
coloured) with add / edit / delete, the months overview, and the «План копилки» card comparing the balance with the
savings goal from the budget.

This SDD was written after the code, from the web code at `8378e4a`. It gives the behavior stable `R-*`/`SC-*` ids
for later changes and audits and changes nothing. Gaps between code and rules are open questions in
[overview](requirements/overview.md).

## State

- Specification: retroactive
- Implementation: done
- Verification: existing tests listed in [traceability](traceability.md)
- Blockers: none

## Requirements

- [Scope and outcome](requirements/overview.md)
- [Rules and scenarios](requirements/behavior.md)

## Technical context

- [Frontend design](design/frontend.md)
- [React Web design](design/react.md)
- [API contract](contracts/api-contract.md)

## Planning and delivery

- [Traceability](traceability.md)
- [Delivery readiness](checklist/delivery-readiness.md)

No task graph: the work is done. A later change adds `TS-*` tasks here by the incremental migration rules of the
frontend SDD template, keeping these ids.

## Change history

- 2026-11-26 — retroactive SDD created from the code at `8378e4a`.
