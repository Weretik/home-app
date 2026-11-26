# 003 — History by month

- **App type:** React Web (Vite, MUI, React Query)
- **App:** `web/`, feature `src/features/shopping/history-page`
- **Architecture source:** [architecture](../../../architecture.md), [rules](../../../rules.md),
  [design rules](../../../design.md), feature [001](../001-shopping-list/README.md) (R-10 history, R-15 states)
- **Status:** implemented, audited
- **Owner:** Vitaliy · **Created:** 2026-11-26 · **Updated:** 2026-11-26
- **Backend SDD:** none: the existing `listShoppingHistory` (newest bought first, paged) is enough

The «История» tab groups its days under months, so a long history reads like a diary: «Ноябрь 2026 · 38 покупок»,
then the day cards of November, then «Октябрь 2026 …». Requested together with
[002 — name suggestions](../002-name-suggestions/README.md). Written from the frontend SDD template, React variant,
condensed.

## State

- Specification: accepted · Implementation: done · Verification: local checks green · Audit:
  [code-audit/audit.md](code-audit/audit.md) · Blockers: none

## Requirements and behaviour

| ID     | Behaviour                                                                                                                                                                                                                                                                                                                           |
| ------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| FR-301 | Loaded history rows are grouped by the local month they were bought, newest month first. Each month is a section with a heading «Ноябрь 2026 · 38 покупок» and the existing day cards of that month inside (newest day first, the API's order inside a day).                                                                        |
| FR-302 | The count is the number of loaded rows in that month, with the Russian plural («1 покупка», «3 покупки», «38 покупок»). When more pages exist, the last loaded month may continue on the next page: its count reads «50+ покупок» until «Показать ещё» loads the rest; the month and its day continue in place (no second heading). |
| FR-303 | «Показать ещё», «Вернуть в список», delete, the empty, loading and error states work as before (R-10, R-15). Rows without a purchase time (should not happen) form a last section «Без даты».                                                                                                                                       |
| FR-304 | Headings nest: the page title `h1`, months `h2`, days `h3`. Every month section and day card is a named region. Phone first, both colour schemes, theme tokens only.                                                                                                                                                                |

### Decision: counts from the loaded rows, no API change

An exact count per month would need a new API operation (or a field on the history page) and a decision on the time
zone of a month on the server, which has none today: months are local to the browser like the day groups. The page
loads 50 rows at a time and a month of the household is usually 50–120 purchases, so the «N+» form (FR-302) is honest
and the user sees the exact number after one or two «Показать ещё». If an exact count becomes wanted, it is a backend
feature of its own.

## Design

- `history-page/history-days.ts` becomes `history-groups.ts` (pure, under 200 lines): `groupByDay` and `dayLabel`
  stay, `groupByMonth(items, now, mayContinue)` returns `{ key: '2026-11', label: 'Ноябрь 2026', count, partial,
days }[]`, built from the day groups (a day is always in one month); `monthHeading(month)` gives
  «Ноябрь 2026 · 38 покупок» / «… · 50+ покупок». `mayContinue` is `history.hasNextPage`; only the last month is
  `partial`.
- `HistoryMonth.tsx`: a `section` named by its heading (`h6` as `h2`, Exo 2 like every heading, the count in
  tabular figures) above the month's `HistoryDay` cards with the page's gap. `HistoryDay`'s title becomes `h3`.
- `HistoryPage.tsx` maps months instead of days. The folder stays at six production files (`HistoryDay`,
  `HistoryMonth`, `HistoryPage`, `history-api.ts`, `history-groups.ts`, `use-return-item.ts`).

## Tasks and traceability

| ID     | Responsibility                              | Test                                                                          | Covers         |
| ------ | ------------------------------------------- | ----------------------------------------------------------------------------- | -------------- |
| TS-301 | `groupByMonth`, `monthHeading`, plural      | Vitest `history-groups.test.ts` (month split, order, partial, «Без даты»)     | FR-301, FR-302 |
| TS-302 | `HistoryMonth`, page wiring, heading levels | Testing Library `HistoryPage.test.tsx` (months, «Показать ещё» across months) | FR-301…FR-304  |
| TS-303 | Docs: `architecture.md`, specs index        | review, `format:check`                                                        | —              |

## Readiness

- [x] Behaviour and the count decision recorded; no API change, no ADR (a presentation choice inside a page).
- [x] Delivery: structure, format, lint, typecheck, tests and build green locally.
- [x] Code audit: [code-audit/audit.md](code-audit/audit.md).
