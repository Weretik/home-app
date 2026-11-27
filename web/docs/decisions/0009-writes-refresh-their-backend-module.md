# 0009. A write refreshes every query of its backend module

Status: accepted, 2026-11-26. Amends the invalidation bullet of [0007](0007-editing-in-dialogs-with-idempotency-and-versions.md).

## Context

The Expenses module is shown by two features: `expenses` (the Бюджет pages, where every write happens) and `home`
(the month summary). Features never import each other ([0004](0004-feature-folders-checked-in-ci.md)), so a write in
`expenses` did not know the summary's query key: it invalidated only its own kind (`['expenses','list']`,
`['expenses','budget']`…), and the summary could show old figures for up to `staleTime` (30 s) after an edit
(web SDD 001 GAP-W01, 003 GAP-W07, 004 GAP-W10). The settings and «Записать как расход» were also squeezed into the
generic add/change/delete writer with a create and a delete that always failed (web SDD 003 GAP-W08).

## Decision

- `src/platform/query-client.ts` (was `query-keys.ts`) holds one query key prefix per backend module (`moduleKeys.expenses = ['expenses']`).
  Every read of the module starts its key with it, in whichever feature it lives.
- Every successful write to the module invalidates the whole prefix (`write-data/write-hooks-api.ts`, `useWrite`);
  a stale entry (404, `expenses.changed_meanwhile`) does the same. React Query refetches only the queries on screen;
  the others are marked stale and refetch when they are shown again.
- Writes are typed by what the API offers: `useSave` (create with an `Idempotency-Key` or change with `If-Match`),
  `useRemove` (delete by id) and `useChange` (a record that is only changed, bound by the caller: the budget settings
  with the budget's version, a plan being recorded).
- Tests hold the contract on both sides: `write-hooks-api.test.tsx` (every write marks the lists, the budget, the plans
  and the summary stale) and `summary-api.test.tsx` (the summary's key is under the prefix).

## Consequences

- The home summary, the budget and the lists never show figures older than the last write in this browser.
- A write refetches the other queries of the module that are on screen (for example the category list next to the
  expenses): one or two small extra requests per write, accepted for a two-person app.
- A new feature that shows a module's data only has to start its keys with the module's prefix; a new module adds one
  entry to `moduleKeys` in `query-client.ts`.
- Another browser still sees a change only after its own `staleTime` or a reload; live updates would need a push
  channel and a new decision.
