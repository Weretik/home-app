# Month summary — behavior

## Business rules

### R-MS-001 — Incomes and expenses

Incomes are summed by person and in total, expenses by payer and in total, over the calendar month.

### R-MS-002 — Result

`result = incomes − expenses − savingsNet`, where `savingsNet` is the month's deposits minus withdrawals.

### R-MS-003 — Jar

`savingsBalance` is the jar at the end of the month (all movements before the next month), `savingsGoal` the setting.

### R-MS-004 — Monthly limit and days

`monthlyLimit` comes from the settings (0 = none). `daysLeft` counts the days from today to the end of the month,
today included, and is `null` for any month but the current one. Today is the date in `Europe/Kyiv`.

### R-MS-005 — Limit usage

Every limit, in limit order, gets a row: `from`, `to` (both included, the month or the quarter holding the month),
`amount`, `spent` (expenses of its categories in that period, whoever paid), `left = amount − spent` (negative when
overspent), `percent = round(spent / amount × 100)` (half away from zero), `perDay = max(0, round(left / days left,
2))` while the period runs, else `null`. No carry-over (changed by 005).

### R-MS-006 — Top categories

The five categories with the largest sums in the month, largest first, with names.

### R-MS-007 — Input

`year` 2000–2100, `month` 1–12; otherwise 400.

## Acceptance scenarios

### SC-MS-001 — November 2026 adds up like the sheet

**Covers:** R-MS-001 … R-MS-006

Given the November 2026 entries and the seeded budget
When the November summary is read
Then incomes, expenses, result, jar figures, each limit's spent/left/percent and the top categories equal the sheet's
«Сводка» for November

### SC-MS-002 — A quarterly limit counts the quarter

**Covers:** R-MS-005

Given spending in «Зоотовары» in October and November 2026
When November is read
Then «Зоотовары» has `from` 2026-10-01, `to` 2026-12-31 and `spent` = October + November

### SC-MS-003 — Per day only while the period runs

**Covers:** R-MS-004, R-MS-005

Given today is 2026-11-21 in Kyiv
When November is read
Then `daysLeft` is 10 and every monthly limit has `perDay = max(0, round(left / 10, 2))`
When October is read
Then `daysLeft` is `null` and monthly limits have `perDay` `null`

### SC-MS-004 — Invalid month

**Covers:** R-MS-007

When `month=13` or `year=1999` is requested
Then 400
