# 002 — Name suggestions in the quick add

- **App type:** React Web (Vite, MUI, React Query)
- **App:** `web/`, feature `src/features/shopping` (the quick add of «Список»)
- **Architecture source:** [architecture](../../../architecture.md), [rules](../../../rules.md),
  [design rules](../../../design.md), feature [001](../001-shopping-list/README.md) (R-1 quick add, R-8 frequent)
- **Status:** implemented, audited
- **Owner:** Vitaliy · **Created:** 2026-11-26 · **Updated:** 2026-11-26
- **Backend SDD:** [api/docs/specs/shopping/002-name-suggestions](../../../../../api/docs/specs/shopping/002-name-suggestions/README.md)
  (rules R-201…R-209, scenarios SC-201…SC-207, contract `listShoppingNameSuggestions`)

While typing in «Что купить?», a list under the field suggests names from everything ever added, with the
department, how often it was taken and whether it is already in the list. Picking one puts the usual spelling into the
field. Written from the frontend SDD template (Weretik/obsidian `Templates/frontend-sdd`), React variant, condensed.

## State

- Specification: accepted · Implementation: done · Verification: local checks green · Audit:
  [code-audit/audit.md](code-audit/audit.md) · Blockers: none

## Requirements and behaviour

The data rules are the backend SDD's. The screen adds these:

| ID     | Behaviour                                                                                                                                                                                                                                                                  |
| ------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| FR-201 | Suggestions are for the **segment being typed**: the text after the last `,`, `;` or new line, without leading spaces, bullets or dashes. «молоко, хле» asks for «хле». An empty segment asks for nothing and shows no list.                                               |
| FR-202 | The request waits until typing pauses for 200 ms and the segment has at least one character; at most 8 suggestions. While a newer answer loads, the previous list stays (no flicker); a failed request shows no list and never blocks adding.                              |
| FR-203 | An option shows the department icon and the name on the first line, «Отдел · брали N раз» (or «ещё не брали») on the second, and an outlined chip «уже в списке» when `inList`. The matched part of the name is not highlighted (kept simple).                             |
| FR-204 | Choosing an option (click, tap, or arrow keys and Enter) replaces only the current segment with the suggested name and keeps everything before it: «молоко, хле» → «молоко, Хлеб». The cursor stays at the end, so a quantity can follow («Хлеб 2»); nothing is added yet. |
| FR-205 | Enter with no option highlighted submits the text as before (R-1); Escape closes the list; the list closes after a choice and after an add. The «Срочно» flag, the department preview and the ⭐ chips keep working.                                                       |
| FR-206 | Accessible: the field is a combobox named «Что купить» with a listbox of options; arrow keys move through them. On a phone the list opens under the field inside the sticky card's width, options are at least 48 px tall, and nothing scrolls sideways at 360 px.         |

## Design

### Structure

`quick-add/` had six production files; the field with suggestions adds five, so the folder splits by meaning into
two subfolders (folder rule):

```
quick-add/
  entry/             QuickAdd (the card: field, «Срочно», add, preview, error, ⭐), use-quick-add.ts, UndoSnackbar,
                     DepartmentPreview, FrequentChips
  name-suggestions/  NameField (freeSolo Autocomplete), SuggestionOption (one option), name-segment.ts (pure: the
                     segment, replacing it), suggestion-caption.ts (pure: «Хлеб · брали 3 раза»),
                     use-name-suggestions.ts (segment → debounced query), use-debounced-value.ts (moved)
```

`ListPage` imports from `quick-add/entry/`; `DepartmentPreview` and the keyword search import
`use-debounced-value` from `quick-add/name-suggestions/`.

### Data

- `data/shopping-api.ts` gets `useNameSuggestions(query)`: `GET /api/shopping/items/suggestions` with
  `{ query, limit: 8 }`, key `['shopping', 'suggestions', query]` (under the module prefix, so writes refresh `inList`),
  `enabled` only for a non-empty query, `staleTime` 30 s, `placeholderData: keepPreviousData` (FR-202). Type
  `NameSuggestion = Schema<'NameSuggestion'>` from the regenerated client.
- No write: choosing only changes the field's text; adding stays `use-quick-add`.

### Component

- `NameField` is a controlled MUI `Autocomplete` with `freeSolo`, `filterOptions={(x) => x}` (the server filters),
  `inputValue` = the whole text and `value` = `null` (the field is never "bound" to one option),
  `autoHighlight={false}` so Enter submits unless the user moved to an option (FR-205),
  `getOptionLabel = name`, `onChange` with a `NameSuggestion` → `replaceSegment(text, name)` (FR-204), `onInputChange`
  only for `reason === 'input'` (MUI's own `reset` after a choice is ignored, as the text is set by the choice).
  `open` follows MUI (opens on typing, closes on Escape, blur and choice) and only when there are options.
- The add form's `onSubmit` stays in `QuickAdd`; Enter on a highlighted option is handled by Autocomplete (it calls
  `preventDefault`), so it never submits.
- `SuggestionOption` renders one `<li>` with icon, name, caption and the chip; `slotProps.listbox` keeps the list
  within the card (the popper is the field's width).

### Pure logic (`name-segment.ts`)

- `currentSegment(text)` → `{ start, query }`: `start` is the index after the last separator, `query` the segment
  with leading spaces, `-`, `–`, `—`, `•`, `*` trimmed and trailing spaces trimmed.
- `replaceSegment(text, name)` → the text up to `start`, one space after a separator when there was text before it,
  then `name`. «молоко,хле» → «молоко, Хлеб»; «хле» → «Хлеб».

## Tasks and traceability

| ID     | Responsibility                                                          | Test                                                             | Covers         |
| ------ | ----------------------------------------------------------------------- | ---------------------------------------------------------------- | -------------- |
| TS-201 | Regenerate the client                                                   | `npm run contracts:generate`, typecheck                          | —              |
| TS-202 | `name-segment.ts`, `suggestion-caption.ts`                              | Vitest unit `name-segment.test.ts`, `suggestion-caption.test.ts` | FR-201, FR-204 |
| TS-203 | `useNameSuggestions`, `use-name-suggestions.ts`                         | through `QuickAdd.test.tsx` with mocked fetch                    | FR-202         |
| TS-204 | `NameField`, `SuggestionOption`, wiring in `QuickAdd`; split of folders | Testing Library `QuickAdd.test.tsx`, `ListPage.test.tsx`         | FR-203…FR-206  |
| TS-205 | Docs: `architecture.md`, specs index                                    | review, `npm run architecture:structure`, `format:check`         | —              |

Component tests (`quick-add/entry/QuickAdd.test.tsx`, the quick add with mocked fetch):

- typing «молоко, хле» asks `GET …/suggestions?query=хле&limit=8` and shows the option with «уже в списке»;
- choosing it makes the field «молоко, Хлеб» and sends nothing;
- arrow keys and Enter pick an option, the next Enter adds «Хлеб 2»;
- Enter without a highlighted option still adds the typed text;
- an empty segment («молоко, ») asks for nothing and hides the list.

`ListPage.test.tsx` keeps the existing add, preview and ⭐ cases with the field queried as a combobox.

## Readiness

- [x] Behaviour, states and keyboard defined; contract from the backend SDD; no new lasting decision (no ADR).
- [x] Delivery: contracts, structure, format, lint, typecheck, tests and build green locally.
- [x] Code audit: [code-audit/audit.md](code-audit/audit.md).
