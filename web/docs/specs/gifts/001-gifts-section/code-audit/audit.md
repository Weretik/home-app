# Code audit — Gifts section (final, feature scope)

- **Feature / SDD:** `web/docs/specs/gifts/001-gifts-section/`
- **Scope:** `web/src/features/gifts/**`, `web/src/features/home/gifts-summary/**`, the changed
  `web/src/app/root/App.tsx`, `web/src/features/home/page/{summary-sections.ts,HomePage.tsx,HomePage.test.tsx}`,
  the regenerated `web/src/api-client/generated/api.ts`, `web/docs/{architecture.md,design.md}`.
- **Date:** 2026-11-26
- **Rules:** root `AGENTS.md` (Boundaries); `web/docs/rules.md` (Structure, API, Writes, App, Tests);
  `web/docs/architecture.md`; `web/docs/design.md`; web ADRs 0002, 0003, 0006, 0007, 0008; the user's size rule
  (component ≤150 lines, hook/module ≤200), 2026-11-26.
- **Verdict:** **ready for review.** No open finding inside the feature; one project-level finding (AF-002) and one
  accepted duplication (AF-001) remain, neither blocks delivery.

## Step 0 — quality targets

The repository has no quality-targets document (searched `api/docs`, `web/docs`, `docs`). That is AF-002. Proposed,
to confirm: «Список» shows its first cards within 1 s on a mid-range phone over 4G at p75 (one request, ≤300 items),
API `GET /api/gifts/items` p95 ≤ 300 ms; measure with the browser's performance panel and App Service metrics. No
performance verdict is given without them.

## 00. Inventory

- [x] Read AGENTS.md, web architecture/rules/design, ADRs, the SDD, the contract; `git status` shows only this
      feature's files plus the backend agent's `api/` work (not in scope, untouched).
- [x] Stack in use: React 19, MUI 9, React Query 5, React Router 6, openapi-fetch with generated types, Vitest +
      Testing Library. No new package.

## 01. Architecture matrix

| Module                        | Role and paths                                                                                | Owner by project rules                                  | Allowed dependencies                                    | Finding |
| ----------------------------- | --------------------------------------------------------------------------------------------- | ------------------------------------------------------- | ------------------------------------------------------- | ------- |
| `features/gifts`              | the section: data, display, url-state, section, editing, card, list, pages, editors, settings | one feature folder per backend module (architecture.md) | `api-client` (only `*-api.ts`), `platform`, MUI, router | —       |
| `features/home/gifts-summary` | the home summary                                                                              | home feature (ADR 0008)                                 | its own `*-api.ts`; no `features/gifts` import          | —       |
| `app/root/App.tsx`            | routes only                                                                                   | app composition                                         | feature pages                                           | —       |

`npm run architecture:structure`: «Structure is fine.» No feature imports another (grep for `features/expenses`,
`../../gifts` in both folders: none).

## 02. State, API, DTO

| Flow            | Actual path                                                                       | Approved path                                            | Finding       |
| --------------- | --------------------------------------------------------------------------------- | -------------------------------------------------------- | ------------- |
| list reads      | `GiftsPage → useGiftList → useGifts → apiCall(api.GET)`                           | React Query hook in `*-api.ts` through `platform/api.ts` | —             |
| gift writes     | `GiftDialog → useEditForm → useSaveGift → Writer → api.POST/PUT`                  | same, ADR 0007                                           | —             |
| status change   | `CardActions → useGiftList.changeStatus → useChangeGiftStatus → api.PUT …/status` | same                                                     | fixed: AF-003 |
| settings writes | `DetailDialog`/`CategoryDialog → useEditForm → settings-writes-api`               | same                                                     | —             |
| home summary    | `GiftsSummary → useGiftsSummary → api.GET /api/gifts/summary`                     | own `*-api.ts` (ADR 0008)                                | —             |

- [x] Server data only in React Query; filters in the URL; form values only while a dialog is open.
- [x] `ApiError`, `apiCall`, `newIdempotencyKey` imported only by `gifts/data/*-api.ts` and `summary-api.ts`.
- [x] Generated types used through `Schema<>`; the forms map strings to the request in `gift-form.ts`,
      `detail-form.ts`, `category-form.ts`. No runtime response validation, as in the rest of the app.
