# Home expenses summary — frontend design (as implemented)

## File map (implemented at 8378e4a, under `web/src/features/home/`)

| Path                                                                                                            | One responsibility                                                 |
| --------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| `page/{HomePage,SectionCard,SummarySwitcher,ComingSoonPage}.tsx`, `summary-sections.ts`, `use-summary-param.ts` | page, registry of summaries, `?s=`                                 |
| `expenses-summary/data/{summary-api,limit-rows,money,month,labels,use-month-param}.ts`                          | query, rows with «ИТОГО», money format and percent, months, labels |
| `expenses-summary/view/{ExpensesSummary,KpiCards,LimitsCard,LimitFill,SpendingCards,MonthPicker}.tsx`           | the summary's parts                                                |

(Web 005 moves the limit files to `expenses-summary/limits/` and adds `data/figure-lines.ts`, `view/ForecastCard.tsx`.
The audit of 2026-11-26 splits `view/` into `overview/` (ExpensesSummary, SummaryContent, EmptyMonth, MonthPicker)
and `figures/` (KpiCards, KpiCard, ForecastCard, SpendingCards, AmountsCard, card-lines.ts).)

## State and data

One query `['expenses','summary',month]` (`staleTime` 30 s from the query client).

## Boundaries

`home` never imports `expenses`: own labels, money format, month helpers (GAP-W11).

## Test design

`HomePage.test.tsx`, `limit-rows.test.ts`.
