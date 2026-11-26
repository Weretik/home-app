# Savings page — rules and scenarios

## Business rules

### R-W2-001 — Balance figures come from the API

«В копилке сейчас» is `currentBalance`; the month card shows `balanceAtStart`, `deposits`, `withdrawals`,
`balanceAtEnd` of the open month.

### R-W2-002 — Movements are signed

A deposit shows with a plus and the success colour, a withdrawal with a real minus and the error colour. The form takes
a direction and a positive amount and sends a negative amount for a withdrawal; an edit of a withdrawal shows its
amount positive with «Взяли из копилки».

### R-W2-003 — New movement defaults

Direction «Положили в копилку», person Виталий, date today in the current month or the 1st of the open month.

### R-W2-004 — Writes

As entries (web 001): one key per opened form, `If-Match` on edit, confirmation before delete; success refreshes
`['expenses','savings']` (movements and months together).

### R-W2-005 — The goal card

With `savingsGoal > 0`: whole percent done (never below 0, may pass 100, rounded down), now / goal / left (0 once
reached), and «сделано» when reached. With no goal: «План не задан» and a link «Задать план в настройках».

### R-W2-006 — Months overview

Every month with movements, newest first, with its end balance; «Показать все месяцы» expands the list; a month opens it.

## Acceptance scenarios

### SC-W2-001 — Balance and signed movements

**Covers:** R-W2-001, R-W2-002
Given a month with a deposit and a withdrawal
Then the balance card shows the API figures and the rows are signed and coloured

### SC-W2-002 — Take money out

**Covers:** R-W2-002, R-W2-003
When the user adds «Взяли из копилки» 500 with the default person
Then the request has amount −500 and person `Vitaliy`

### SC-W2-003 — Edit and delete

**Covers:** R-W2-004
When the user edits a movement → `If-Match` carries its version; when deleting → confirmation first

### SC-W2-004 — Goal progress

**Covers:** R-W2-005
Given balance 40 000 and goal 100 000 → «40 %», left 60 000
Given balance ≥ goal → reached
Given goal 0 → the card offers to set a goal in «Настройки»

### SC-W2-005 — Form mapping

**Covers:** R-W2-002
Given an existing withdrawal of −120.50 → the form shows «Взяли из копилки» and «120,5»
