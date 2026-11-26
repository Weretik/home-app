# Budget forecast and carry-over (web) — React Web design

## Place in the architecture

- Routes: `src/app/root/App.tsx`, nested under `/expenses` (`forecast`).
- Model: pure form logic in `*-form.ts`, rows in `planned-rows.tsx`, summary lines in `figure-lines.ts` and `limit-rows.ts`.
- State and data: `planned-data/*-api.ts`, `home/expenses-summary/data/summary-api.ts`.
- UI: `forecast-page/`, `planned-editing/`, `home/expenses-summary/limits/` and `view/`.

## React Web behavior

- Router: React Router nested route; no lazy loading (small app).
- Forms in MUI dialogs; focus goes to the first field; Enter submits; the submit is disabled while pending.
- Lists: MUI `List` below `md`, `Table` from `md` (`useMediaQuery`), like «Расходы».
- Theme: «Синий плащ» ([web ADR 0005](../../../../decisions/0005-blue-coat-theme-and-berry-style-shell.md)), both schemes.
- States: loading, empty («Планов на этот месяц нет»-style empty state with the add button), error with retry.

## State and API

- No client-side store; React Query only. Mutations through `useSave`/`useRemove`.
- Errors mapped to Russian by `describeWriteError` (404 / changed meanwhile / key reused / 429 / 400).
- No session yet (open API).

## Verification

- `npm run typecheck && npm run lint && npm test && npm run architecture:structure`.
- Theme and breakpoints: page tests at phone and computer widths; a manual check at 360 px in both schemes.
- Browser run only on the user's explicit request.
