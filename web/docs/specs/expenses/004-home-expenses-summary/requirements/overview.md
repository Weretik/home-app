# Home expenses summary — scope and outcome

## Goal

As Masha or Vitaliy, I want the first page to tell me how the month is going, so that I don't need the sheet's «Сводка».

## In scope (implemented)

- Switcher of section summaries; sections without a summary are disabled and marked «скоро»; a missing or unknown
  `?s=` opens the first built one.
- Expenses summary for a month with arrows; loading, empty («За этот месяц записей нет…»), error with «Повторить».
- «Открыть расходы» opens the same month in the Expenses section.

## Out of scope (at 8378e4a)

- Carry-over, planned amounts, outside-limits line and the forecast card (web 005).

## Open questions and gaps found while writing this SDD

- [GAP-W09] Summary rules duplicated in the web: the «ИТОГО» row's left, percent and per day
  (`limit-rows.ts`), and the percent on the Расходы and Копилка cards (`percentOf`), are computed in TypeScript while
  the API computes the same for limits in C#. A change of a rule must be made twice (see api 004 GAP-014).
- [GAP-W10] The summary query is not invalidated by any write (web 001 GAP-W01, web 003 GAP-W07). **Fixed 2026-11-26**
  (audit AF-01).
- [GAP-W11] `home` repeats the month helpers and a month picker of `expenses` (`expenses-summary/data/month.ts`,
  `use-month-param.ts`, `view/MonthPicker.tsx`) because features must not import each other. Accepted by the rules;
  noted so a shared place (`src/app` or `src/platform`) can be considered when a third copy appears.
