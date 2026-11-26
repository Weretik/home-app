# Name suggestions — final frontend code audit

- **Date:** 2026-11-26 · **Kind:** final feature audit (skill `architecture:frontend-code-audit`, feature review list)
- **Branch:** `claude/project-thread-g4hajn`, uncommitted working tree
- **SDD:** [README.md](../README.md) (FR-201…FR-206), backend rules
  [api 002](../../../../../../api/docs/specs/shopping/002-name-suggestions/requirements.md) (R-201…R-209)
- **Result:** no open blocker. 1 finding fixed, 4 accepted with a reason.

## Scope

`web/src/features/shopping/quick-add/**` (split into `entry/` and `name-suggestions/`),
`web/src/features/shopping/data/shopping-api.ts` (`useNameSuggestions`), the import paths in `list-page/ListPage.tsx`
and `keyword-settings/KeywordsCard.tsx`, `src/api-client/generated/api.ts` (regenerated), tests
(`QuickAdd.test.tsx`, `name-segment.test.ts`, `suggestion-caption.test.ts`, `ListPage.test.tsx`), docs
(`docs/architecture.md`, `docs/specs/README.md`, this SDD). Measured against `docs/rules.md`, `docs/design.md`,
`docs/architecture.md` and feature 001.

## Method

1. Read the SDD, the backend contract (`listShoppingNameSuggestions` in `api/docs/contracts/openapi.json`), web rules,
   design rules and the quick add of 001; read every changed production file.
2. Checked: folder axiom, file sizes, feature isolation, API only through `*-api.ts`, query keys under the module
   prefix, the Enter / option behaviour against MUI's `useAutocomplete` source (9.6.0, `case 'Enter'`), accessibility
   names, theme tokens, states.
3. Ran `npm run architecture:structure`, `format:check`, `lint`, `typecheck`, `test`, `build`.
4. No runtime smoke run in a browser (no browser automation here; the user asked for no screenshots): AF-W2-004.

## Step 0 — quality targets

The web half has no client targets; the backend's `api/docs/architecture/quality-targets.md` proposes a read
P95 / P99 of 300 / 800 ms (not measured). The suggestions call is one such read per 200 ms typing pause; the field
never waits for it (free text), so the API time affects only how soon the list appears. No performance verdict
without measured values (AF-W2-005, project-wide). Proposed, to confirm: suggestions visible ≤ 500 ms after typing
pauses on a mid phone over 4G (measure: Playwright trace on the deployed site).

## Findings

| Id        | Severity       | Rule / source                                                                                                                                                                                                         | Evidence                                                                                                                                                                                                                 | Status                                                                                                                                                                                                                                                                                                                              |
| --------- | -------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| AF-W2-001 | Medium         | FR-205 «Enter with no option highlighted submits»; MUI `useAutocomplete` (`case 'Enter'`): with a _programmatic_ highlight a freeSolo Enter commits the text and calls `preventDefault`, so the form would not submit | A first draft left `value` uncontrolled: MUI kept the last chosen option as the value and `syncHighlightedIndex` highlighted it again when it reappeared in the options, which turns Enter into a commit without submit. | **Fixed.** `NameField.tsx` controls `value={null}` and `autoHighlight={false}`, so nothing is highlighted unless the user moves to an option; tests «Enter without a highlighted suggestion adds what was typed» and «arrow keys pick a suggestion with Enter; the next Enter adds the text» (`quick-add/entry/QuickAdd.test.tsx`). |
| AF-W2-002 | Low            | One responsibility per request                                                                                                                                                                                        | After a choice the text changes to the chosen name, so one more suggestions request runs for it (`use-name-suggestions.ts`) although the list stays closed until the next keystroke.                                     | **Accepted.** A cheap read; its answer is already cached when the user keeps typing («Хлеб 2» starts a new query anyway).                                                                                                                                                                                                           |
| AF-W2-003 | Info           | Design rules: «Mark only problems in lists»                                                                                                                                                                           | The option shows an outlined chip «уже в списке».                                                                                                                                                                        | **Accepted.** Asked for by the user, and it is a warning (adding it again only changes its quantity), not an «OK» badge.                                                                                                                                                                                                            |
| AF-W2-004 | Low            | Feature review list: main path proved in a real browser, phone width                                                                                                                                                  | Behaviour is proved in jsdom (Testing Library, 5 cases) only; the popper's position inside the sticky card and the on-screen keyboard on a real phone were not exercised.                                                | **Accepted, to check by hand** after deploy: 360 px wide, type «мол», pick with a tap. MUI's own listbox keeps options ≥ 48 px tall below `sm`, and the popper takes the field's width. No end-to-end suite exists in the project (no Playwright in `package.json`).                                                                |
| AF-W2-005 | Info (project) | Step 0                                                                                                                                                                                                                | No client targets or measured API P95 in the repository.                                                                                                                                                                 | **Accepted for this feature**; project-wide, as recorded in the todos audit.                                                                                                                                                                                                                                                        |

