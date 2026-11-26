# Budget settings page — scope and outcome

## Goal

As Masha or Vitaliy, I want to change our limits and the jar's goal on the site, so that the summary compares against
our current plan.

## In scope (implemented)

- Card «Бюджет и копилка»: «Общий лимит расходов в месяц» (empty = none) and «План копилки» (empty = none), saved
  together; «Сохранено» after success.
- Card «Лимиты по категориям»: each limit with amount, period label and its categories as chips; «Добавить лимит»;
  row menu to change or delete.
- No month picker on this page.

## Out of scope

- Reordering limits; category management; `limitsFrom` (web 005 open question).

## Open questions and gaps found while writing this SDD

- [GAP-W07] Budget writes invalidate `['expenses','budget']` only, not the home summary (`['expenses','summary',…]`),
  which shows limits and the goal; it can stay stale up to `staleTime` 30 s (see web 001 GAP-W01). **Fixed 2026-11-26**
  (audit AF-01).
- [GAP-W08] `budget-writes-api.ts` fits the settings into the generic `Writer` with `create` and `remove` that reject
  at run time. Works, but the type allows calls that always fail. **Fixed 2026-11-26** (audit AF-03): the settings use
  `useChange`, bound to the budget's version, with no create or delete.
