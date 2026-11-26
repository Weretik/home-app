# TS-012 — Month summary: carry-over, outside limits, planned, forecast

- **Task ID:** TS-012
- **Covers:** SC-OL-003, SC-OL-004, SC-CO-001 … SC-CO-006, SC-CO-009, SC-FC-007, SC-FC-008
- **Depends on:** TS-003, TS-005, TS-007, EN-001
- **Exact paths:** `src/Modules/Expenses/Expenses.Application/Features/Summary/Queries/GetMonthSummary/GetMonthSummaryHandler.cs`, `.../Summary/Report/{MonthSummary,LimitUsage,MonthForecast}.cs` (relative to `api/`)
- **Test level:** integration (summary over seeded data)
- **Architecture basis:** [ADR 0020](../../../../../../decisions/0020-month-summary-computed-on-read.md), [API rules, Data](../../../../../../rules.md#data)

## Work

Order: Red → Green → Refactor → Regression → evidence.

- [ ] Red: integration tests with the numbers of each listed scenario; `The_MonthSummary_AddsUp_LikeTheSheet` stays green (R-CO-008).
- [ ] Green: one grouped limit-spending query from the first counted period, filtered `outside_limits = false`; totals keep all expenses; outside-limits count/sum; plans grouped by category and month; `LimitCarryOver` per limit and for the total; forecast.
- [ ] Refactor: if the handler mixes reading and per-limit arithmetic beyond one screen, move the per-limit part into a sibling type under `Features/Summary/` and record the decision here.
- [ ] Check the changed files for one responsibility each; split a file that mixes independent responsibilities
      along the module's folders (Domain by aggregate, Application `Features/<Feature>/Commands|Queries/<UseCase>/`,
      Api by feature); keep the six-files-per-folder rule.

## Evidence

- Test and assertion:
- Red command and expected behavioral failure:
- Green command and result:
- Refactor note and re-run: 2026-11-26 audit — the handler mixed reading with per-limit, carry and forecast
  arithmetic (208 lines); the arithmetic moved to `GetMonthSummary/SummaryFigures.cs` (pure, unit-tested in
  `UnitTests/Expenses/Summary/SummaryFiguresTests.cs` with a fixed day, SC-CO-009 included) and the per-category
  month sum to `CategoryMonthAmount.cs`; the handler only reads and assembles (142 lines). Integration suite green.
- Regression command and result:
- Deviation or blocker:

## Done when

Every listed scenario passes and the November 2026 sheet test is unchanged.
