# Budget forecast and carry-over — forecast: planned expenses

## Behavior

- For any month the family keeps a list of planned expenses («Прогноз»): name, category, who pays, amount.
- A plan can be changed, deleted, or recorded as a real expense, which removes the plan.
- The month summary shows the forecast: what is planned, the expected expenses, the expected result and what is
  expected to stay within the monthly limit; every limit shows the planned amount of its categories.

## Business rules

### R-FC-001 — A plan

A plan belongs to one month (stored as its first day), has a name (trimmed, 1–100 characters), an existing category,
a payer (`Vitaliy`, `Maria`, `Shared`) and an amount with the expense money rules (greater than 0, at most
10 000 000, two decimals). It has a creation time and a row version. It has no outside-limits flag: a plan always
counts toward its limits.

### R-FC-002 — Listing plans

The plans of a month are listed ordered by name, with `totalAmount` = the sum of the month's plans.

### R-FC-003 — Writing plans

A create takes an `Idempotency-Key` (kind `planned` in `create_requests`; same key + same data → the first id; same
key + other data → 409 `expenses.idempotency_key_reused`). An edit and a delete carry the plan's version in
`If-Match`; a stale version answers 409 `expenses.changed_meanwhile`; an unknown id answers 404. An unknown category
answers 404 `expenses.category_not_found`.

### R-FC-004 — Recording a plan

`record` takes the date, the payment method and an optional comment, and the plan's version in `If-Match`. In one
transaction (one `SaveChanges`) it creates an expense with the plan's payer, category and amount, `OutsideLimits`
false, and deletes the plan. It answers 201 with the new expense id. A repeat after success finds no plan and answers
404 — no second expense can appear. A stale version answers 409 and creates nothing. The expense date follows the
expense rules (2000–2100); it need not lie in the plan's month.

### R-FC-005 — The forecast block

`MonthSummary.forecast` = `{ planned, expectedExpenses, expectedResult, expectedTotalLeft }`:

- `planned` = sum of the month's plans;
- `expectedExpenses = expenses + planned`;
- `expectedResult = incomes − expectedExpenses − savingsNet`;
- `expectedTotalLeft = monthlyLimit − (limit-counted expenses + planned) − carriedIn` when `monthlyLimit > 0`, else 0.

### R-FC-006 — Planned per limit

`LimitUsage.planned` = the sum of plans in the limit's categories over the months of the limit's current period (the
quarter for a quarterly limit). Planned amounts do not change `spent`, `left`, `percent`, `perDay` or any carry-over.

## Acceptance scenarios

### SC-FC-001 — Plans are added and listed

**Covers:** R-FC-001, R-FC-002, R-FC-003

Given no plans for December 2026
When plans «Подарки маме» 3 000 (Подарки, Shared) and «Абонемент» 2 000 (Здоровье, Maria) are created, each with a
new `Idempotency-Key`
Then each answer is 201 with an id
And `GET /api/expenses/planned?year=2026&month=12` returns «Абонемент» then «Подарки маме», each with `version`,
`categoryName`, and `totalAmount` 5 000

### SC-FC-002 — Repeated create

**Covers:** R-FC-003

Given a plan created with key K
When the same request is sent again with K
Then the answer is 201 with the same id and the month still has one such plan
When K is sent with another amount
Then the answer is 409 `expenses.idempotency_key_reused`

### SC-FC-003 — Invalid plan

**Covers:** R-FC-001

When a plan is created with an empty name, a name of 101 characters, amount 0, amount 1.005, month 13 or year 1999
Then the answer is 400 with field errors and nothing is stored
When it is created with an unknown category
Then the answer is 404 `expenses.category_not_found`

### SC-FC-004 — Edit and delete with the version

**Covers:** R-FC-003

Given a plan with version V
When it is changed with `If-Match: V`
Then the answer is 204 and the list shows the new values and a new version
When it is changed or deleted again with `If-Match: V`
Then the answer is 409 `expenses.changed_meanwhile`
When it is deleted with the current version
Then the answer is 204 and the list no longer has it
When an unknown id is changed or deleted
Then the answer is 404

### SC-FC-005 — A plan becomes an expense

**Covers:** R-FC-004

Given the plan «Подарки маме» 3 000, Подарки, Shared, for December 2026, with version V
When it is recorded with `If-Match: V`, date 2026-12-20, payment method Card, comment «торт»
Then the answer is 201 with an expense id
And the December expense list has that expense: 3 000, Подарки, Shared, Card, «торт», `outsideLimits` false
And the December plans no longer contain «Подарки маме»
When the same record request is sent again
Then the answer is 404 and the expense list still has exactly one such expense

### SC-FC-006 — Recording a changed plan fails as a whole

**Covers:** R-FC-004

Given a plan whose version is no longer V
When it is recorded with `If-Match: V`
Then the answer is 409 `expenses.changed_meanwhile`, no expense is created and the plan stays

### SC-FC-007 — The forecast in the summary

**Covers:** R-FC-005

Given December 2026: incomes 80 000, expenses 30 000 of which 2 000 outside limits, savings net 10 000,
plans 3 000 + 2 000, `monthlyLimit` 50 000 and total `carriedIn` 2 000
When the December summary is read
Then `forecast.planned` is 5 000, `forecast.expectedExpenses` 35 000, `forecast.expectedResult` 35 000 and
`forecast.expectedTotalLeft` 15 000 (50 000 − (28 000 + 5 000) − 2 000)
Given `monthlyLimit` 0
Then `forecast.expectedTotalLeft` is 0

### SC-FC-008 — Planned per limit

**Covers:** R-FC-006

Given the limit «Еда и быт» (Продукты, Быт) monthly 15 000 and a December plan of 3 000 in Продукты
And the quarterly limit «Зоотовары» 2 500 with plans of 500 in October and 700 in December in Зоотовары
When the December summary is read
Then «Еда и быт» has `planned` 3 000 and its `spent`, `left` and `carriesOut` are as without the plan
And «Зоотовары» has `planned` 1 200

## Deferred scenarios

- Copying the previous month's plans.
- Plans that repeat every month («Платежи»).
