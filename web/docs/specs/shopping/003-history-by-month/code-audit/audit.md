# History by month — final frontend code audit

- **Date:** 2026-11-26 · **Kind:** final feature audit (skill `architecture:frontend-code-audit`, feature review list)
- **Branch:** `claude/project-thread-g4hajn`, uncommitted working tree
- **SDD:** [README.md](../README.md) (FR-301…FR-304)
- **Result:** no open blocker. 0 findings to fix, 3 accepted with a reason.

## Scope

`web/src/features/shopping/history-page/**`: `history-groups.ts` (was `history-days.ts`), `HistoryMonth.tsx` (new),
`HistoryDay.tsx` (heading level), `HistoryPage.tsx`, their tests; `docs/architecture.md`, `docs/specs/README.md`.

## Method

Read the SDD, design rules and the history page of 001; read every changed file; checked the folder axiom, sizes,
heading order, the month split on local time, the «Показать ещё» paging across months and the count wording; ran the
full web check set (results in [web 002 audit](../../002-name-suggestions/code-audit/audit.md#verification-2026-11-26-after-the-fix),
same run).

## Step 0 — quality targets

No client targets in the repository (project-wide, see web 002 AF-W2-005). The grouping is pure work over the
loaded rows (50 per page), linear in their number; no performance verdict needed or given.

## Findings

| Id        | Severity | Rule / source                                  | Evidence                                                                                                                                                       | Status                                                                                                                                                                                                                                                                                 |
| --------- | -------- | ---------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| AF-W3-001 | Low      | FR-302, product wish «Ноябрь 2026 · N покупок» | The count is of loaded rows; the last month reads «50+ покупок» while more pages exist (`history-groups.ts`, `purchaseCount`, `groupByMonth(…, mayContinue)`). | **Accepted** as designed (SDD «Decision: counts from the loaded rows»): an exact count needs a new API operation and a server-side month time zone. Tests: `history-groups.test.ts` (partial month), `HistoryPage.test.tsx` (the «1+» month becomes «1 покупка» after «Показать ещё»). |
| AF-W3-002 | Info     | FR-303                                         | Rows without a purchase time show a «Без даты» month heading above a «Без даты» day card.                                                                      | **Accepted.** Bought rows always have a time (domain rule); the branch only keeps a broken row visible.                                                                                                                                                                                |
| AF-W3-003 | Info     | Docs of feature 001                            | `001-shopping-list` (tasks, design, traceability) still names `history-days.ts`.                                                                               | **Accepted.** A finished specification records what was built then; `docs/architecture.md` (the lasting description) names `history-groups.ts`.                                                                                                                                        |

## Checked and in order

- **Folders and sizes:** `history-page/` has 6 production files after the change (`HistoryDay`, `HistoryMonth`,
  `HistoryPage`, `history-api.ts`, `history-groups.ts`, `use-return-item.ts`), the month grouping went into the
  renamed pure module instead of a seventh file; `history-groups.ts` 122 lines, `HistoryMonth.tsx` 38.
- **Pure logic tested:** months in order across a year boundary, day labels inside a month, counts with Russian plural
  (1 покупка, 4 покупки, 11 покупок, 21 покупка), partial month, «Без даты» last, empty input.
- **Paging:** the page flattens every loaded page before grouping, so a month (and a day) split across pages
  continues in place, without a second heading (page test).
- **Headings and regions:** page `h1` (section), months `h2` (`h6` style), days `h3`; every month section and day card
  is a region named by its heading (tests query them by name and level).
- **No writes changed:** «Вернуть в список» and delete keep their keys, versions and pending states (existing page
  tests pass).
- **Design:** theme tokens only, `tabular-nums` count, the page's `gap` between months and cards, phone first.
- **No API change, no ADR:** a presentation choice inside one page.
