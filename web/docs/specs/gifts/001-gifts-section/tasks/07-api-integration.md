# Phase 07 — API integration

**Purpose:** the only files of `features/gifts` that call the API. Contract: [api-contract.md](../contracts/api-contract.md).

## TS-002 — Reads and query keys

- **Task id:** TS-002
- **Covers:** SC-001, SC-006, SC-016, SC-019, SC-020
- **Depends on:** EN-001
- **Exact paths:** `web/src/features/gifts/data/gifts-api.ts`; covered by `active-page/GiftsPage.test.tsx`,
  `archive-page/ArchivePage.test.tsx`, `settings-page/GiftSettingsPage.test.tsx`
- **Test level:** page (Testing Library with stubbed `fetch`)
- **Baseline:** [rules, API](../../../../rules.md#api): only `*-api.ts` imports the client, through `platform/api.ts`

### Work

- [ ] Page tests first assert `GET /api/gifts/items?archived=false` on «Список» and `archived=true` on «Подаренные».
- [ ] `giftKeys` (`all`, `items(archived)`, `categories`, `details`), `useGifts(archived)`, `useGiftCategories()`,
      `useGiftDetails()`, each with `signal`; export `GiftItem`, `GiftVariantItem`, `GiftCategoryItem`,
      `GiftDetailItem`, `Recipient`, `GiftPriority`, `GiftStatus` via `Schema<>`.
- [ ] Responsibility check (module ≤200 lines).

### Evidence

- Red / Green / Refactor / Regression: —

### Done when

The three page tests load data only through these hooks; `architecture:structure` passes.

## TS-003 — Writes, invalidation, idempotency, error text

- **Task id:** TS-003
- **Covers:** SC-008, SC-011, SC-020, SC-023 (used by SC-007…SC-015, SC-019)
- **Depends on:** EN-001
- **Exact paths:** `web/src/features/gifts/data/writes-api.ts`; `web/src/features/gifts/data/write-errors-api.ts`; test
  `web/src/features/gifts/data/write-errors-api.test.ts`
- **Test level:** unit (error text) + page (requests, headers, invalidation)
- **Baseline:** [web ADR 0007](../../../../decisions/0007-editing-in-dialogs-with-idempotency-and-versions.md);
  [rules, Writes](../../../../rules.md#writes); AF-001 (own copy, not imported from Expenses)

### Work

- [ ] Test first (`write-errors-api.test.ts`): no `ApiError` → «Нет связи с сервером…»; 409 `gifts.changed_meanwhile`
      and 412 → stale text; 404 → «…удалили в другом месте»; 409 `gifts.idempotency_key_reused` → `renewKey`;
      409 `gifts.category_in_use` → «Эта категория есть у подарков — сначала перенесите их в другую»; 429 → «Слишком
      много изменений подряд…»; 400 → field errors; other → «Не удалось {action}: …». `isStale` for 404/412/changed_meanwhile.
- [ ] `writes-api.ts`: `Writer<Body>` with `keys: QueryKey[]`; writers for gifts, categories, details;
      `useSaveGift`, `useDeleteGift`, `useChangeGiftStatus` (body `{ status }`, `If-Match`), `useSaveCategory`,
      `useDeleteCategory`, `useSaveDetail`, `useDeleteDetail`; invalidation per the table in frontend.md, also on a
      stale error; `useIdempotencyKey`.
- [ ] Responsibility check: if `writes-api.ts` passes 200 lines, move settings writers to `data/settings-writes-api.ts`
      (data then holds 6 files).

### Evidence

- Red / Green / Refactor / Regression: —

### Done when

`write-errors-api.test.ts` passes and page tests see the right method, path, `Idempotency-Key`/`If-Match` and a refetch
after each write.
