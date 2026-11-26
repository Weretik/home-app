# Savings jar — behavior

## Business rules

### R-SV-001 — A signed movement

A movement has a date (2000–2100), a person (`Vitaliy`, `Maria`), a signed amount and an optional comment. The
absolute amount follows the money rules of R-EN-001; the amount is never 0 (`expenses.amount_zero`). Plus is
`Deposit`, minus is `Withdrawal`; the direction is derived, not stored.

### R-SV-002 — The balance is a sum

The jar's balance at any moment is the sum of all movements up to it; it may go below zero. Nothing is stored.

### R-SV-003 — A month's view

For `year` (2000–2100) and `month` (1–12): the month's movements (newest first, paged like entries), `deposits` and
`withdrawals` (both positive), `balanceAtStart` (sum before the 1st), `balanceAtEnd` = start + deposits −
withdrawals, `currentBalance` (sum of everything).

### R-SV-004 — The months overview

Every month that has movements, newest first: deposits, withdrawals and the running closing balance; plus
`currentBalance`.

### R-SV-005 — Writes

As entries: create with `Idempotency-Key` (kind `savings`), edit with `If-Match`, delete without version; same error
codes (`expenses.savings_movement_not_found`).

### R-SV-006 — The goal

`savingsGoal` is money from 0 (0 = no goal), part of the budget settings, edited with them (R-BG-004 in 003).

## Acceptance scenarios

### SC-SV-001 — A month adds up

**Covers:** R-SV-001 … R-SV-004

Given +1 000 on 2043-01-10, +300 on 2043-02-03 and −120.50 on 2043-02-05
When February 2043 is read
Then it lists 2 movements, newest first, the withdrawal with direction `Withdrawal` and amount −120.50
And `deposits` is 300, `withdrawals` 120.50, `balanceAtEnd − balanceAtStart` 179.50
And the overview shows January deposits 1 000, February withdrawals 120.50

### SC-SV-002 — Zero is refused

**Covers:** R-SV-001

When a movement of 0 is created
Then 400 and nothing is stored

### SC-SV-003 — An edit changes the month

**Covers:** R-SV-005

Given the −120.50 withdrawal with its version
When it is changed to −20.50 with that version
Then 204 and February's withdrawals are 20.50

### SC-SV-004 — Repeats and stale edits

**Covers:** R-SV-005

Given a movement created with key K
When K is sent again with the same body
Then 201 with the same id
When it is edited with an old version
Then 409 `expenses.changed_meanwhile`
When an unknown id is changed or deleted
Then 404

### SC-SV-005 — The goal is read with the budget

**Covers:** R-SV-006

Given the seeded settings
When the budget is read
Then `savingsGoal` is 100 000

## Deferred scenarios

- Paging the months overview.
