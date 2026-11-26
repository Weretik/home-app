# React design

- **Routing:** React Router 6 nested routes under `/shopping` with `ShoppingSection` as the layout (`<Outlet />`), as
  Expenses (ADR 0006).
- **Server state:** TanStack Query 5 (`useQuery`, `useInfiniteQuery` for history, `useMutation` with
  `onMutate` / `onError` / `onSettled` for the optimistic toggle). No other state library.
- **Local state:** dialogs and the snackbar in component state; «Я» via `useSyncExternalStore` so the header toggle
  and the list share it without context.
- **Controllers:** `use-quick-add.ts` (add + undo), `status-api.ts` (toggle), `use-me.ts`; pages stay thin.
- **UI:** MUI 9 components only, theme tokens only (design.md). Phone first: sticky quick add (`position: sticky`
  under the header), full-screen dialogs below `sm`, 48 px checkboxes, horizontal chip row with scroll.
- **Forms:** plain controlled inputs with pure `*-form.ts` checks (as Expenses `limit-form.ts`), API field errors
  under the fields.
- **Debounce:** a small `useDebouncedValue` hook (setTimeout in an effect) for the department preview and keyword
  search.
- **Testing:** Vitest + Testing Library with a stubbed global `fetch` routed by method and path (Expenses pattern);
  pure modules tested directly.
- **Packages:** no new packages.
