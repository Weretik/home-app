# Gifts section — frontend design

## Clients and boundaries

- Client: `web/` only (React 19 SPA, Vite, MUI, React Query, React Router 6), [web ADR 0001](../../../../decisions/0001-single-vite-app-without-nx.md), [0002](../../../../decisions/0002-react-spa-mui-react-query.md).
- Variant: React Web, see [react.md](react.md).
- Code: new feature `web/src/features/gifts/`; new home summary `web/src/features/home/gifts-summary/`; one entry in
  `web/src/features/home/page/summary-sections.ts`; routes in `web/src/app/root/App.tsx`. Nothing in `src/app/shell`
  changes: `/gifts` is already in `mainSections`.
- Public API of the feature: the page components imported by `App.tsx` (`GiftsSection`, `GiftsPage`, `ArchivePage`,
  `GiftSettingsPage`); nothing else is imported from outside. `GiftsSummary` is imported only by the home registry.
- Shared behaviour across clients: none (one client).

## Architecture baseline

| Area                                  | Evidence                                                                    | Rule                                                                                                                                                   | Target                                                                                                                                                                                                                                                              | Finding        |
| ------------------------------------- | --------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------- |
| Folders                               | `web/tools/check-structure.mjs`                                             | [rules, Structure](../../../../rules.md#structure): ≤6 production files per folder, files or subfolders not both, no single-file folder, no dump names | every new folder 2–6 production files, tests beside them                                                                                                                                                                                                            | —              |
| File size (new user rule, 2026-11-26) | to be added to `check-structure.mjs` by a separate change                   | a **component file ≤150 lines**, a **hook or module file ≤200 lines**; split by responsibility, never by line count alone                              | every planned file below has one responsibility sized to fit; a file that grows past the limit is split along the seams named in the map                                                                                                                            | —              |
| Feature boundaries                    | `check-structure.mjs` (feature → feature imports fail)                      | [rules](../../../../rules.md#structure), [AGENTS.md, Boundaries](../../../../../../AGENTS.md)                                                          | `features/gifts` and `features/home/gifts-summary` import nothing from `features/expenses` or each other; the few labels/formatters they need exist in each (as [web ADR 0008](../../../../decisions/0008-home-page-as-a-registry-of-section-summaries.md) accepts) | AF-001 (below) |
| API access                            | `check-structure.mjs` (only `platform/` and `*-api.ts` import `api-client`) | [rules, API](../../../../rules.md#api)                                                                                                                 | only `gifts/data/gifts-api.ts`, `gifts/data/writes-api.ts`, `home/gifts-summary/data/summary-api.ts` import the client                                                                                                                                              | —              |
| Data/tests tools                      | `web/package.json` (Vitest, Testing Library, React Query 5, openapi-fetch)  | [rules, Tests](../../../../rules.md#tests-and-checks)                                                                                                  | React Query for server state; URL params for page state; fetch stubbed in page tests                                                                                                                                                                                | —              |

**AF-001 (accepted, no remediation now):** the editing frame of Expenses (`EntryDialog`, `DeleteEntryDialog`,
`RowActions`, `useEntryForm`, `useEntryEditor`, `useIdempotencyKey`, `describeWriteError`) cannot be imported from
`features/gifts`. Gifts keeps its own smaller copies in `features/gifts/editing/` and `features/gifts/data/`, adapted
to the `gifts.*` error codes. Moving them to a shared non-feature place would be a structure decision (an ADR and a
change to Expenses) and is out of scope; recorded as a follow-up for the coordinator, not a blocker.

## Audit of existing code that changes

| File                                             | Current role                                       | Keep or split                                                                                     | Target    | Task   |
| ------------------------------------------------ | -------------------------------------------------- | ------------------------------------------------------------------------------------------------- | --------- | ------ |
| `web/src/app/root/App.tsx`                       | the route table; `/gifts` renders `ComingSoonPage` | keep; replace the `/gifts` line with a nested `GiftsSection` route (index, `archive`, `settings`) | same file | TS-006 |
| `web/src/features/home/page/summary-sections.ts` | the summary registry; `gifts` has `Summary: null`  | keep; set `Summary: GiftsSummary`                                                                 | same file | TS-014 |
| `web/src/features/home/page/HomePage.test.tsx`   | asserts the switcher                               | keep; update the «скоро» expectation for gifts                                                    | same file | TS-014 |
| `web/src/features/home/page/ComingSoonPage.tsx`  | placeholder                                        | keep (still used by `/shopping`, `/todos`)                                                        | —         | —      |
| `web/docs/architecture.md`                       | routes table, folder trees                         | extend: Gifts section, `/gifts*` routes, home `gifts-summary/`                                    | same file | TS-015 |
| `web/docs/design.md`                             | design rules                                       | add the «Gift lists» and «Person colours» patterns (see below)                                    | same file | TS-015 |

## Planned feature file map

All paths are under `web/src/features/`. Every folder holds 2–6 production files; tests (`*.test.ts(x)`) sit beside
the file they cover. Line budgets: component ≤150, hook/module ≤200. **State after implementation (2026-11-26):
every row below exists**; the largest file is well under its budget. Changes against the first plan are listed after
the table.

| Path                                                                                                                                                                  | Responsibility                                                                                                           | Task           |
| --------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------ | -------------- |
| `gifts/data/gifts-api.ts`                                                                                                                                             | contract types via `Schema<>`, the query keys, `useGifts(archived)`, `useGiftCategories()`, `useGiftDetails()`           | TS-002         |
| `gifts/data/writes-api.ts`                                                                                                                                            | `Writer`, `ifMatch`, `useRefresh`, `useSave`, `useRemove`, `useIdempotencyKey`                                           | TS-003         |
| `gifts/data/gift-writes-api.ts`                                                                                                                                       | gift writer, `useSaveGift`, `useDeleteGift`, `useChangeGiftStatus`                                                       | TS-003, TS-010 |
| `gifts/data/settings-writes-api.ts`                                                                                                                                   | category and «нюанс» writers and hooks                                                                                   | TS-003         |
| `gifts/data/write-errors-api.ts` (+ test)                                                                                                                             | `describeWriteError`, `isStale` (named `*-api.ts` because only such files may import `ApiError`)                         | TS-003         |
| `gifts/display/labels.ts`, `formatting.ts` (+ test), `recipient-tones.ts` (+ test), `RecipientAvatar.tsx`                                                             | Russian labels; price, date, occasion, variant label, plurals; the person → theme colour mapping and the person's circle | TS-001         |
| `gifts/url-state/gift-filters.ts` (+ test), `use-gift-filters.ts`                                                                                                     | URL filters, filtering and grouping in memory                                                                            | TS-004         |
| `gifts/section/GiftsSection.tsx`, `SectionTabs.tsx`, `gifts-pages.ts` (+ `GiftsSection.test.tsx`)                                                                     | the section frame, tabs, gear                                                                                            | TS-006         |
| `gifts/editing/EditDialog.tsx`, `DeleteDialog.tsx` (+ `EditDialog.test.tsx`), `RowActions.tsx`, `use-edit-form.ts`, `use-editor-state.ts`, `field-checks.ts` (+ test) | the editing frame and field checks                                                                                       | TS-005         |
| `gifts/gift-rows/GiftName.tsx`, `GiftActions.tsx`, `VariantLinks.tsx`, `GroupTitle.tsx`                                                                               | what a table row and a list row share: name, link and notes; status and «⋮»; variant links; a person's group header      | TS-007         |
| `gifts/gift-table/GiftTable.tsx` (+ test), `GiftTableRow.tsx`                                                                                                         | the computer table, a coloured header row per person, variants in a sub-row                                              | TS-007         |
| `gifts/gift-list/GiftList.tsx` (+ test), `PersonBlock.tsx`, `ListRows.tsx`, `GiftListRow.tsx`                                                                         | the phone list, a coloured block per person, compact rows                                                                | TS-007         |
| `gifts/list-view/use-gift-list.ts`, `GiftFilters.tsx`, `GiftRows.tsx`, `ListStatus.tsx`, `StatusSnackbar.tsx`                                                         | the controller of a tab and its parts; `GiftRows` picks the table from `md`, the list below                              | TS-008, TS-010 |
| `gifts/pages/GiftsPage.tsx` (+ test), `ArchivePage.tsx` (+ test)                                                                                                      | «Список», «Подаренные»                                                                                                   | TS-008, TS-011 |
| `gifts/gift-editing/GiftEditor.tsx`, `GiftDialog.tsx`, `GiftFields.tsx`, `OccasionFields.tsx`, `gift-form.ts` (+ test)                                                | gift add / edit / delete                                                                                                 | TS-009         |
| `gifts/variant-editing/VariantsEditor.tsx`, `VariantRow.tsx`, `PriceField.tsx`                                                                                        | the variants block                                                                                                       | TS-009         |
| `gifts/settings-page/GiftSettingsPage.tsx` (+ test), `SettingsCard.tsx`, `DetailsCard.tsx`, `CategoriesCard.tsx`                                                      | «Настройки»                                                                                                              | TS-012, TS-013 |
| `gifts/detail-editing/DetailEditor.tsx`, `DetailDialog.tsx`, `detail-form.ts` (+ test)                                                                                | «нюанс» dialogs                                                                                                          | TS-012         |
| `gifts/category-editing/CategoryEditor.tsx`, `CategoryDialog.tsx`, `category-form.ts` (+ test)                                                                        | category dialogs                                                                                                         | TS-013         |
| `home/gifts-summary/data/summary-api.ts`, `labels.ts`, `formatting.ts` (+ test)                                                                                       | the summary read and its own labels and formatting                                                                       | TS-014         |
| `home/gifts-summary/overview/GiftsSummary.tsx` (+ test), `SummaryContent.tsx`, `NoWishes.tsx`                                                                         | the summary's states and layout (figures, «🔥 Очень хочу» full width, then occasions and «нюансы»)                       | TS-014         |
| `home/gifts-summary/cards/SummaryCard.tsx`, `FigureCard.tsx`, `RecipientCards.tsx`, `OccasionsCard.tsx`, `DetailsCard.tsx`                                            | the summary cards; person cards with a top border in the person's colour                                                 | TS-014         |
| `home/gifts-summary/persons/person-colors.ts` (+ test), `PersonAvatar.tsx`                                                                                            | the home copy of the person → theme colour mapping and the person's circle                                               | TS-014         |
| `home/gifts-summary/top-wishes/TopWishesCard.tsx`, `PersonWishes.tsx`                                                                                                 | «🔥 Очень хочу»: a tinted block per person                                                                               | TS-014         |
| `app/root/App.tsx` (existing)                                                                                                                                         | `/gifts` → `GiftsSection` with `GiftsPage`, `archive` → `ArchivePage`, `settings` → `GiftSettingsPage`                   | TS-006         |
| `home/page/summary-sections.ts`, `HomePage.test.tsx`, `HomePage.tsx` (existing)                                                                                       | registry line, «Подарки» enabled, comment                                                                                | TS-014         |

Changes against the first plan:

- Labels and formatting moved to `display/` so `data/` holds only API files; writes split into `writes-api`,
  `gift-writes-api`, `settings-writes-api`; `write-failure.ts` became `write-errors-api.ts` (structure check).
- One controller `list-view/use-gift-list.ts` serves both tabs; the two pages share the folder `pages/` instead of
  `active-page/` and `archive-page/` (each would have held one file).
- `GiftMenu` became `CardActions` with `RowActions` taking extra items; `SettingsCard` frames both settings cards;
  `SummaryCard` frames the home summary cards; the home `money.ts` is `formatting.ts`.
- The temporary `section/PageInProgress.tsx` was removed when the pages were wired.
- After the first review (2026-11-26) the wish cards (`gift-card/`, `list-view/GiftGroups.tsx`) were replaced by
  `gift-table/` and `gift-list/` sharing `gift-rows/`, with `list-view/GiftRows.tsx` choosing by width; person colours
  came with `display/recipient-tones.ts` and `RecipientAvatar.tsx`; `groupByRecipient` now adds the group's price sum.

## Architecture questions for a new feature

- Goal and user: [overview](../requirements/overview.md#goal) and `SC-*` in Given/When/Then.
- Non-functional target: up to ~300 wishes per tab without paging, one request per tab, filtering in memory
  ([overview, NFR](../requirements/overview.md#non-functional-requirements)); the project has no other quality targets
  for web pages (system default).
- Access rights: ➖ the app has no identity yet (API ADR 0015); writes are rate limited by the API like Expenses.
- Main path end to end: ➖ no browser stand; covered by page tests (see Test design).
- Double submit: every submit disabled while pending; creates carry an `Idempotency-Key`; edits carry `If-Match`.
- Personal data in logs or analytics: ➖ the app has no analytics or client logging; sizes and wishes stay in API
  responses only.

## State and data

- Server state: React Query only ([web ADR 0002](../../../../decisions/0002-react-spa-mui-react-query.md)). Keys:
  `['gifts', 'items', { archived }]`, `['gifts', 'categories']`, `['gifts', 'details']`, home `['gifts', 'summary']`.
- Page state: the tab is the route; `recipient` and `category` are URL params ([web ADR 0006](../../../../decisions/0006-section-pages-as-tabs-with-state-in-the-url.md)).
  Filtering and grouping are client-side over the one list (the API has no filter params; ≤ a few hundred rows), so
  a filter change sends no request.
- Dialog state: `use-editor-state` in the page controller (component state, not server data). Form values in
  `use-edit-form`. No global store.
- Invalidation (every success, and a stale 404/conflict error): every Gifts write (gift create / update / delete /
  status, category create / rename / delete, detail create / update / delete) invalidates the whole Gifts prefix
  `moduleKeys.gifts` (`['gifts']`), per web ADR 0009. Delivered first with narrower keys per write; aligned with the
  ADR when the branches merged (same data shown, a few more refetches after a write).

- Loading, empty, error: `list-view/ListStatus.tsx` per tab; cards in settings show their own spinner/error; home
  summary like `ExpensesSummary`. Stale data: refetch on invalidation only (the query client defaults); no polling.
- Retry: the query client does not retry 4xx; a user retry is «Повторить». Cancellation: `signal` passed to every GET.

## Navigation and deep links

| URL               | Page                          |
| ----------------- | ----------------------------- |
| `/gifts`          | «Список» (index)              |
| `/gifts/archive`  | «Подаренные»                  |
| `/gifts/settings` | «Настройки» (gear, not a tab) |
| `/?s=gifts`       | home with the Gifts summary   |

- Params on `/gifts` and `/gifts/archive`: `recipient` ∈ `Maria | Vitaliy | Together`, `category` = a category id.
  An unknown recipient is ignored at once; a category id not among the loaded categories is ignored once they load.
  Filter changes use `replace` (like Expenses), tab links push history.
- Switching between «Список» and «Подаренные» keeps both params; the gear drops them.
- Unknown sub-paths fall to the app's catch-all redirect to `/`.
- No access checks (the app has no identity yet; API ADR 0015).

## Visual system and responsiveness

- Theme «Синий плащ», tokens only, light and dark both designed ([design rules](../../../../design.md)).
- Gift lists (design.md «Gift lists», changed 2026-11-26 after the first review: cards were inconvenient): the tabs
  follow the Expenses pattern, a table from `md` and a compact list below it, each row ending with «⋮». Table columns:
  Подарок (name, link icon, notes), Варианты (count → a sub-row with the links; moved next to the name on review),
  Категория, Приоритет («Список»), Цена, Повод or Итог, the quick status button and «⋮». Phone: name and price, the category, small chips (priority or
  outcome, occasion), «N вариантов» → a `Collapse`; every action in «⋮».
- Person colours (design.md «Person colours», an explicit exception to «info is status only», like signed amounts):
  Виталий `primary`, Маша `secondary`, Вместе `info`, in `gifts/display/recipient-tones.ts`; the home summary keeps its
  own copy (`home/gifts-summary/persons/person-colors.ts`) because features never import each other. Without a person
  filter both tabs group by person: a header with the person's circle (`<tone>.main`, «В»/«М»/two people), the name,
  «N подарков · ≈ сумма», on a 0.08 tint, and a 4px line in the colour along the group. The «Для кого» filter buttons
  carry the same circle and a 0.12 tint when picked. No «Для кого» column: blocks or the filter already say who.
- Priority chip: neutral outlined, «🔥 Очень хочу» bolder (crimson now means Маша); the emoji carries the meaning.
  «Не актуально» is a default outlined chip.
- Price: «1 625 ₴» when whole, real kopecks kept («1 234,50 ₴»), tabular, right-aligned; group sums round.
- Phone (360–400px): filters wrap (recipient toggle scrolls sideways like the home switcher), the dialog is full
  screen, variant rows stack link over title/price, long names and links wrap (`overflowWrap: 'anywhere'`), no
  horizontal page scroll. Computer: the table.
- Risks: long variant URLs (shown as host, full URL only in `href`), many chips on one phone row (wrap), 20 variant rows in
  a dialog (the dialog content scrolls; «Добавить вариант» stays at the block's end).

## Platform adapters and permissions

➖ None: links open with `target="_blank" rel="noopener noreferrer"`; no storage, camera or clipboard.

## API integration

See [contracts/api-contract.md](../contracts/api-contract.md). Types come from `Schema<'…'>` of the regenerated
client; components get the generated item types directly (the app has no separate domain model for lists, as in
Expenses); the forms map their string values to the request type in `gift-form.ts`, `detail-form.ts`,
`category-form.ts`.

## Test design

- Risks: wrong request body (variants list, null price), lost idempotency key, stale edit, a wish that does not move
  between tabs, filters that drift from the URL, the summary showing an unknown price as 0.
- Levels: unit (Vitest) for every pure module (`formatting`, `write-errors-api`, `gift-filters`, `field-checks`,
  `gift-form`, `detail-form`, `category-form`, `money`); page tests (Testing Library + stubbed `fetch`, like
  `ExpensesPage.test.tsx`) for `GiftsPage`, `ArchivePage`, `GiftSettingsPage`, `GiftsSummary`, and the updated
  `HomePage.test.tsx`.
- Existing targets: `npm test`, `npm run typecheck`, `npm run lint`, `npm run architecture:structure`,
  `npm run format:check`, `npm run contracts:check`.
- E2E: ➖ the project has no browser test stand; the main path (open list → add → «Подарено» → see it in the archive)
  is covered at page level across `GiftsPage.test.tsx` and `ArchivePage.test.tsx`. No `EN-*` for E2E.

## Responsibilities in scope

Data and writes: `gifts/data`. URL state: `gifts/url-state`. Generic editing: `gifts/editing`. UI: table, list,
pages, editors as in the map. Navigation: `gifts/section` + `App.tsx`. Home: `home/gifts-summary` + registry.
Verification: tests beside each file, TS-016.

## Allowed tools and forbidden shortcuts

- Allowed: React Query hooks in `*-api.ts`, `apiCall` + `api` from `platform/api.ts`, `Schema<>` types,
  `useSearchParams`, MUI components with theme tokens, `newIdempotencyKey()` from `api-client`.
- Forbidden: raw `fetch`/Axios; importing from `features/expenses` or between `features/gifts` and
  `features/home`; copying server data into component state beyond the open form; literal colours; editing
  `api-client/generated/`; hand-written DTO types for contract shapes.
- Reuse checked: `features/expenses/data/writes-api.ts`, `entry-editing/*`, `entry-dialogs/*`,
  `month-view/ListStatus.tsx`, `url-state/use-filter-param.ts`, `section/SectionTabs.tsx`,
  `home/expenses-summary/view/KpiCards.tsx` — patterns copied and adapted, not imported (AF-001).

## Chosen approach and significant tools

No new packages or approaches. Everything follows existing web ADRs 0002, 0003, 0006, 0007 and 0008; no new web ADR
is needed. The only new rule applied is the user's file-size limit (component ≤150, hook/module ≤200 lines), which a
separate change adds to the structure check.

## Assumptions

Decided without asking; each is cheap to change.

1. **URL scheme** follows the repo, not the brief's `?tab=`: tabs are nested routes (`/gifts`, `/gifts/archive`) and
   settings is the gear page `/gifts/settings` ([design rules](../../../../design.md#sections-with-several-pages-and-month-lists)).
2. **Edit conflict** is `409` with code `gifts.changed_meanwhile` (API ADR 0017 convention, confirmed in the backend's
   `GiftErrors.cs`), not `412`; the client also treats a `412` as the same stale state, defensively.
3. **Other codes**: `gifts.idempotency_key_reused` (409, renew the key), `gifts.category_in_use` (409 on category
   delete), `gifts.category_name_taken` (409, shown under the name field), unknown ids `404` (`gifts.gift_not_found`,
   `gifts.category_not_found`, `gifts.detail_not_found`); all present in the backend's `*Errors.cs`. Unknown 409 codes
   fall back to the server's message.
4. **Status change** (`PUT /api/gifts/items/{id}/status`) sends the gift's version in `If-Match`; it sends no
   `giftedOn`, so the server sets today in Kyiv. Returning to the list sends `status: Wanted`.
5. **Category and «нюанс» order** is the server's; requests carry no `sortOrder` (the server appends). Reordering is out of scope.
6. **Category rename and delete** send no `If-Match` (categories carry no version, confirmed by the backend SDD);
   detail edits send `If-Match`.
7. **Variant request** items carry no id: `{ title?, link, price? }`, the order of the array is the order shown.
8. **Edit dialog** does not change status; status changes only through the row actions.
9. **Default recipient** of a new gift or «нюанс» is the recipient filter (or the group's «+» in settings), else
   empty and required. Default category: the category filter, else empty and required. Default priority: `Normal`.
10. **Archive actions**: «Вернуть в список» is the row's quick button (in «⋮» on a phone); a gifted wish can be marked «Не актуально» only
    by returning it first (keeps the menu small).
11. **No undo snackbar** after «Подарено»: the archive's «Вернуть в список» reverses it; failures of a quick action
    show in a `Snackbar` with an error `Alert`.
12. **Home summary order**: the registry keeps today's switcher order (Расходы, Дела, Покупки, Подарки); `?s=gifts`
    opens it. Summary blocks: figure cards → «🔥 Очень хочу» → «Ближайшие поводы» → «Нюансы».
