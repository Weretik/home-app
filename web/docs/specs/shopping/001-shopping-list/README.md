# 001 — Shopping list («Покупки»)

- **Client type:** React Web (private SPA, `react` variant)
- **App:** `web/` (Vite, React 19, MUI, React Query, React Router)
- **Project architecture:** [web/docs/architecture.md](../../../architecture.md), [rules](../../../rules.md),
  [design](../../../design.md), [decisions](../../../decisions/README.md)
- **Domain:** shopping
- **Product brief:** `/mnt/project-files/home-app/specs/shopping-module.md` (scope, UX, endpoint names)
- **Status:** accepted, implemented
- **Owner:** Виталий
- **Created / updated:** 2026-11-26

The shared shopping list of two people replaces the Google Sheet «🛒 Список покупок» and its Telegram bot: quick add
from text, a list grouped by store department with big checkboxes that both phones see within 20 seconds, the cart and
its history, frequent items, and the department dictionary moved into settings. The home page gets a «Покупки»
summary. No sign-in: «Я: Виталий / Маша» is chosen once per device.

## State

- Specification: accepted
- Implementation: done (see [tasks](tasks/README.md))
- Verification: see [delivery readiness](checklist/delivery-readiness.md) and [code audit](code-audit/audit.md)
- Blockers: none

## Documents

- [Scope and outcome](requirements/overview.md), [rules and scenarios](requirements/behavior.md)
- [Frontend design](design/frontend.md), [React design](design/react.md), [API contract](contracts/api-contract.md)
- [Traceability](traceability.md), [tasks](tasks/README.md)
- [Spec readiness](checklist/spec-readiness.md), [delivery readiness](checklist/delivery-readiness.md)

## History

- 2026-11-26 — created from the product brief; implemented in the same change.
