# 001 — Gifts section («Подарки»)

- **App type:** React Web (variant `react`)
- **App / client:** `web/` (one Vite React SPA, [web ADR 0001](../../../decisions/0001-single-vite-app-without-nx.md))
- **Project architecture source:** [web/docs/architecture.md](../../../architecture.md),
  [web/docs/rules.md](../../../rules.md), [web/docs/design.md](../../../design.md),
  [web/docs/decisions](../../../decisions/README.md), root [AGENTS.md](../../../../../AGENTS.md)
- **Domain:** gifts (backend module `Gifts`, schema `gifts`, built in parallel)
- **Status:** draft
- **Owner:** Vitaliy (product), web half of the Gifts feature
- **Created:** 2026-11-26
- **Updated:** 2026-11-26

Masha and Vitaliy keep a gift wishlist in the Google Sheet «🎁 Список подарков». This feature replaces it with a
«Подарки» section: the active wishlist («Список») as a table in a coloured block per person with recipient and category filters, an archive
(«Подаренные») where a gifted or dropped wish moves instead of being greyed out, and settings (the «нюансы» — ring
size, clothes, shoes — and the categories). A gift has a recipient, name, category, priority, an optional price and
link, any number of variants each with its own link, an occasion with a date, and notes; it is added, edited and
deleted on the site. The home page gets a read-only Gifts summary in its switcher, including the «нюансы». The import
of the sheet's rows is a separate SQL script (not part of this feature); the backend endpoints are built by the API
half and consumed here.

## State

- Specification: draft, ready for implementation once [EN-001](tasks/00-readiness.md#en-001--confirm-the-gifts-contract-and-regenerate-the-client) confirms the contract
- Implementation: in progress — contract-independent batch done (TS-001, TS-004, TS-006, TS-007, part of TS-005)
- Verification: not started
- Blockers: the Gifts HTTP contract must be in `api/docs/contracts/openapi.json` before `npm run contracts:generate`
  (EN-001); the backend agent is building it now.

## Requirements

- [Scope and outcome](requirements/overview.md)
- [Business rules and scenarios](requirements/behavior.md)

## Technical context

- [Frontend design](design/frontend.md)
- [React Web design](design/react.md)
- [Data model](data-model.md) (presentation model: form values, filters, groups)
- [API contract](contracts/api-contract.md)

## Planning and delivery

- [Traceability](traceability.md)
- [Task graph](tasks/README.md)
- [Specification readiness](checklist/spec-readiness.md)
- [React readiness](checklist/react-readiness.md)
- [Delivery readiness](checklist/delivery-readiness.md)
- [Final code audit](code-audit/audit.md)

## Deviations from the template (proportionate for a two-person home app)

- Tasks are written as sections of the phase files (`tasks/0N-*.md`) with every field of the TS/EN template,
  instead of one file per task; ids and evidence anchors stay stable.
- Phases 01 (shared behaviour across clients), 04 (React Native) and 05 (Angular) are left out: one web client.
- The template's Ukrainian headings are written in English, the language of this repository's docs (AGENTS.md).

## Change history

- 2026-11-26 — after the first review: the wish cards became a table (computer) and a compact list (phone) like
  Expenses, with a coloured block per person (Виталий primary, Маша secondary, Вместе info); whole prices without
  «,00»; audit pass 2 appended.
- 2026-11-26 — implemented; file map synced with the code, audit AF-001…AF-006 recorded.
- 2026-11-26 — first draft from the product brief; contract names assumed until the backend lands (EN-001).
