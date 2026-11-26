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

## TS-007 — Gift card

- **Task id:** TS-007
- **Covers:** SC-001, SC-014, SC-016, SC-017
- **Depends on:** TS-001
- **Exact paths:** `web/src/features/gifts/gift-card/{GiftCard.tsx,GiftChips.tsx,VariantLinks.tsx,CardActions.tsx}`;
  tested through `GiftsPage.test.tsx` and `ArchivePage.test.tsx`
- **Test level:** page
- **Baseline:** design.md cards, chips, accessibility; «Wish cards» pattern (TS-015)

### Work

- [ ] Page test first: a card shows name, «Украшения», «🔥 Очень хочу», «цена не указана», «День рождения · 12 марта
      2027», a «Открыть ссылку» link with `target=_blank`; «2 варианта» toggles a list of two links (title or host,
      price).
- [ ] Components; the card gets `variant: 'active' | 'archive'` to choose «Подарено» or «Вернуть в список» and the
      menu items; `aria-label` «Действия: {name}» on «⋮».
- [ ] Responsibility check (each component ≤150 lines; `CardActions.tsx` seam if needed).

### Evidence

- Test: `web/src/features/gifts/gift-card/GiftCard.test.tsx` (facts and chips, shop link in a new tab, variants toggle, «Подарено», «Не актуально» from the menu, pending lock, archive «Вернуть в список» and gifted day, dropped mark, recipient chip).
- Red: `cd web && npx vitest run src/features/gifts` → `./GiftCard` not resolved. Green: 6 tests passed.
- Refactor: the planned `GiftMenu` became `CardActions` (link, status button, `RowActions` with an extra item).
- Status: implemented at card level; the page assertions follow in TS-008/TS-011.

### Done when

The card assertions in both page tests pass in light and dark themes' token use (no literal colours).

## TS-008 — «Список» page

- **Task id:** TS-008
- **Covers:** SC-001, SC-002, SC-003, SC-004, SC-005, SC-006
- **Depends on:** TS-002, TS-004, TS-006, TS-007
- **Exact paths:** `web/src/features/gifts/list-view/{GiftFilters.tsx,GiftGroups.tsx,ListStatus.tsx}`;
  `web/src/features/gifts/active-page/{GiftsPage.tsx,use-gift-list.ts}`; test `web/src/features/gifts/active-page/GiftsPage.test.tsx`
- **Test level:** page
- **Baseline:** ADR 0006; design.md lists and empty states; controller hook per page

### Work

- [ ] Tests first: grouped headings with counts; «Маша» filter writes `?recipient=Maria`, hides headings, sends no
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
  `web/src/features/gifts/gift-card/GiftCard.tsx`, `CardActions.tsx`; cases in `GiftsPage.test.tsx`
- **Test level:** page
- **Baseline:** rules, Writes (submit disabled while pending)

### Work

- [ ] Tests first: «Подарено» sends `PUT …/status` `{ status: 'Gifted' }` with `If-Match`, is disabled while pending,
      the wish disappears after refetch; «Не актуально» sends `Dropped`; 429 shows the snackbar text.
- [ ] Status mutation with per-card pending state and a `Snackbar` error.
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
