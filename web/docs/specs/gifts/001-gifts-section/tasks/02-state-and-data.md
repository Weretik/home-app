# Phase 02 — state and data

**Purpose:** pure presentation logic and URL state. Server reads and writes are in [07](07-api-integration.md).

## TS-001 — Labels and formatting

- **Task id:** TS-001
- **Covers:** SC-001, SC-016, SC-017 (and the Examples table)
- **Depends on:** none (enum string unions may be typed locally until EN-001, then switched to `Schema<>`)
- **Exact paths:** `web/src/features/gifts/data/labels.ts`; `web/src/features/gifts/data/formatting.ts`; test
  `web/src/features/gifts/data/formatting.test.ts`
- **Test level:** unit
- **Baseline:** [rules](../../../../rules.md) (one responsibility per file, no dump names); design.md amounts

### Work

- [ ] Test first: `formatPrice(null)` → «цена не указана», `formatPrice(1234.5)` → «1 234,50 ₴»; `formatDay('2027-03-12')`
      → «12 марта 2027» (calendar day, no time-zone shift); `variantLabel({ title: null, link: 'https://www.rozetka.com.ua/x' })`
      → «rozetka.com.ua»; `variantsCount(1|2|5)` → «1 вариант», «2 варианта», «5 вариантов».
- [ ] `labels.ts`: `recipientLabels` (Маша, Виталий, Вместе), `recipientOrder`, `priorityLabels` (🔥 Очень хочу,
      ⭐ Хочу, 💭 Когда-нибудь), `statusLabels` (Ждёт, Подарено, Не актуально).
- [ ] Implement `formatting.ts`.
- [ ] Responsibility check (module ≤200 lines).

### Evidence

- Test: `web/src/features/gifts/data/formatting.test.ts` (unknown price, kopecks, calendar day without «г.», occasion, variant host, plurals).
- Red (2026-11-26): `cd web && npx vitest run src/features/gifts` → failed, `./formatting` not resolved (written before the module).
- Green: same command → 6 files, 38 tests passed (whole gifts batch).
- Refactor: enum types live in `data/gifts-api.ts` (hand-written until EN-001), labels import them.
- Regression: `npm test` 23 files / 111 tests passed; `npm run typecheck`, `npm run lint`, `npm run architecture:structure` clean.
- Status: implemented.

### Done when

`formatting.test.ts` passes and both files are used by the card and the dialogs.

## TS-004 — Filters in the URL, filtering and grouping

- **Task id:** TS-004
- **Covers:** SC-002, SC-003, SC-004
- **Depends on:** TS-001
- **Exact paths:** `web/src/features/gifts/url-state/use-gift-filters.ts`; `web/src/features/gifts/url-state/gift-filters.ts`;
  test `web/src/features/gifts/url-state/gift-filters.test.ts`
- **Test level:** unit (pure); the hook is covered by the page test (TS-008)
- **Baseline:** [web ADR 0006](../../../../decisions/0006-section-pages-as-tabs-with-state-in-the-url.md)

### Work

- [ ] Test first: `parseFilters` ignores `recipient=Bob` and a category id not in the given categories;
      `applyFilters` keeps server order; `groupByRecipient` returns Maria, Vitaliy, Together, leaves out empty groups,
      counts items.
- [ ] `gift-filters.ts`: `parseFilters(params, categories?)`, `applyFilters`, `groupByRecipient`.
- [ ] `use-gift-filters.ts`: read/write `recipient`, `category` with `replace: true`, `clear()` removing both.
- [ ] Responsibility check (modules ≤200 lines).

### Evidence

- Test: `web/src/features/gifts/url-state/gift-filters.test.ts` (known/unknown values, categories not loaded, server order, group order).
- Red: `cd web && npx vitest run src/features/gifts` → failed, module not resolved. Green: passed. The hook is covered by the page test in TS-008.
- Regression: as TS-001.
- Status: implemented (hook not yet used by a page).

### Done when

`gift-filters.test.ts` passes; the hook keeps other params untouched.
