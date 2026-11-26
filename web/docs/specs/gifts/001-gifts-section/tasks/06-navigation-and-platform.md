# Phase 06 — navigation and platform

## TS-006 — Section frame, tabs, routes

- **Task id:** TS-006
- **Covers:** SC-018
- **Depends on:** none
- **Exact paths:** `web/src/features/gifts/section/{GiftsSection.tsx,SectionTabs.tsx,gifts-pages.ts}`;
  `web/src/app/root/App.tsx`; cases in `active-page/GiftsPage.test.tsx` (tabs keep `recipient`/`category`, gear drops them)
- **Test level:** page
- **Baseline:** ADR 0006; design.md «Sections with several pages» (gear, not a tab)

### Work

- [ ] Test first: from `/gifts?recipient=Maria`, the «Подаренные» tab links to `/gifts/archive?recipient=Maria`;
      the gear links to `/gifts/settings`, has `aria-label` «Настройки» and `aria-current` when open.
- [ ] Section frame and tabs (copied pattern from Expenses, no month picker); routes: `/gifts` → `GiftsSection` with
      `index`, `archive`, `settings`; remove the `ComingSoonPage` line for gifts.
- [ ] Responsibility check.

### Evidence

- Test: `web/src/features/gifts/section/GiftsSection.test.tsx` (title and tabs, filters kept between tabs, gear without filters, `aria-current` on settings).
- Red: `cd web && npx vitest run src/features/gifts` → `./GiftsSection` not resolved. Green: 4 tests passed.
- Deviation: the three routes render `section/PageInProgress.tsx` until TS-008, TS-011, TS-012 replace it (no contract yet).
- Regression: as TS-001.
- Status: implemented (placeholder pages).

### Done when

The tab cases pass and `/gifts` no longer shows «Раздел скоро появится».

## TS-014 — Home Gifts summary

- **Task id:** TS-014
- **Covers:** SC-021, SC-022
- **Depends on:** EN-001
- **Exact paths:** `web/src/features/home/gifts-summary/data/{summary-api.ts,labels.ts,money.ts}`;
  `web/src/features/home/gifts-summary/view/{GiftsSummary.tsx,RecipientCards.tsx,TopWishesCard.tsx,OccasionsCard.tsx,DetailsCard.tsx}`;
  `web/src/features/home/page/summary-sections.ts`; tests `data/money.test.ts`, `view/GiftsSummary.test.tsx`,
  `web/src/features/home/page/HomePage.test.tsx`
- **Test level:** unit + page
- **Baseline:** [web ADR 0008](../../../../decisions/0008-home-page-as-a-registry-of-section-summaries.md) (own labels,
  no import from `features/gifts`); design.md figure cards and summaries

### Work

- [ ] Tests first: `money.test.ts` (whole hryvnias, «цена не указана»); `GiftsSummary.test.tsx` (requests
      `GET /api/gifts/summary`; four cards with counts, sums, «без цены», 🔥 and «подарено»; tops per person with link
      buttons; occasions; «Нюансы» read-only with no «⋮» and no add; «Изменить» → `/gifts/settings`; «Открыть
      подарки» → `/gifts`; empty and error states); `HomePage.test.tsx` («Подарки» is enabled, `?s=gifts` opens it).
- [ ] Data hook (key `['gifts', 'summary']`), view components, the registry line `Summary: GiftsSummary`.
- [ ] Responsibility check (components ≤150 lines).

### Evidence

- Red / Green / Refactor / Regression: —

### Done when

The three test files pass and `architecture:structure` reports no feature-to-feature import.
