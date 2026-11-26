# Budget settings and spending limits — behavior

## Business rules

### R-BG-001 — A spending limit

A limit has a name (trimmed, 1–100 characters), a period `Month` or `Quarter`, an amount greater than 0 with the
money rules, and at least one category, each once, all existing (`404 expenses.category_not_found` otherwise). At
most 100 categories in a request. Limits may overlap.

### R-BG-002 — Its period

A `Month` limit covers the calendar month; a `Quarter` limit covers the calendar quarter holding the month
(January–March, …). Spending is the sum of the period's expenses in its categories, whoever paid (used by 004).

### R-BG-003 — Order

A new limit goes to the end (`SortOrder` of the last + 10); an edit keeps its place. Lists are ordered by `SortOrder`
then name.

### R-BG-004 — Settings

One settings row. `monthlyLimit` and `savingsGoal` are money from 0 (0 = none); invalid values answer 400
(`expenses.budget_settings_invalid` from the domain, or validator errors). Saved together with the version in
`If-Match`; stale → 409; missing → 400.

### R-BG-005 — Limit writes

Create with `Idempotency-Key` (kind `limit`), edit with `If-Match`, delete without version (204 / 404). Deleting a
limit keeps all expenses.

### R-BG-006 — Seed

The `Budget` migration inserts the seven limits of the sheet (Еда и быт 15 000 / month; Личные Виталий 7 000; Личные
Маша 13 000; Кафе, доставка, развлечения 3 000; Уход и косметика 3 000 / quarter; Зоотовары 2 500 / quarter; Другое
15 000 / month over ten categories) and settings 50 000 / 100 000.

## Acceptance scenarios

### SC-BG-001 — Seeded budget

**Covers:** R-BG-006, R-BG-003

When the budget is read on a fresh database
Then it has the seven limits in the sheet's order, monthly limit 50 000 and savings goal 100 000

### SC-BG-002 — Settings are saved with the version

**Covers:** R-BG-004

Given the settings with version V
When they are saved as 42 000.50 / 0 with `If-Match: V`
Then 204 and the budget shows them
When they are saved again with V
Then 409
When saved without `If-Match`, or with −1 or 1.001
Then 400

### SC-BG-003 — A limit is added, changed, deleted

**Covers:** R-BG-001, R-BG-003, R-BG-005

Given a new key
When a limit is created
Then 201 and it is listed last
When it is changed with its version
Then 204; with the old version 409
When it is deleted
Then 204; again 404

### SC-BG-004 — Repeated limit create

**Covers:** R-BG-005

Given a limit created with key K
When K is sent with the same body
Then the same id
When K is sent with other data
Then 409 `expenses.idempotency_key_reused`

### SC-BG-005 — Invalid limit

**Covers:** R-BG-001

When a limit has an unknown category
Then 404 `expenses.category_not_found`
When it has no categories, an empty name, a name over 100 characters, or amount 0
Then 400

### SC-BG-006 — Period bounds

**Covers:** R-BG-002

Given a monthly and a quarterly limit
When the period around 2026-11-15 is asked
Then the monthly one is 2026-11-01 … 2026-11-30 and the quarterly one 2026-10-01 … 2026-12-31
