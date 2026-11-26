# Phase 03 — React Web UI

**Purpose:** user interactions and visible states. Size rule for every file here: component ≤150 lines, hook or
module ≤200 lines; split along the seams in [frontend.md](../design/frontend.md#planned-feature-file-map).

## TS-005 — Editing frame and field checks

- **Task id:** TS-005
- **Covers:** SC-009, SC-012 (frame for SC-007, SC-010, SC-019, SC-020)
- **Depends on:** TS-003
- **Exact paths:** `web/src/features/gifts/editing/{EditDialog.tsx,DeleteDialog.tsx,RowActions.tsx,use-edit-form.ts,use-editor-state.ts,field-checks.ts}`;
  test `web/src/features/gifts/editing/field-checks.test.ts`
- **Test level:** unit (checks) + page (dialogs, via TS-009/TS-012)
- **Baseline:** ADR 0007; design.md «Editing entries»; AF-001

### Work

- [ ] Test first: `parsePrice` («1 234,5» → 1234.5, «» → null, «0», «-1», «12,345», «10000000,01» → invalid);
      `isHttpUrl` (http/https only, ≤2000); `checkLength`; `checkYear` 2000–2100.
- [ ] Frame components and hooks adapted from Expenses (`EntryDialog`, `DeleteEntryDialog`, `RowActions`,
      `useEntryForm`, `useEntryEditor`), with `add` taking a preset.
- [ ] Responsibility check.

### Evidence

- Tests: `web/src/features/gifts/editing/field-checks.test.ts`, `web/src/features/gifts/editing/EditDialog.test.tsx` (submit, error alert, pending lock; delete confirmation and its error).
- Red: `cd web && npx vitest run src/features/gifts` → modules not resolved. Green: passed.
- Deviation: `EditDialog` and `DeleteDialog` take the error as text (not a mutation), so they need no `ApiError`; `RowActions` takes `extra` actions, which replaces the planned `GiftMenu`.
- Remaining: `use-edit-form.ts` (needs the idempotency hook of TS-003).
- Status: partly implemented.

### Done when

`field-checks.test.ts` passes; no import from `features/expenses`.

## TS-007 — Gift rows: table and phone list

- **Task id:** TS-007
- **Covers:** SC-001, SC-014, SC-016, SC-017
- **Depends on:** TS-001
- **Exact paths:** `web/src/features/gifts/gift-rows/{GiftName.tsx,GiftActions.tsx,VariantLinks.tsx,GroupTitle.tsx}`,
  `gift-table/{GiftTable.tsx,GiftTableRow.tsx}`, `gift-list/{GiftList.tsx,GiftListRow.tsx}`,
  `display/{recipient-tones.ts,RecipientAvatar.tsx}`; tests `gift-table/GiftTable.test.tsx`, `gift-list/GiftList.test.tsx`
- **Test level:** component
- **Baseline:** design.md «Gift lists» and «Person colours»; the Expenses list (table from `md`, compact list below)

### Work

- [x] Tests first where practical: table columns per tab, a coloured header row per person with «N подарков · ≈
      сумма», variants sub-row, quick status button and «⋮» (Не актуально, Изменить, Удалить), pending lock, archive
      «Итог» and «Вернуть в список»; phone row texts, a block per person, menu order, archive outcome.
- [x] Components; `variant: 'active' | 'archive'` chooses the quick action and the columns; `aria-label`
      «Действия: {name}» on «⋮», «Подарено: {name}» on the quick button, «Варианты: N» on the count.
- [x] Responsibility check (every component ≤150 lines).

### Evidence

- Tests: `gift-table/GiftTable.test.tsx` (6), `gift-list/GiftList.test.tsx` (5), plus `display/formatting.test.ts`
  (`formatOutcome`, whole prices, group sums) and `url-state/gift-filters.test.ts` (group price sum).
- Red: `formatOutcome`, the whole-price, plural and group-sum cases failed before the code (4 failed, 18 passed).
  Deviation: the table and list component tests were written alongside the components, not after a failing run.
- Green: `npx vitest run src/features/gifts` → 88 passed.
- Refactor: the first version (wish cards, `gift-card/`) was replaced after the first review; the shared row parts
  went to `gift-rows/`. Status: done.

### Done when

Both component tests and the page tests pass; no literal colours (person colours come from theme tokens).

## TS-008 — «Список» page

- **Task id:** TS-008
- **Covers:** SC-001, SC-002, SC-003, SC-004, SC-005, SC-006
- **Depends on:** TS-002, TS-004, TS-006, TS-007
- **Exact paths:** `web/src/features/gifts/list-view/{GiftFilters.tsx,GiftRows.tsx,ListStatus.tsx}`;
  `web/src/features/gifts/active-page/{GiftsPage.tsx,use-gift-list.ts}`; test `web/src/features/gifts/active-page/GiftsPage.test.tsx`
- **Test level:** page
- **Baseline:** ADR 0006; design.md lists and empty states; controller hook per page

### Work

- [ ] Tests first: a block per person with its header and count; «Маша» filter writes `?recipient=Maria`, hides the blocks, sends no
      request; category filter; unknown params ignored; empty and empty-filtered states with «Сбросить фильтры»;
      loading label and error with «Повторить» refetching.
- [ ] Components and the controller hook.
- [ ] Responsibility check.

### Evidence

- Tests: `web/src/features/gifts/pages/GiftsPage.test.tsx` (12 cases; also covers TS-009 and TS-010).
- Deviation: the page and `list-view/` code were written just before this page test, not after a failing run; the
  pure modules it relies on were test-first. The page lives in `pages/` with `ArchivePage` (one shared controller
  `list-view/use-gift-list.ts`).
- Green: `npx vitest run src/features/gifts` passed. Status: done.

### Done when

`GiftsPage.test.tsx` SC-001…SC-006 cases pass.

## TS-009 — Gift add / edit / delete with variants

- **Task id:** TS-009
- **Covers:** SC-007, SC-008, SC-009, SC-010, SC-011, SC-012
- **Depends on:** TS-003, TS-005, TS-008
- **Exact paths:** `web/src/features/gifts/gift-editing/{GiftEditor.tsx,GiftDialog.tsx,GiftFields.tsx,OccasionFields.tsx,gift-form.ts}`;
  `web/src/features/gifts/variant-editing/{VariantsEditor.tsx,VariantRow.tsx,PriceField.tsx}`; tests
  `web/src/features/gifts/gift-editing/gift-form.test.ts`, cases in `active-page/GiftsPage.test.tsx`
- **Test level:** unit (form) + page
- **Baseline:** ADR 0007; design.md «Editing entries»

### Work

- [ ] Tests first: `gift-form.test.ts` (defaults from preset, `toGiftRequest` nulls and variant order, `fromGift`
      round trip, every field message of SC-009, the 20-variant limit); page: add with two variants sends `POST`
      with `Idempotency-Key` and the body; a retry with same data reuses the key, changed data renews it; edit sends
      `PUT` with `If-Match` and the replaced variant list; stale 409 shows the message and refetches; delete asks and
      sends `DELETE`.
- [ ] Components: `GiftDialog` composes `GiftFields`, `PriceField`, `OccasionFields`, `VariantsEditor`; focus moves to
      a new variant's link; remove buttons labelled.
- [ ] Responsibility check (`GiftDialog` ≤150 lines by composition).

### Evidence

- Red / Green / Refactor / Regression: —

### Done when

`gift-form.test.ts` and the SC-007…SC-012 page cases pass.

## TS-010 — Quick status actions

- **Task id:** TS-010
- **Covers:** SC-013, SC-014, SC-023
- **Depends on:** TS-003, TS-008
- **Exact paths:** `web/src/features/gifts/active-page/use-gift-list.ts` (or `use-status-change.ts` seam);
  `web/src/features/gifts/gift-rows/GiftActions.tsx`; cases in `GiftsPage.test.tsx`
- **Test level:** page
- **Baseline:** rules, Writes (submit disabled while pending)

### Work

- [ ] Tests first: «Подарено» sends `PUT …/status` `{ status: 'Gifted' }` with `If-Match`, is disabled while pending,
      the wish disappears after refetch; «Не актуально» sends `Dropped`; 429 shows the snackbar text.
- [ ] Status mutation with per-row pending state and a `Snackbar` error.
- [ ] Responsibility check.

### Evidence

- Red / Green / Refactor / Regression: —

### Done when

The SC-013, SC-014, SC-023 page cases pass.

## TS-011 — «Подаренные» page

- **Task id:** TS-011
- **Covers:** SC-015, SC-016
- **Depends on:** TS-008, TS-010
- **Exact paths:** `web/src/features/gifts/archive-page/{ArchivePage.tsx,use-archive-list.ts}`; test
  `web/src/features/gifts/archive-page/ArchivePage.test.tsx`
- **Test level:** page
- **Baseline:** ADR 0006, 0007

### Work

- [ ] Tests first: `archived=true` request; server order kept; «Подарено 26 ноября 2026» and «Не актуально» marks;
      filters; empty text «Здесь появятся подаренные подарки»; «Вернуть в список» sends `{ status: 'Wanted' }`;
      edit and delete reuse the gift editor.
- [ ] Page and controller hook (flat list, no groups).
- [ ] Responsibility check.

### Evidence

- Red / Green / Refactor / Regression: —

### Done when

`ArchivePage.test.tsx` passes.

## TS-012 — Settings: «Нюансы»

- **Task id:** TS-012
- **Covers:** SC-019
- **Depends on:** TS-002, TS-003, TS-005, TS-006
- **Exact paths:** `web/src/features/gifts/settings-page/{GiftSettingsPage.tsx,DetailsCard.tsx}`;
  `web/src/features/gifts/detail-editing/{DetailEditor.tsx,DetailDialog.tsx,detail-form.ts}`; tests
  `detail-editing/detail-form.test.ts`, `settings-page/GiftSettingsPage.test.tsx`
- **Test level:** unit + page
- **Baseline:** ADR 0007; design.md settings page

### Work

- [ ] Tests first: form checks (label 1–100, value 1–200, recipient required); page: grouped by person, add with a
      group's preset sends `POST /api/gifts/details` with `Idempotency-Key`, edit `PUT` with `If-Match`, delete asks;
      empty card text.
- [ ] Components.
- [ ] Responsibility check.

### Evidence

- Red / Green / Refactor / Regression: —

### Done when

The «Нюансы» cases in `GiftSettingsPage.test.tsx` and `detail-form.test.ts` pass.

## TS-013 — Settings: categories

- **Task id:** TS-013
- **Covers:** SC-020
- **Depends on:** TS-012
- **Exact paths:** `web/src/features/gifts/settings-page/CategoriesCard.tsx`;
  `web/src/features/gifts/category-editing/{CategoryEditor.tsx,CategoryDialog.tsx,category-form.ts}`; tests
  `category-editing/category-form.test.ts`, cases in `settings-page/GiftSettingsPage.test.tsx`
- **Test level:** unit + page
- **Baseline:** ADR 0007

### Work

- [ ] Tests first: name 1–100 trimmed; add/rename/delete requests; 409 `gifts.category_in_use` message stays in the
      delete dialog; duplicate name error under the field; categories and items queries refetch.
- [ ] Components.
- [ ] Responsibility check.

### Evidence

- Red / Green / Refactor / Regression: —

### Done when

The category cases pass.
