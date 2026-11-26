# 0013. The Expenses module and its model

Status: accepted, 2026-11-26

## Context

The family has kept its money in the Google Sheet «Учёт расходов» since October 2026: a log of expenses («Расходы»)
and a log of incomes («Доходы»), with categories in «Справочник», and summaries, limits, plans, payments and savings
on other tabs. It moves to this site first, before shopping, to-dos and gifts.

## Decision

- One module, `Expenses`, owns both expenses and incomes, with its own schema `expenses`. Incomes are too small and
  too tied to monthly totals to be a module of their own.
- `Expense` and `Income` are separate aggregates. An expense has a date, a `Payer` (`Vitaliy`, `Maria` or
  `Shared`), a category, an amount, a payment method and a comment; an income has a date, a `Person`, an amount and
  a comment, and no category (the sheet dropped them).
- `Category` is a table, not an enum: categories are renamed and merged often. The 14 categories of the sheet are
  inserted by the first migration with fixed ids. Expenses reference them with a real foreign key (same schema).
- Money is `decimal` in the code and `numeric(12,2)` in the database, in hryvnias; dates are `DateOnly`.
- Bank cards are not modelled yet; their numbers stay in comments.
- The sheet's other tabs (limits, plan, payments, savings, monthly balance) are later use cases of this module and
  pages of its section; each gets its model when it is built.

## Consequences

- Monthly totals, limits and plans can all be computed from two tables with indexed dates.
- Changing categories is a data change, but the seed in the first migration must never be edited; later changes
  go through use cases.
- If incomes ever need categories or accounts, they are added to this module without touching expenses.
