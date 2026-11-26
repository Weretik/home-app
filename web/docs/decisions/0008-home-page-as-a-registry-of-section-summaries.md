# 0008. The home page as a registry of per-section summaries

Status: accepted, 2026-11-26

## Context

The old Google Sheet had a «Сводка» tab: the month's incomes, expenses, result, savings jar, limits, who spent what
and the top categories. The Expenses pages show no totals (they are lists), and the owners asked for that summary on
the home page, with a switcher that later also shows a summary of to-dos, and then of every other section. The home
page so far was a dashboard of «Скоро» cards that linked to the sections.

## Decision

- The home page (`/`, `features/home`) shows one section's summary at a time, picked with a switcher (MUI
  `ToggleButtonGroup`) under the title. The open summary is in the URL (`?s=expenses`); a missing, unknown or not yet
  built key opens the first built summary. Switching keeps the other params.
- The summaries are a registry, `features/home/page/summary-sections.ts`: key, label, icon and the summary component,
  or `null` while a section has none. Sections without a summary stay in the switcher, disabled and marked «скоро», so
  the switcher shows what is coming. Adding a summary is its folder in `features/home` plus one line in the registry.
- A summary belongs to the home feature, not to its section's feature, because features never import each other. It
  reads its data through its own `*-api.ts` (the Expenses summary calls `GET /api/expenses/summary`) and duplicates
  the few labels and formatters it needs instead of importing them.
- A summary that shows a month uses the same `?year=&month=` params and month picker as the section pages
  ([ADR 0006](0006-section-pages-as-tabs-with-state-in-the-url.md)), so a link between them keeps the month.
- The Expenses summary reproduces the sheet's «Сводка» without its «План / Факт / Прогноз» block: four figure cards,
  the limits with an «ИТОГО» row against the monthly limit, who spent what and the top five categories.

## Consequences

- A link opens the same summary and month; the home page needs no state library.
- The section dashboards' «Скоро» highlight cards are gone; the home page links only to the service pages (for
  phones, which have no side menu), because the main sections are in the menus.
- Small label maps and formatters exist twice (in the section and in its summary); they are a few lines each and
  covered by both features' tests.
- Each summary loads only when it is open, so the home page makes one request, not one per section.
