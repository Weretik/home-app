# 0019. Spending limits are named, overlapping category sets stored in the module

Status: accepted, 2026-11-26; the part on carrying overspend is superseded by
[0021](0021-limit-carry-over-like-the-sheet.md) (overspend now carries into the next period like the sheet)

## Context

The sheet's «Бюджет» tab sets how much may be spent, and «Сводка» compares it with what was spent. Its limits do not
map one to one onto categories: «Еда и быт» is Продукты plus Быт, two limits («Уход и косметика», «Зоотовары») are per
quarter, and «Другое» counts ten categories, three of which also have a limit of their own. Besides the category
limits the sheet has a limit on all expenses of a month and a goal for the savings jar
([0018](0018-savings-jar-as-signed-movements.md)). The family wants to change these numbers on the site.

Options considered: a limit column on `Category` (cannot express a limit over several categories, nor overlaps); a
budget per month copied forward (more rows and a copy job for numbers that rarely change); limits as named sets of
categories (chosen).

## Decision

- The budget belongs to the Expenses module, in `Domain/Budget/`.
- `SpendingLimit` is its own aggregate: a name, a `LimitPeriod` (`Month` or `Quarter`, calendar periods), a positive
  amount, and a set of category ids, at least one, each once. Limits may overlap; a category may count towards any
  number of limits. What a limit has spent is the sum of the expenses in its categories over its period, whoever
  paid.
- The category ids are a `uuid[]` column of `expenses.spending_limits`, read as a set and never joined on, so there is
  no foreign key; the handlers check that every id is a stored category on create and edit. Categories cannot be
  deleted today; when they can, the delete must handle the limits that name them.
- `BudgetSettings` is a singleton row (fixed id) with the monthly limit on all expenses and the savings goal; `0`
  means none.
- A limit holds one current amount, not a history. Changing it changes how past months compare with it.
  ~~Overspending is not carried into the next period.~~ Superseded by [0021](0021-limit-carry-over-like-the-sheet.md):
  overspend carries over from `BudgetSettings.LimitsFrom`.
- Writes follow the module's entries: `Idempotency-Key` on create ([0007](0007-idempotency-key-for-creates.md)), the
  `xmin` row version in `If-Match` on edits ([0017](0017-row-version-via-xmin-for-edits.md)), the `PublicWrite` rate
  limit ([0015](0015-expenses-open-until-identity.md)).
- The `Budget` migration seeds the sheet's limits and settings on the day of the move with SQL, since EF seed data
  cannot hold the array column without a model change on every build.

## Consequences

- Every limit of the sheet, overlaps included, is expressed without special cases, and new combinations need no code.
- Without a history of limits, a past month is measured against today's numbers. If that matters later, a limit can
  get a valid-from date without changing the shape of the API.
- An array column without a foreign key can name a category that no longer exists; the check on write and the absence
  of category deletes keep it consistent for now.
