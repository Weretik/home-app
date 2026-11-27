# Overview, rules and scenarios

## Outcome

Changing a limit's amount (a category limit or the monthly limit on all expenses) never changes what an earlier
period was measured against. The month summary, the carry-over and the budget page show, for every period, the
amount that was in force in that period.

## Terms

- **Step:** an amount and the first day of the period it starts in (a month start, or a quarter start for a
  quarterly limit). A limit has one or more steps, ordered by that day.
- **Amount of a period:** the amount of the last step that starts on or before the period's first day; for a period
  before the first step, the first step's amount.

## Rules

- **R-LA-001** A new limit (and the monthly limit as migrated) has one step that starts on 2000-01-01, so it applies to
  every period, as before.
- **R-LA-002** Changing the amount takes `amountFrom` (any day; it is moved to the start of its month, or of its quarter
  for a quarterly limit). With an explicit `amountFrom` the steps that start on or after that period are always replaced
  by one step with the new amount, even when that amount is already in force there, so a planned later change can be
  cancelled by sending the current amount from the current period. A step equal to the amount of the period just
  before it is not stored (no-op steps are merged).
- **R-LA-003** Left out, `amountFrom` is the period that holds today (Europe/Kyiv). It must fall between 2000-01-01
  and 2100-12-31.
- **R-LA-004** Changing a limit from month to quarter or back starts its history again: one step from 2000-01-01 with
  the new amount (month steps have no meaning for quarters).
- **R-LA-005** Without `amountFrom`, an amount already in force in the period that holds today leaves the steps alone,
  so a change of name or categories that sends the amount back as it was keeps a planned later step. The same holds
  for `monthlyLimitFrom` of the monthly limit.
- **R-LA-006** The carry-over of every period uses that period's amount:
  `carry(next) = max(0, carry + spent − amount(period))`. A period whose amount is 0 (the monthly limit set to «no
  limit» from some month) has no limit: it takes nothing in, passes nothing on, and the carry starts again from 0 after it.
- **R-LA-007** The month summary reports, per limit and for the monthly limit, the amount of the asked period
  (`LimitUsage.amount`, `monthlyLimit`), and `left`, `percent`, `perDay` and the forecast's room use it.
- **R-LA-008** `GET /api/expenses/budget` returns, per limit and for the monthly limit, the amount in force today
  plus all steps (`amounts`, `monthlyLimits`), oldest first.
- **R-LA-009** The current amounts move into one step each in the migration; nothing about past months changes on the
  day of the release.

## Scenarios

- **SC-LA-001** A monthly limit of 7 000 since 2000; on 2026-12-10 it is changed to 5 000 without `amountFrom`. The
  November summary still shows 7 000 and its carry, December shows 5 000, and November's overspend carried into
  December is computed against 7 000.
- **SC-LA-002** Steps 7 000 (2000-01), 5 000 (2026-12). Changing to 6 000 from 2026-11 replaces both later-or-equal
  steps: 7 000 (2000-01), 6 000 (2026-11).
- **SC-LA-003** Steps 7 000 (2000-01), 5 000 (2026-12). Changing to 7 000 from 2026-12 leaves one step: 7 000 (2000-01).
- **SC-LA-004** A quarterly limit, `amountFrom` 2027-02-14 → the step starts on 2027-01-01.
- **SC-LA-005** A monthly limit becomes quarterly with 18 000 → one step 18 000 from 2000-01-01.
- **SC-LA-006** Carry: limit 7 000 in November, 5 000 from December; spent 8 000 in November, 4 000 in December →
  December takes in 1 000 and passes on 0 (1 000 + 4 000 − 5 000).
- **SC-LA-007** The monthly limit is 50 000 until November, 0 in December and 50 000 again from January; November was
  over, yet December and January take in nothing (the month without a limit resets the carry).
- **SC-LA-008** `amountFrom` 1999-12-01 → 400 with the validation problem.
- **SC-LA-009** After the migration every limit has exactly one step from 2000-01-01 with its old amount, and the
  monthly limit likewise.
- **SC-LA-010** Steps 7 000 (2000-01), 5 000 (2027-01); in December the limit is sent with 7 000 and `amountFrom`
  2026-12-01 → one step 7 000 (2000-01), January keeps 7 000. Sent with 7 000 and no `amountFrom` (a rename) → the
  steps stay as they were.

## Open questions

None. Kept as before: the savings goal and `LimitsFrom` have one value.
