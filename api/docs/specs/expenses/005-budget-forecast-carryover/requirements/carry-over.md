# Budget forecast and carry-over — carry-over of overspend

## Behavior

- What a limit's period spends over the limit is taken from the next period, exactly like the column «Перенос с
  прошлого мес.» of the sheet's «Бюджет» tab. Unspent money is not carried.
- The same happens for the monthly total limit («ИТОГО» against `monthlyLimit`).
- Counting starts at a date the family can change on «Настройки» (`LimitsFrom`, 2026-11-01 at first).

## Business rules

### R-CO-001 — The carry formula

For a limit with amount `A` and its counted periods `p_0, p_1, …` (see R-CO-003, R-CO-004), with `spent_k` the
limit-counted spending of period `p_k`:

- `carry_0 = 0`, `carry_{k+1} = max(0, carry_k + spent_k − A)`;
- for the requested period `p_n`: `carriedIn = carry_n`, `left = A − spent_n − carriedIn`,
  `percent = round((spent_n + carriedIn) / A × 100)` (half away from zero), `carriesOut = max(0, spent_n + carriedIn − A)`;
- `perDay = max(0, round(left / daysLeft, 2))` while the period runs, else `null` (the existing rule with the new `left`).

### R-CO-002 — Only limit-counted expenses

`spent_k` is the sum of the period's expenses in the limit's categories, whoever paid, without outside-limits
expenses (R-OL-002).

### R-CO-003 — Monthly limits step by month from the first month on or after LimitsFrom

The first counted month is the month that starts on `LimitsFrom`, or the next month when `LimitsFrom` is not the 1st.
A month before it has `carriedIn` 0 and `carriesOut` 0, and its spending never reaches a later month.

### R-CO-004 — Quarterly limits step by calendar quarter from the quarter that holds LimitsFrom

The first counted quarter is the calendar quarter that contains `LimitsFrom` (2026-11-01 → Q4 2026, 1 October to
31 December), and its whole spending counts. Every month of a quarter shows the same quarter figures. (See the open
question in [overview](overview.md).)

### R-CO-005 — The current limit amount is used for past periods

The carry for past periods uses the limit's current amount, as the sheet does; there is no amount history.

### R-CO-006 — The monthly total carries the same way

With `monthlyLimit > 0`, the month summary's `carriedIn` / `carriesOut` follow R-CO-001 and R-CO-003 over months,
with `spent` = all limit-counted expenses of the month (every category). With `monthlyLimit = 0` both are 0.

### R-CO-007 — LimitsFrom is a setting

`BudgetSettings.LimitsFrom` is a date (2000-01-01 … 2100-12-31), seeded 2026-11-01 by the migration, returned by
`GET /api/expenses/budget` and changed by `PUT /api/expenses/budget/settings` (optional `limitsFrom`; when left out
the stored value stays) with the settings' version in `If-Match`.

### R-CO-008 — November 2026 still matches the sheet

With `LimitsFrom` 2026-11-01 the carry into November 2026 is 0, so the November figures equal the sheet's; the
existing test `The_MonthSummary_AddsUp_LikeTheSheet` stays green.

## Acceptance scenarios

All scenarios: `LimitsFrom` 2026-11-01, limit «Личные Виталий» monthly 7 000 (category «Личные Виталий»), limit
«Уход и косметика» quarterly 3 000, no outside-limits expenses unless stated.

### SC-CO-001 — Overspend carries into the next month

**Covers:** R-CO-001, R-CO-003

Given November 2026 spending in «Личные Виталий» of 8 000
When the November summary is read
Then the limit has `carriedIn` 0, `spent` 8 000, `left` −1 000, `percent` 114, `carriesOut` 1 000
When the December summary is read with December spending 5 000
Then the limit has `carriedIn` 1 000, `spent` 5 000, `left` 1 000, `percent` 86, `carriesOut` 0
When the January 2027 summary is read
Then the limit has `carriedIn` 0

| Month | spent | carriedIn | left | percent | carriesOut |
| --- | --- | --- | --- | --- | --- |
| 2026-11 | 8 000 | 0 | −1 000 | 114 | 1 000 |
| 2026-12 | 5 000 | 1 000 | 1 000 | 86 | 0 |
| 2027-01 | 0 | 0 | 7 000 | 0 | 0 |

### SC-CO-002 — A large overspend keeps carrying

**Covers:** R-CO-001

Given November spending 8 000 and December spending 9 000
When the January 2027 summary is read
Then the limit has `carriedIn` 3 000 (December: 1 000 + 9 000 − 7 000)

### SC-CO-003 — Underspend does not carry

**Covers:** R-CO-001

Given November spending 5 000
When the December summary is read
Then the limit has `carriedIn` 0 (not −2 000)

### SC-CO-004 — Spending before LimitsFrom is ignored

**Covers:** R-CO-003, R-CO-008

Given October 2026 spending in «Личные Виталий» of 20 000 and November spending 7 000
When the October summary is read
Then the limit has `carriedIn` 0 and `carriesOut` 0
When the November summary is read
Then the limit has `carriedIn` 0 and `carriesOut` 0

### SC-CO-005 — Quarterly carry steps by quarter

**Covers:** R-CO-004

Given spending in «Уходовые средства и косметика» of 1 000 in October 2026 and 2 500 in December 2026
And 2 000 in February 2027
When any month of Q4 2026 is read
Then «Уход и косметика» has `from` 2026-10-01, `to` 2026-12-31, `spent` 3 500, `carriedIn` 0, `carriesOut` 500
When January, February or March 2027 is read
Then it has `carriedIn` 500, `spent` 2 000, `left` 500, `percent` 83, `carriesOut` 0
When any month of Q2 2027 is read
Then it has `carriedIn` 0

### SC-CO-006 — The monthly total carries

**Covers:** R-CO-006

Given `monthlyLimit` 50 000 and limit-counted November expenses of 52 000
When the November summary is read
Then `carriedIn` is 0 and `carriesOut` is 2 000
When the December summary is read
Then `carriedIn` is 2 000
Given `monthlyLimit` 0
Then `carriedIn` and `carriesOut` are 0 in every month

### SC-CO-007 — LimitsFrom is read and changed

**Covers:** R-CO-007

Given the seeded settings
When the budget is read
Then `limitsFrom` is "2026-11-01"
When the settings are saved with the version in `If-Match` and `limitsFrom` "2026-12-01"
Then the answer is 204, the budget shows "2026-12-01", and the December summary has `carriedIn` 0 for every limit
When the settings are saved without `limitsFrom`
Then `limitsFrom` stays as it was
When they are saved with an old version
Then the answer is 409 `expenses.changed_meanwhile`

### SC-CO-008 — Invalid LimitsFrom

**Covers:** R-CO-007

When the settings are saved with `limitsFrom` "1999-12-31" or "2101-01-01"
Then the answer is 400 with a field error for `limitsFrom` and nothing changes

### SC-CO-009 — Per-day uses the new left

**Covers:** R-CO-001

Given today is 2026-12-21 in Kyiv (11 days left including today), December carriedIn 1 000 and spent 5 000
When the December summary is read
Then «Личные Виталий» has `left` 1 000 and `perDay` 90.91

## Deferred scenarios

- Limit amount history per period.
- Carrying unspent money forward.