## Checked and in order

- **Folders:** `quick-add/` was at six production files; the four new ones split it by meaning into `entry/` (5:
  `QuickAdd`, `use-quick-add`, `UndoSnackbar`, `DepartmentPreview`, `FrequentChips`) and `name-suggestions/` (6:
  `NameField`, `SuggestionOption`, `name-segment`, `suggestion-caption`, `use-name-suggestions`,
  `use-debounced-value`); `quick-add/` holds only the two subfolders; `data/` stays at 6. `architecture:structure`:
  15 rules, fine.
- **Sizes:** largest new component `NameField.tsx` 67 lines, `QuickAdd.tsx` 81, `shopping-api.ts` 93; all under the
  150 / 200 limits.
- **Boundaries:** no import from another feature; the API call is in `data/shopping-api.ts` through
  `platform/api.ts` and the typed client; type `NameSuggestion` from the regenerated schema.
- **State and data:** server state only in React Query; key `['shopping', 'suggestions', query]` under the module
  prefix, so an add refreshes `inList`; `enabled` only for a non-empty query; `keepPreviousData` against flicker; an
  error or an empty segment shows no list (`use-name-suggestions.ts`); the query is cut to the API's 80 characters.
- **Pure logic tested:** `name-segment.ts` (segment, bullets, empty segment, replacing with one space after the
  separator) and `suggestion-caption.ts` (Russian plural «раз / раза»).
- **Writes and repeated submits:** the feature adds no mutation; a choice only rewrites the text. The add keeps its
  `Idempotency-Key` per attempt and its pending lock (`QuickAdd.tsx` disables «Добавить» and ignores a submit while
  pending); Enter on an option is `preventDefault`ed by MUI, so it never submits.
- **Accessibility:** the field is a `combobox` named «Что купить» with a `listbox` of `option`s (tests query them by
  role); arrow keys and Escape from MUI; the department emoji is `aria-hidden`.
- **Design:** theme tokens only (`text.secondary`), outlined chip, body font; works on a phone (the field shrinks in
  the flex row: `minWidth: 0`).
- **Existing behaviour kept:** department preview, ⭐ chips, «Срочно», undo snackbar — `ListPage.test.tsx` passes
  with the field queried as a combobox.
- **Docs:** `docs/architecture.md` (tree, data, behaviour) and the specs index match the code; no new lasting
  decision, so no ADR.

## Verification (2026-11-26, after the fix)

| Check                                                       | Result                                                                       |
| ----------------------------------------------------------- | ---------------------------------------------------------------------------- |
| `npm run contracts:generate`                                | `NameSuggestion` and `/api/shopping/items/suggestions` in `generated/api.ts` |
| `npm run architecture:structure`                            | fine, 15 rules                                                               |
| `npm run format:check`, `npm run lint`, `npm run typecheck` | clean                                                                        |
| `npm test`                                                  | 80 files, 354 tests passed                                                   |
| `npm run build`                                             | built (only Vite's chunk-size advisory)                                      |
