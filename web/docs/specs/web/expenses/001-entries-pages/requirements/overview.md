# Expenses and incomes pages — scope and outcome

## Goal

As Masha or Vitaliy, I want to see, add, change and delete our expenses and incomes on my phone, so that we no longer
open the Google Sheet.

## In scope (implemented)

- Section `/expenses` with month picker (`?year=&month=`, current month by default), tabs «Расходы», «Доходы»,
  «Копилка», and the gear to «Настройки».
- «Расходы»: filters «Категория» and «Кто» (`?category=&payer=`), «Период» (`?from=&to=`) instead of the month,
  «Добавить расход», rows with date, category, who, amount, payment method and comment, row menu «⋮» to change or delete.
- «Доходы»: filter «Кто» (`?person=`), «Добавить доход», the same row actions.
- «Показать ещё» loads the next 50 rows. No month total card on these pages.

## Out of scope

- Category management, totals per page (an overview page is planned).

## Constraints

- Phone first; Russian text; the [design rules](../../../../../design.md).

## Open questions and gaps found while writing this SDD

- [GAP-W01] Expense and income writes invalidate `['expenses','list']` / `['expenses','incomes']` only. The home
  summary (`['expenses','summary',month]`) is not invalidated, so with `staleTime` 30 s it can show old figures for up
  to 30 s after an edit if it is still cached. `web/docs/architecture.md` says the summary key sits under
  `['expenses']` "so invalidating everything the Expenses module shows refreshes the summary too", but no write
  invalidates `['expenses']` itself. **Fixed 2026-11-26** (audit AF-01, [web ADR 0009](../../../../../decisions/0009-writes-refresh-their-backend-module.md)): every write invalidates the module prefix `['expenses']`, the summary included.
- [GAP-W02] Deletes send no version (backend takes none; see api 001 GAP-001).
- [GAP-W03] `IncomesPage.test.tsx` covers list, filter, empty and error only; adding, changing and deleting an income
  in the UI has no test (the shared dialogs are tested through expenses). **Fixed 2026-11-26** (audit AF-04): `IncomesPage.test.tsx` adds, changes and deletes an income.
- [GAP-W04] There is no end-to-end test in the project; the main path is covered only by page tests with a mocked
  client. Web rules do not require E2E, the frontend SDD template asks for a decision per feature.
