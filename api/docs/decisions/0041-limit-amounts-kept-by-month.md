# 0041. Limit amounts are kept by the period they start in

Status: accepted, 2026-11-27

Specification: [SDD 006 — limit amounts by month](../specs/expenses/006-limit-amounts-by-month/README.md).

## Context

[0027](0027-limit-carry-over-like-the-sheet.md) carried overspend like the old sheet and used the current amount of
a limit for past periods too. The family lowers limits from time to time, and with one amount per limit every such
change rewrote the past: months that were already closed suddenly showed more overspend and pushed a bigger carry into
the current month. Vitaliy asked that every month keeps its own limit.

## Decision

- A category limit and the monthly limit on all expenses keep a list of steps: an amount and the first day of the
  month (or quarter) it starts in. The amount of a period is the amount of the last step that starts on or before it.
- A change applies from a chosen period onward and replaces the later steps; without a chosen period it applies from
  the current one, and an amount already in force there changes nothing (so a rename keeps a planned change). A change of
  month ↔ quarter starts the history again.
- The carry-over walks the periods with each period's own amount; a period with amount 0 (no monthly limit) resets the
  carry.
- The steps are a value object (`AmountHistory`) of the aggregate, stored as a `jsonb` column of its row. They are
  always read with the row and never filtered by SQL, so a child table would only add joins.
- The existing amounts became one step from 2000-01-01 in the migration, so nothing about the past changed on release.
  The migration only adds (the old single-amount columns stay until a follow-up migration), so the previous release
  keeps working while the migration runs before the new code.

## Consequences

- Lowering or raising a limit no longer changes earlier months; the budget page can show the earlier amounts.
- Choosing a past month as the start rewrites that month and the later ones on purpose (fixing a mistake).
- The savings goal and `LimitsFrom` still have one value; they can get the same treatment if needed.
- 0027's sentence "the current amount of a limit is used for past periods too" is superseded by this record.
