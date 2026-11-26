# Gifts — HTTP contract

**Status:** agreed (routes fixed by the product brief; shapes decided here and generated into the machine contract)

The machine-readable contract is [`api/docs/contracts/openapi.json`](../../../../contracts/openapi.json), generated from
the endpoints on every build (ADR 0008). This page records the decisions; the schema lives only there. Consumer: the
web app, through `npm run contracts:generate`.

All routes are under `/api/gifts`, tag `Gifts`, open without sign-in (ADR 0015). Enums travel as strings. Every write
uses the `PublicWrite` rate limit and lists `429`.

| operationId | Method and route | Body / headers | Success | Errors |
| --- | --- | --- | --- | --- |
| `listGifts` | `GET /items?archived=false\|true` | — | `200` `GiftItem[]` | — |
| `createGift` | `POST /items` | `GiftRequest`; `Idempotency-Key` | `201` `CreatedGiftsEntry { id }`, `Location` | 400, 404 category, 409 key reused, 429 |
| `updateGift` | `PUT /items/{id}` | `GiftRequest`; `If-Match: <version>` | `204` | 400, 404 gift/category, 409 changed meanwhile, 429 |
| `deleteGift` | `DELETE /items/{id}` | — | `204` | 400, 404, 429 |
| `changeGiftStatus` | `PUT /items/{id}/status` | `{ status, giftedOn? }`; `If-Match` | `204` | 400, 404, 409, 429 |
| `listGiftCategories` | `GET /categories` | — | `200` `GiftCategoryItem[]` | — |
| `createGiftCategory` | `POST /categories` | `{ name }`; `Idempotency-Key` | `201` `{ id }` | 400, 409 name taken / key reused, 429 |
| `renameGiftCategory` | `PUT /categories/{id}` | `{ name }` | `204` | 400, 404, 409 name taken, 429 |
| `deleteGiftCategory` | `DELETE /categories/{id}` | — | `204` | 400, 404, 409 in use, 429 |
| `listGiftDetails` | `GET /details` | — | `200` `GiftDetailItem[]` | — |
| `createGiftDetail` | `POST /details` | `{ recipient, label, value }`; `Idempotency-Key` | `201` `{ id }` | 400, 409 key reused, 429 |
| `updateGiftDetail` | `PUT /details/{id}` | same body; `If-Match` | `204` | 400, 404, 409 changed meanwhile, 429 |
| `deleteGiftDetail` | `DELETE /details/{id}` | — | `204` | 400, 404, 429 |
| `getGiftsSummary` | `GET /summary` | — | `200` `GiftsSummary` | — |

Shapes (JSON names as generated). Every schema name of the module starts with `Gift`/`Gifts` (or is unique, such as
`UpcomingOccasion`), because two modules' types with one simple name merge into one OpenAPI schema; the
architecture test `ContractSchemaNameTests` enforces it.

- `GiftRequest`: `recipient`, `name`, `categoryId`, `priority`, `price?`, `link?`, `occasion?`, `occasionDate?`,
  `notes?`, `variants: GiftVariantRequest[]` = `[{ title?, link, price? }]` (an empty list or missing means none).
- `GiftItem`: `id`, `version`, `recipient`, `name`, `categoryId`, `categoryName`, `priority`, `price?`, `link?`,
  `status`, `giftedOn?`, `occasion?`, `occasionDate?`, `notes?`, `variants: GiftVariantItem[]` = `[{ id, title?, link, price? }]`,
  `createdAtUtc`.
- `GiftCategoryItem`: `id`, `name`, `sortOrder`. `GiftDetailItem`: `id`, `version`, `recipient`, `label`, `value`,
  `sortOrder`.
- `GiftsSummary`: `recipients: [{ recipient, counts, topWanted: [{ id, name, price?, link? }] }]` for `Maria`,
  `Vitaliy`, `Together` in that order; `total` (`counts`); `upcoming: [{ id, recipient, name, occasion?, occasionDate }]`;
  `details` (`GiftDetailItem[]`). `counts` = `{ wantedCount, wantedPriceSum, wantedWithoutPrice, highCount,
  giftedCount, giftedPriceSum }`.
- Dates are `yyyy-MM-dd`; `createdAtUtc` is ISO 8601; prices are JSON numbers with up to two decimals.

Errors are RFC 9457 problems with `code` (`gifts.*`) and, for validation, `errors` by field (ADR 0003).

Idempotency: creates take `Idempotency-Key` (≤ 100). Same key and data → the first `{ id }` with `201`; same key with
other data or another kind → `409` `gifts.idempotency_key_reused`. Edits and the status change are target values and
need no key; the version in `If-Match` (quoted or not, `W/` ignored) guards them; without it `400`.

Compatibility: a new tag and new operations only; nothing existing changes.