- [x] Every GET passes `signal`; 4xx not retried (shared query client); invalidation per frontend.md.
- [x] No raw `fetch`/Axios outside tests (lint rule `no-restricted-globals` passes).

### Repeated submits (3c)

- [x] Creates (gift, category, «нюанс») send one `Idempotency-Key` per opened form and data: reused on retry, new
      after a data change, new after `gifts.idempotency_key_reused`, new after success (the dialog unmounts). Proven by
      `GiftsPage.test.tsx::adds a gift with variants, preset from the filter, with one key per data`.
- [x] Every submit is locked while pending (`EditDialog`, `DeleteDialog`, `useEditForm` guard).
- [x] Status changes are target-value `PUT`s with `If-Match`; the card's button **and** its «Не актуально» menu item
      are disabled while the change is sent (AF-003, fixed).
- [x] Mutations are never retried by the library (React Query default `retry: 0` for mutations).

## 03. UI and coordination

- [x] Each tab page composes one controller hook (`list-view/use-gift-list.ts`); cards, dialogs and settings cards
      receive values and callbacks.
- [x] Pure logic lives outside components: `display/formatting.ts`, `url-state/gift-filters.ts`,
      `editing/field-checks.ts`, `*-form.ts`.
- [x] One-responsibility signals: no component has more than one `useState`; one `useEffect` in
      `variant-editing/VariantRow.tsx` moves focus to a newly added row (a focus adapter, replaces the `autoFocus`
      that jsx-a11y forbids) — kept on purpose. `DetailsCard`/`CategoriesCard` call their data hook directly, like
      `expenses/settings-page/LimitsCard.tsx`; the web rules do not require a separate controller hook there.
- [x] Theme tokens only (no literal colours found); both schemes through MUI tokens; phone-first grids; icon-only
      buttons have `aria-label` and tooltips; shop links open with `rel="noopener noreferrer"`.

## 04. Structure and file sizes

Production files per folder (tests excluded): `data` 5, `display` 2, `url-state` 2, `section` 3, `editing` 6,
`gift-card` 4, `list-view` 5, `pages` 2, `gift-editing` 5, `variant-editing` 3, `settings-page` 4, `detail-editing` 3,
`category-editing` 3; `home/gifts-summary/data` 3, `view` 6. No folder mixes files and subfolders; no empty, wrapper
or one-file folder; no dump names.

File sizes: no production file comes within 30 lines of its limit (component 150, hook/module 200); measured with
`wc -l`, since the structure check does not enforce the limit yet. No baseline entries.

Decisions: keep `editing/` at six files (one frame per role); `GiftDialog` composes `GiftFields`, `OccasionFields`
and `VariantsEditor` to stay small.

## 05. Tests and tools

- [x] `npm run typecheck` 0 errors; `npm run lint` exit 0; `npm run architecture:structure` fine; `npm run format:check` clean; `npm test` 32 files / 162 tests passed (gifts + gifts summary: 15 files / 88 tests).
- [x] Unit tests for every pure module; page tests for both tabs, settings and the home summary; the main path
      (add → «Подарено» → archive → «Вернуть в список») is covered across `GiftsPage.test.tsx` and
      `ArchivePage.test.tsx`. No E2E stand exists in the project.
- [ ] `npm run contracts:check` compares with the committed file, so it passes only after the regenerated
      `api.ts` is committed together with this change (nothing is committed yet).
- Tests-first: unit, card, section, settings and summary tests were written first and seen failing; the two tab page
  tests were written right after the page code (recorded in TS-008 evidence).

## Findings

