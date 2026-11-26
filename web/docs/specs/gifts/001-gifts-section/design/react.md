# Gifts section — React Web design

## Place in the architecture

- App composition and routes: `src/app/root/App.tsx` nests `/gifts` → `GiftsSection` with `index` → `GiftsPage`,
  `archive` → `ArchivePage`, `settings` → `GiftSettingsPage` (same shape as `/expenses`).
- Domain/model: none client-side beyond pure form and filter modules (`gift-form.ts`, `gift-filters.ts`,
  `field-checks.ts`); the API computes order and summary.
- State/data: `gifts/data/*-api.ts` (React Query), `gifts/url-state` (search params).
- UI: `gift-rows`, `gift-table`, `gift-list`, `list-view`, `*-page`, `*-editing`, `editing` folders (see [frontend.md](frontend.md#planned-feature-file-map)).
- Feature coordination: one controller hook per page that loads or changes data (`use-gift-list`,
  `use-archive-list`); the settings page composes two cards that each use the data hooks and their editor.
- Public exports: the four page components (imported by `App.tsx`) and `GiftsSummary` (home feature, own folder).

## React Web behaviour

- Router and lazy loading: no lazy loading (the app bundles all pages today); nested routes with `<Outlet />`.
- Browser history: tab links push; filter changes `replace`; reload and back restore tab and filters.
- Forms, focus, keyboard: dialogs are MUI `Dialog` (focus trap, Escape closes unless saving); the form submits on
  Enter; the first field gets focus on open; «Добавить вариант» moves focus to the new row's link field; removing a
  variant moves focus to «Добавить вариант». Variant remove buttons have `aria-label` «Удалить вариант N».
- Responsive: phone first; dialog full screen below `sm`; a table from `md`, a compact list below it.
- Existing UI components and tokens: MUI `Card`, `Chip`, `ToggleButtonGroup`, `TextField select`, `Menu`, `Collapse`,
  `Snackbar` + `Alert`; patterns copied from Expenses (`EntryDialog`, `RowActions`, `ListStatus`, `SectionTabs`,
  `KpiCards`).
- One style for lists and returning to the list: the section frame and tabs match Expenses (title, tabs, gear);
  lists follow the Expenses table/list too, with a coloured block per person («Gift lists», «Person colours» in
  design.md). Edits never leave the list
  (dialogs), so there is no detail page to return from.
- Theme: `src/app/theme/*`, light/dark/system; no mode checks in code, `theme.applyStyles('dark', …)` if needed.
- Phone and tablet: see [frontend.md, Visual system](frontend.md#visual-system-and-responsiveness).
- UI library: MUI 9 (`@mui/material`, Outlined icons), as the app uses and allows.
- Locale: Russian text; numbers and dates through `Intl` with `ru-RU` (`Intl.NumberFormat`, `Intl.DateTimeFormat`
  with `timeZone: 'Europe/Kyiv'` not needed for date-only values, which are parsed as calendar days).
- States: loading (spinner with `aria-label`), empty (designed, with the next step), error (reason + «Повторить»),
  write errors (in the dialog or a snackbar); no 401/403 states (no identity).

## State and API

- Local: dialog/editor state, form values, the open variants toggle per row, the quick-action error snackbar.
- Client: URL params (`recipient`, `category`).
- Server: React Query keys in [frontend.md](frontend.md#state-and-data); no copies in component state.
- Fetching: `apiCall(api.GET(..., { signal }))`; no client retry of 4xx; invalidation after writes.
- Validation and DTO mapping: form strings → request in the `*-form.ts` modules (trim, empty → `null`, price parsed
  from «1 234,5»); response items used as generated types.
- Session and 401/403: ➖ none. Safe errors: messages are the problem's `detail`/`title` in Russian wrappers; no raw
  stack or URL.

## Verification

- Targets: `npm test`, `npm run typecheck`, `npm run lint`, `npm run architecture:structure`, `npm run format:check`,
  `npm run contracts:check`.
- Test files: listed in [traceability.md](../traceability.md).
- Unit and page coverage: see [frontend.md, Test design](frontend.md#test-design).
- Theme, locale, breakpoints: checked in code review against design.md (tokens only, `useMediaQuery` breakpoints);
  no screenshot tests exist.
- Browser check: only on the user's explicit request.
- E2E: ➖ no stand in the project; page tests cover the main path.
- `EN-*` for missing tools: none.
