# 004 — Home page expenses summary (web)

- **Application type:** React Web (Vite SPA, MUI, React Query)
- **Application:** `web/`
- **Project architecture source:** [web architecture](../../../architecture.md), [web rules](../../../rules.md), [design rules](../../../design.md)
- **Domain:** expenses
- **Status:** **retroactive, describes implemented behavior** — implemented in PR #9 `b6fb5d7`, phone layout fix in PR #10 `8378e4a`
- **Owner:** Vitaliy
- **Created:** 2026-11-26
- **Updated:** 2026-11-26
- **Backend counterpart:** [api 004](../../../../../api/docs/specs/expenses/004-month-summary/README.md)

The home page shows one section summary at a time, picked with a switcher (`?s=`); the expenses summary shows a
month (`?year=&month=`): four figure cards (Доходы, Расходы against the monthly limit, Итог месяца, Копилка against
the goal), «Лимиты месяца» with «ИТОГО» and per-day amounts, «Кто сколько потратил» and «Топ-5 категорий»
([web ADR 0008](../../../decisions/0008-home-page-as-a-registry-of-section-summaries.md)).

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
