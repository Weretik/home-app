# Home expenses summary — React Web design (as implemented)

## Place in the architecture

- Routes: `src/app/root/App.tsx` (`/` index).
- Model (pure logic): `expenses-summary/data/limit-rows.ts`, `money.ts`, `month.ts`.
- State and data: `expenses-summary/data/summary-api.ts`.
- UI: `page/`, `expenses-summary/overview/`, `expenses-summary/figures/`, `expenses-summary/limits/` (was `view/` until 2026-11-26).

## React Web behavior

- React Router nested routes; no lazy loading.
- Dialog forms (MUI) with field messages from the form's own checks first, then the API's field errors; the submit is
  disabled while pending.
- Below `md` a compact MUI `List`, from `md` a `Table` (`useMediaQuery`).
- Theme «Синий плащ» with light, dark and system modes ([web ADR 0005](../../../../../decisions/0005-blue-coat-theme-and-berry-style-shell.md)).
- States: loading text, empty state with the next action, error with «Повторить».

## State and API

- React Query only; no client store. Query client: `staleTime` 30 s, 4xx not retried (`src/platform/query-client.ts`).
- Write errors mapped to Russian by `describeWriteError` (`features/expenses/data/writes-api.ts`).
- No session (open API until Identity).

## Verification

- `npm test`, `npm run typecheck`, `npm run lint`, `npm run architecture:structure`; CI runs them.
- Tests: `HomePage.test.tsx`, `limit-rows.test.ts`.
