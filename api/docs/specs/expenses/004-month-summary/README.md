# 004 — Month summary

- **Module:** Expenses
- **Type:** feature — **retroactive, describes implemented behavior**
- **Status:** implemented (PR #9 `b6fb5d7`); SDD written 2026-11-26 from the code at `8378e4a`
- **Owner:** Vitaliy
- **Created:** 2026-11-26
- **Frontend counterpart:** [web 004](../../../../../web/docs/specs/expenses/004-home-expenses-summary/README.md)
- **Changed by:** [005 forecast and carry-over](../005-budget-forecast-carryover/README.md) (carry-over, outside-limits
  figures, forecast)

One month at a glance, like the sheet's «Сводка» without its «План / Факт / Прогноз» block: incomes by person,
expenses by payer, the result after the jar, the jar and its goal, every spending limit's usage and the top five
categories. Computed on every request, nothing stored ([ADR 0020](../../../decisions/0020-month-summary-computed-on-read.md)).
The November 2026 figures were checked against the sheet.

Retroactive: no behavior change; gaps are in [overview](requirements/overview.md).

## Requirements

- [Overview and scope](requirements/overview.md)
- [Month summary](requirements/month-summary.md)

## Technical design

- [Domain](design/domain.md)
- [Infrastructure](design/infrastructure.md)
- [Data model](data-model.md) (read model only)
- [API contract](contracts/api-contract.md)

## File map (implemented)

| Path (relative to `api/src/Modules/Expenses/`) | State | One responsibility |
| --- | --- | --- |
| `Expenses.Application/Features/Summary/Queries/GetMonthSummary/{GetMonthSummaryQuery,GetMonthSummaryValidator,GetMonthSummaryHandler}.cs` | existing | the query, `year`/`month` validation, sums |
| `Expenses.Application/Features/Summary/Report/{MonthSummary,LimitUsage,PersonAmount,PayerAmount,CategoryAmount}.cs` | existing | the read model records |
| `Expenses.Api/Endpoints/Budget/Overview/GetMonthSummaryEndpoint.cs` | existing | `GET /api/expenses/summary` |

## Planning and delivery

- [Scenario traceability](traceability.md)
- [Delivery readiness](checklist/delivery-readiness.md)

## Change history

- 2026-11-26 — retroactive SDD created from the code at `8378e4a`.
