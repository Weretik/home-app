# Month summary — scenario traceability (retroactive)

| Scenario | Rules | Test level | Code | Evidence | Status |
| --- | --- | --- | --- | --- | --- |
| SC-MS-001 | R-MS-001 … R-MS-006 | integration | `GetMonthSummaryHandler` | `ExpensesBudgetApiTests.The_MonthSummary_AddsUp_LikeTheSheet` | verified |
| SC-MS-002 | R-MS-005 | domain + application unit | `SpendingLimit.PeriodAround`, `SummaryFigures.Usage` | `SpendingLimitTests.A_Quarterly_Limit_CoversTheCalendarQuarter`, `SummaryFiguresTests.A_Quarterly_Limit_CountsEveryMonthOfItsQuarter_AndNotTheNext` | verified |
| SC-MS-003 | R-MS-004, R-MS-005 | application unit (fixed day) | `SummaryFigures.DaysLeft`, `.Usage` | `SummaryFiguresTests.DaysLeft_CountsToday_AndIsNull_OutsideThePeriod`, `.PerDay_IsNull_OutsideThePeriod_AndNeverBelowZero_WhenOverspent`, `.PerDay_TakesTheCarryIntoAccount_AndRoundsToKopecks` | verified |
| SC-MS-004 | R-MS-007 | validator unit + integration | `GetMonthSummaryValidator` | `MonthSummaryValidatorTests.A_Month_IsACalendarMonth_In2000To2100`, `ExpensesSummaryApiTests.A_Month_OutsideTheCalendar_IsAValidationProblem` | verified |
