# Savings page — scope and outcome

## Goal

As Masha or Vitaliy, I want to see how much is in the jar, what moved this month and how far we are from our goal.

## In scope (implemented)

- `/expenses/savings?year=&month=`: balance card («В копилке сейчас», «На начало месяца», «Положили», «Взяли», «На
  конец месяца»), «План копилки», «Движения за месяц» (add / edit / delete), «Копилка по месяцам» with «Показать все
  месяцы»; a month in that list opens it.
- The savings form: direction «Положили в копилку» / «Взяли из копилки» plus a positive amount → a signed request.

## Out of scope

- Editing the goal here (it is on «Настройки», web 003; the card links there when no goal is set).

## Open questions and gaps found while writing this SDD

- [GAP-W05] The goal percent is computed twice in the web with different rounding: «План копилки» on «Копилка» uses
  `Math.floor` (`savings-goal/goal-progress.ts`), the home summary uses `Math.round` (`home/expenses-summary/data/money.ts`
  `percentOf`). At 99.6 % one page shows 99 %, the other 100 %. **Fixed 2026-11-26** (audit AF-02): both round down
  (`goalPercent` / `goalPercentOf`), with the same test cases on each side.
- [GAP-W06] The savings tests cover add (withdrawal), edit and delete, but not the repeated-create key reuse or the
  stale-version message for movements (shared code is tested through expenses).