| ID     | Severity | Rule / source                                               | Evidence                                                                                                               | Fix                                                                     | Task                              | Status                    | Verification                                                                                |
| ------ | -------- | ----------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------- | --------------------------------- | ------------------------- | ------------------------------------------------------------------------------------------- |
| AF-001 | low      | rules.md «A feature never imports another feature»          | `gifts/editing/*`, `gifts/data/writes-api.ts`, `write-errors-api.ts` copy the Expenses editing frame and write helpers | keep; a shared editing frame would need an ADR and a change to Expenses | — (follow-up for the coordinator) | accepted                  | structure check passes                                                                      |
| AF-002 | low      | frontend-code-audit step 0                                  | no quality-targets document in the repository                                                                          | add targets (proposal above) in a project-level change                  | — (project)                       | open, outside the feature | —                                                                                           |
| AF-003 | low      | rules.md «Every mutation disables its submit while pending» | `gift-card/CardActions.tsx`: the «Не актуально» menu item stayed active while the card's status change was pending     | `RowAction.disabled`, set from `isPending`                              | TS-010                            | verified                  | `GiftCard.test.tsx::disables the status actions while its change is sent` (red, then green) |
| AF-004 | low      | jsx-a11y `no-autofocus` (eslint)                            | `VariantRow.tsx` used `autoFocus`                                                                                      | focus the new row's link in an effect on mount                          | TS-009                            | verified                  | `npm run lint` exit 0; `GiftsPage.test.tsx` checks focus on the new link                    |
| AF-005 | low      | design.md «Summaries on the home page» / SC-021             | no test opened the summary from `?s=gifts`                                                                             | added `HomePage.test.tsx::opens the gifts summary from ?s=gifts`        | TS-014                            | verified                  | test passes                                                                                 |
| AF-006 | low      | correctness                                                 | `priceText(100)` would have shown «10» (trailing-zero strip)                                                           | split whole and cents                                                   | TS-009                            | verified                  | `field-checks.test.ts::priceText` (red, then green)                                         |

## Pass 2 — gift lists and person colours (2026-11-26)

Scope: the change after the first review. Wish cards were replaced by a table (from `md`) and a compact phone list.
Each person now has a coloured block. Changed area: `gifts/gift-rows`, `gift-table`, `gift-list`,
`list-view/GiftRows.tsx` and `GiftFilters.tsx`, `display/recipient-tones.ts` and `RecipientAvatar.tsx`,
`display/formatting.ts`, `url-state/gift-filters.ts`, `pages/*`, `editing/RowActions.tsx`. The home summary's copy
of the colours belongs to a separate change and was not audited here.

- [x] Boundaries: no feature imports another feature. The person → colour mapping lives in one gifts file; home keeps
      its own copy (`home/gifts-summary/persons/person-colors.ts`), as AF-001 already accepts. No new API calls; the
      writes are unchanged.
- [x] Structure: production files per folder: `gift-rows` 4, `gift-table` 2, `gift-list` 2, `display` 4,
      `list-view` 5, `pages` 2. `gift-card/` and `list-view/GiftGroups.tsx` were removed; no folder is left empty
      or holding one file. The largest changed component is `GiftListRow.tsx` at 110 lines (limit 150).
- [x] Theme: there are no literal colours. Person colours come from `theme.vars.palette.<tone>` (`main`,
      `mainChannel`, `contrastText`). The `info` exception is recorded in design.md. The name is always written next
      to a colour, so colour is never the only hint.
- [x] Accessibility: - Each group is a `section` labelled by its `h2` on a phone, or a `th scope="rowgroup"` row in its own `tbody`
      in the table. - The quick status button has an `aria-label` and a tooltip. - The variants toggle has `aria-expanded` and `aria-controls`. - The avatars are `aria-hidden`.
- [x] Responsive: `useMediaQuery(breakpoints.up('md'))` chooses the table or the list, the same pattern as
      Expenses. The table sits in a `TableContainer`, so it scrolls inside itself, not the page.
- [x] Tests: gifts 14 files / 88 tests. The whole web suite has 34 files / 172 tests. typecheck, lint,
      format:check and architecture:structure all pass.

| ID     | Severity | Rule / source                               | Evidence                                                                                                           | Fix                                                                                 | Task   | Status   |
| ------ | -------- | ------------------------------------------- | ------------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------- | ------ | -------- |
| AF-007 | low      | design.md Accessibility «contrast ≥ 4.5:1»  | `list-view/GiftFilters.tsx`: a picked «Вместе» button wrote its text in `info.main` on a 0.12 tint (light ≈ 3.7:1) | text stays `text.primary` 600; the tint, the border and the circle carry the colour | TS-008 | verified |
| AF-008 | low      | design.md «Status colours mean only status» | «🔥 Очень хочу» used a `secondary` chip, which now means Маша                                                      | priority chips are neutral outlined; 🔥 is bolder                                   | TS-007 | verified |

Verdict: no open findings in the changed area. AF-001 and AF-002 stay as recorded above.
