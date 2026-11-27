# 001 — Ukrainian interface

- **App type:** React Web (Vite, MUI, React Query, React Router)
- **App:** `web/`, every feature (`src/features/*`) and the shell (`src/app`)
- **Architecture source:** [architecture](../../../architecture.md), [rules](../../../rules.md), [design rules](../../../design.md)
- **Status:** accepted for delivery
- **Owner:** Vitaliy · **Created:** 2026-11-27 · **Updated:** 2026-11-27
- **Backend SDD:** [api/docs/specs/platform/001-ukrainian-texts](../../../../../api/docs/specs/platform/001-ukrainian-texts/README.md)
  (problem messages, bot messages and the schedule titles the page shows)
- **Decision:** [ADR 0014](../../../decisions/0014-ukrainian-interface.md) · **Words:** [glossary](glossary.md)

Vitaliy, 2026-11-27: the whole interface in Ukrainian. The site's own words change from Russian to Ukrainian; what the
family typed (categories, departments, item names, notes, gift titles) stays as it is. One language, no switcher.
Written from the frontend SDD template (Weretik/obsidian `Templates/frontend-sdd`), React variant, condensed.

## State

- Specification: accepted · Implementation: see the PR · Verification: local checks, audit in [audit.md](audit.md) ·
  Blockers: none

## Requirements and behaviour

| ID     | Behaviour                                                                                                                                                                                                                                                                                                                                               |
| ------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| FR-001 | Every text the web app writes itself is Ukrainian: shell (menus, top bar, theme menu), page titles, tabs, buttons, form labels, placeholders, helper and validation texts, dialogs, snackbars, empty, loading and error states, `aria-label`s and tooltips, in every section (Головна, Бюджет, Скарбничка, Подарунки, Покупки, Справи, Телеграм, Стан). |
| FR-002 | The words follow the [glossary](glossary.md), the same in every section.                                                                                                                                                                                                                                                                                |
| FR-003 | Dates, months, weekdays, numbers, money and plural forms are formatted for `uk-UA` (`Intl.DateTimeFormat`, `NumberFormat`, `PluralRules`, `localeCompare`). The locale lives in one constant, `src/platform/locale.ts`; a structure rule (`oneLocale`) fails on any other locale literal.                                                               |
| FR-004 | `index.html` declares `lang="uk"`; the document title is Ukrainian.                                                                                                                                                                                                                                                                                     |
| FR-005 | Data from the API is shown as it comes: the family's categories, departments and items are not translated. Texts the API writes (problem messages, the bot's schedule titles) come in Ukrainian from the API ([api 001](../../../../../api/docs/specs/platform/001-ukrainian-texts/README.md)).                                                         |
| FR-006 | Russian input still works where the site parses text (the shopping quick add sends the text to the API as before).                                                                                                                                                                                                                                      |
| FR-007 | Nothing else changes: routes, URL state, layout, colours, behaviour.                                                                                                                                                                                                                                                                                    |

## Design

- Strings stay where they are (in the components and the `labels.ts` modules), replaced in place. No i18n library and
  no string catalogue: one language, two users ([ADR 0014](../../../decisions/0014-ukrainian-interface.md)).
- `src/platform/locale.ts` exports `locale = 'uk-UA'`; every feature imports it from `src/platform` (allowed import
  direction), never from another feature.
- Tests query the Ukrainian names; their expectations change with the texts.

## Delivery

Split by feature for parallel work, each part runs the web checks: shell and status, home, expenses, gifts, shopping,
to-dos and notifications, then one pass over the whole tree for words left in Russian.

## Tests and checks

| ID     | Check                                                                                                                                                                                                                      |
| ------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| TS-001 | Every page test and logic test passes with the Ukrainian texts (`npm test`).                                                                                                                                               |
| TS-002 | `oneLocale` structure rule has its own test in `tools/architecture/rules.test.mjs` and passes on the tree.                                                                                                                 |
| TS-003 | No Cyrillic string with Russian-only letters (ы, э, ъ, ё) is left in `src/` outside generated code and test fixtures of family data: `grep -rnP "[ыэъёЫЭЪЁ]" src --include=*.ts --include=*.tsx` lists only data fixtures. |
| TS-004 | typecheck, lint, format, structure and build pass in CI.                                                                                                                                                                   |
