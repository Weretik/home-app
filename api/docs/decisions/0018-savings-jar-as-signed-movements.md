# 0018. The savings jar is a log of signed movements

Status: accepted, 2026-11-26

## Context

The sheet's «Копилка» tab tracks money put aside on card *8140: each row has a date, a person, an amount with a
sign (plus put in, minus taken out) and a comment, and the sheet sums them into a balance and monthly totals. It is
the next tab to move to the site after expenses and incomes ([0013](0013-expenses-module-model.md)).

## Decision

- The jar belongs to the Expenses module. `SavingsMovement` is its own aggregate in `Domain/Savings/`, in table
  `expenses.savings_movements`.
- The amount is signed, as in the sheet: plus is a deposit, minus a withdrawal, zero is refused. `SavingsDirection`
  is derived from the sign and never stored. Otherwise the rules of expenses and incomes apply (date range, kopecks,
  limit, comment).
- The balance is never stored. The current balance, a month's opening and closing balance and the monthly deposits
  and withdrawals are sums over the movements in SQL.
- Creates, edits and deletes follow the module's other entries: `Idempotency-Key` on create
  ([0007](0007-idempotency-key-for-creates.md)), row version on edit
  ([0017](0017-row-version-via-xmin-for-edits.md)), import keys for rows from the sheet.

## Consequences

- No stored balance can drift from its movements; an edit or delete of an old movement corrects every later balance
  at once.
- Each read sums the whole table. With a few rows a month that stays small for years; if it ever does not, a monthly
  snapshot table can be added without changing the API.
- A second jar or account would need an account id on the movement; that is a later decision.
