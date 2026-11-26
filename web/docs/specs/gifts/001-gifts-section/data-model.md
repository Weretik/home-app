# Gifts section — data model

The server's shapes are the generated contract types (see [api-contract.md](contracts/api-contract.md)); the client
adds only presentation and form models.

| Model                | Owner / path                                       | Fields and invariants                                                                                                                                                                               | Consumers                                                      | Source       |
| -------------------- | -------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------- | ------------ |
| `GiftFormValues`     | `features/gifts/gift-editing/gift-form.ts`         | `recipient: Recipient \| ''`, `name`, `categoryId`, `priority: GiftPriority`, `price` (text), `link`, `occasion`, `occasionDate` (`YYYY-MM-DD` or `''`), `notes`, `variants: VariantValues[]` (≤20) | `GiftDialog`, `GiftFields`, `OccasionFields`, `VariantsEditor` | R-001, R-002 |
| `VariantValues`      | same                                               | `key` (client-only, for React lists and focus), `title`, `link`, `price` (text)                                                                                                                     | `VariantRow`                                                   | R-002        |
| `DetailFormValues`   | `features/gifts/detail-editing/detail-form.ts`     | `recipient`, `label` (≤100), `value` (≤200)                                                                                                                                                         | `DetailDialog`                                                 | R-009        |
| `CategoryFormValues` | `features/gifts/category-editing/category-form.ts` | `name` (≤100)                                                                                                                                                                                       | `CategoryDialog`                                               | R-010        |
| `GiftFilters`        | `features/gifts/url-state/gift-filters.ts`         | `recipient?: Recipient`, `categoryId?: string`; only known values                                                                                                                                   | list pages                                                     | R-006        |
| `GiftGroup`          | same                                               | `recipient`, `items` (server order kept), `priceSum` (known prices); groups in the order Maria, Vitaliy, Together; empty groups left out                                                            | `GiftTable`, `GiftList`                                        | R-005, R-006 |
| `EditorState<T>`     | `features/gifts/editing/use-editor-state.ts`       | `{ mode: 'add', preset? } \| { mode: 'edit', item } \| { mode: 'delete', item } \| null`                                                                                                            | pages, settings cards                                          | ADR 0007     |

## Transformations and lifecycle

- Input checks and defaults: priority `Normal`; recipient/category from the filters; empty optional texts → `null`;
  price text → number via `parsePrice` («1 234,5» → 1234.5; empty → `null`; 0, negative, >10 000 000 or >2 decimals
  → field error); links trimmed and checked `^https?://` with `URL` parsing; occasion date year 2000–2100.
- Normalisation and serialisation: `toGiftRequest(values)` trims, maps empties to `null`, drops the client `key` of
  variants, keeps variant order; `fromGift(item)` fills the form from a `GiftItem` (price → «1234,5» text).
- Lifecycle: form values live while the dialog is open; a closed and reopened dialog starts again (and gets a new
  idempotency key).
- Platform differences: none.
- Compatibility: none (new feature).
- Tests: `gift-form.test.ts` (round trip, null price, variants order, 20 limit), `field-checks.test.ts`,
  `gift-filters.test.ts` (unknown values, group order, server order inside groups), `detail-form.test.ts`,
  `category-form.test.ts`.
