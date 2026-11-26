# Budget forecast and carry-over — expenses outside limits

## Behavior

- When adding or editing an expense, the person can mark it «Не в лимиты».
- Such an expense appears in every month total (expenses, by payer, top categories, result) but in no spending limit,
  not in the monthly total limit («ИТОГО») and not in any carry-over.
- The month summary says how many such expenses the month has and their sum.

## Business rules

### R-OL-001 — The flag is part of an expense

An expense has `OutsideLimits`, false by default. A create or an edit that does not send it stores false. The list
returns it for every row. An edit replaces it like any other field.

### R-OL-002 — Totals count it, limits do not

An outside-limits expense counts in `expenses`, `expensesByPayer`, `topCategories` and `result` of the month, and is
excluded from every limit's `spent`, from the limit-counted total against `monthlyLimit`, and from every carry-over.

### R-OL-003 — The summary reports it

`outsideLimitsCount` (int) and `outsideLimitsAmount` (money) are the number and sum of the month's outside-limits
expenses; both 0 when there are none.

### R-OL-004 — Idempotent create covers the flag

The create fingerprint includes the flag: the same `Idempotency-Key` with the flag changed answers 409
`expenses.idempotency_key_reused`.

## Acceptance scenarios

### SC-OL-001 — An expense is created outside limits

**Covers:** R-OL-001

Given no expenses in November 2026
When an expense of 1 000 in «Личные Виталий» is created with `outsideLimits: true`
Then the answer is 201 with the new id
And the November list returns that row with `outsideLimits: true`

### SC-OL-002 — The flag defaults to false and is editable

**Covers:** R-OL-001

Given an expense created without `outsideLimits`
When the list is read
Then the row has `outsideLimits: false`
When it is edited with its version in `If-Match` and `outsideLimits: true`
Then the answer is 204 and the list shows `outsideLimits: true`

### SC-OL-003 — Counted in totals, not in limits

**Covers:** R-OL-002, R-OL-003

Given the limit «Личные Виталий» of 7 000 a month, `LimitsFrom` 2026-11-01, monthly limit 50 000
And in November 2026 expenses in «Личные Виталий»: 6 000 normal and 1 000 outside limits
When the November summary is read
Then `expenses` includes 7 000 for these rows and the category's top entry is 7 000
And the limit «Личные Виталий» has `spent` 6 000, `left` 1 000, `carriesOut` 0
And the limit-counted total against `monthlyLimit` includes 6 000, not 7 000
And `outsideLimitsCount` is 1 and `outsideLimitsAmount` is 1 000

| November «Личные Виталий» | limit spent | month expenses | outsideLimitsCount / Amount |
| --- | --- | --- | --- |
| 6 000 normal + 1 000 outside | 6 000 | 7 000 | 1 / 1 000 |
| 8 000 outside only | 0 | 8 000 | 1 / 8 000 |
| none | 0 | 0 | 0 / 0 |

### SC-OL-004 — An outside-limits overspend does not carry

**Covers:** R-OL-002

Given the limit «Личные Виталий» 7 000 a month and November spending of 7 000 normal plus 5 000 outside limits
When the December summary is read
Then «Личные Виталий» has `carriedIn` 0

### SC-OL-005 — Same key, other flag

**Covers:** R-OL-004

Given an expense created with key K and `outsideLimits: false`
When the same body is sent with key K and `outsideLimits: true`
Then the answer is 409 `expenses.idempotency_key_reused` and no second row exists

## Deferred scenarios

- Filtering the expense list by the flag.
