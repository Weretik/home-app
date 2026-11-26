# 0006. Section pages as tabs, with the month and filters in the URL

Status: accepted, 2026-11-26

## Context

The Expenses section replaces a Google Sheet with many sheets (Расходы, Доходы, Обзор, Лимиты, План, Платежи, По
месяцам, Копилка, Категории). Every sheet shows one month, and the two people send each other links to «that month,
only groceries». The bottom bar holds at most five sections, so the sheets cannot become sections of their own.

## Decision

- A section with several pages shows them as tabs under its title. The tabs are links to nested routes
  (`/expenses`, `/expenses/incomes`); a page gets a tab only when it is built, so there are no dead tabs. The list of
  pages lives in one file of the feature (`features/expenses/section/expenses-pages.ts`).
- The month a page shows and its list filters live in the URL search params (`?year=2026&month=10&payer=Shared`), not
  in component or global state. Without them a page shows the current month and no filter. Switching tabs keeps the
  month and drops the filters, because each page has its own.
- Server data stays in React Query; the URL params are part of its query keys.

## Consequences

- A link, a reload or the browser's back button opens the same month with the same filters.
- No state library is needed for page state; a feature reads and writes the params through small hooks
  (`use-month-param.ts`, `use-filter-param.ts`).
- Invalid params fall back to the current month or no filter instead of an error.
- Adding a page is one entry in the page list plus one route.
