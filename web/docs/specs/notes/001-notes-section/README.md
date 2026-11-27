# 001 — «Про нас» section (notes about each other)

- **App type:** React Web (Vite, MUI, React Query, React Router)
- **App:** `web/`, feature `src/features/notes`
- **Architecture source:** [architecture](../../../architecture.md), [rules](../../../rules.md), [design rules](../../../design.md)
- **Status:** implemented
- **Owner:** Vitaliy · **Created:** 2026-11-27 · **Updated:** 2026-11-27
- **Backend SDD:** [api/docs/specs/notes/001-notes-module](../../../../../api/docs/specs/notes/001-notes-module/README.md) (rules
  `R-*`, scenarios `SC-*`, contract)
- **Decision:** [web ADR 0017](../../../decisions/0017-notes-section-person-tabs-me-first.md)

A new section «Про нас» at `/notes`: what Masha and Vitaliy like, dislike and should know about each other, as cards
grouped by category, with tabs «Про мене» / «Про партнера» / «Усі», filters, search, a quick add, a full dialog and
the category settings. It is its own section of the menu, not a part of the profile; the profile page (identity
feature) only links to it. Written from the frontend SDD template (Weretik/obsidian `Templates/frontend-sdd`), React
variant, condensed.

## State

- Specification: accepted · Implementation: done · Verification: typecheck, lint, tests, format and build green;
  the structure check is green for `features/notes` · Blockers: none
- Deviations from the design below, forced by the six-files rule: the cards have their own `note-card/` (NoteCard,
  KindMark, PersonAvatar, the grid), the shared states their own `feedback/` (ListStatus, ResultSnackbar,
  ConfirmDeleteDialog), the quick add keeps its state in `note-list/use-quick-add.ts`, the tab strip is
  `section/NoteTabs.tsx` and `note-dialog/` holds its own `SegmentedField`. «змінив(ла)» is written by the person:
  «змінив Віталій», «змінила Маша».

## Requirements and behaviour

The product rules are the backend SDD's (R-001…R-014). The screens add these:

| ID     | Behaviour                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |
| ------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| FR-001 | `/notes` «Про нас» with three tabs kept in the URL (`?about=me` · `partner` · `all`): «Про мене», «Про Машу» or «Про Віталія» (the partner of the signed-in member, by name), «Усі». The default is «Про мене». When the signed-in member has no person in the household (`person: null`), the tabs are «Віталій», «Маша», «Усі» (`?about=vitaliy` · `maria` · `all`) and the default is «Усі». A gear next to the tabs opens `/notes/settings`.                                                                                                                                                                                                          |
| FR-002 | Filters on one wrapping row, kept in the URL and across the tabs: kind chips «Усе · Любить · Не любить · Факти» (`?kind=likes`…), category select (`?category=<id>` or `none`), search field (`?q=`, 300 ms debounce).                                                                                                                                                                                                                                                                                                                                                                                                                                    |
| FR-003 | Quick add on top of the list: a kind toggle (Любить · Не любить · Факт, «Любить» first) and a text field whose placeholder follows the tab («Що любить Маша?», «Що я люблю?», …); Enter adds a note about the tab's person with that kind and no category; in «Усі» a small person toggle (Віталій · Маша) appears before the field. The field clears and keeps focus; a pencil next to it opens the full dialog with the typed text.                                                                                                                                                                                                                     |
| FR-004 | The list: notes grouped by category in the API order, a heading per category with its emoji and count, «Без категорії» last; inside, cards in a grid (one column at 360 px, two on a tablet, three on a computer). A card: the kind mark (green «Любить» with a heart, red «Не любить» with a crossed circle, blue «Факт» with a lightbulb), the title, a star when important, the place as a chip with a shop icon, the description clamped to three lines, and a footer «Маша · 27.11» (who wrote it and when; «змінив(ла) Віталій» when the last editor differs). In «Усі» a person avatar (М / В) shows whom the note is about. Tap opens the dialog. |
| FR-005 | The dialog (full screen on a phone, 560 px on a computer), for new and existing notes: «Про кого» (segmented Віталій · Маша), «Що» (segmented Любить · Не любить · Факт), title, category select (with «Без категорії»), «Де купити / бренд» (place), description (multiline), «Важливо» switch. «Зберегти» creates with an `Idempotency-Key` or saves with `If-Match`; «Видалити» asks first. A 409 reloads the list and says the note changed meanwhile; a 404 closes the dialog and says it was deleted.                                                                                                                                               |
| FR-006 | `/notes/settings` «Категорії»: the list with emoji, name and note count; add, rename, change emoji and delete in dialogs; delete warns how many notes lose the category.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  |
| FR-007 | States: loading skeleton cards; an empty tab «Про Машу ще нічого немає. Додайте перше вище»; a filtered empty list offers «Скинути фільтри»; an error shows the message and «Повторити».                                                                                                                                                                                                                                                                                                                                                                                                                                                                  |
| FR-008 | The section is in the side menu («Про нас», a heart icon) on a computer, under the main five; on a phone it is linked from the home page next to the service pages and from the profile page. Everything works at 360 px without horizontal scroll, in both colour schemes, with design tokens only; texts in Ukrainian from the glossary.                                                                                                                                                                                                                                                                                                                |

## Design

### Structure (`src/features/notes`, at most 6 code files a folder)

```
notes/
  data/          notes-api.ts (list query), categories-api.ts, labels.ts (kinds, persons), use-me.ts (the
                 signed-in person and the partner from platform/member-api)
  writes/        note-writes-api.ts (create / update / delete), category-writes-api.ts, failures-api.ts
  section/       NotesSection (title, tabs, gear), notes-tabs.ts (tab ↔ person, labels)
  url-state/     filters.ts (URL ↔ API query), use-note-filters.ts
  note-filters/  NoteFilters, KindChips, CategorySelect, SearchField
  note-list/     NotesPage, NoteGroups, NoteCard, QuickAdd, ListStatus
  note-dialog/   NoteDialog, NoteFields, note-form.ts (values ↔ request, checks), use-note-form.ts
  settings/      CategoriesSettingsPage, CategoryRow, CategoryDialog, DeleteCategoryDialog, category-form.ts
```

### State and data

- Server state only in React Query. Keys start with `moduleKeys.notes` (`['notes']`): `['notes', 'items', query]`,
  `['notes', 'categories']`. Every write invalidates `['notes']`
  ([ADR 0009](../../../decisions/0009-writes-refresh-their-backend-module.md)).
- Creates send an `Idempotency-Key` (one per opened form or quick-add text, renewed after success); edits and deletes
  send `version` in `If-Match` ([ADR 0007](../../../decisions/0007-editing-in-dialogs-with-idempotency-and-versions.md)).
- The signed-in person comes from `platform/member-api` (`useSignedInPerson`), never from the identity feature.
- URL state with `useSearchParams` ([ADR 0006](../../../decisions/0006-section-pages-as-tabs-with-state-in-the-url.md)).

### Routes and shell

- `App.tsx`: `/notes` → `NotesSection` with index `NotesPage` and `settings`.
- `sections.ts`: «Про нас» (`/notes`) joins the side menu; the home page lists it for phones.

### Size limits

Component file ≤ 150 lines, hook or other module ≤ 200 lines, checked by `npm run architecture:structure`.

## Tasks and traceability

| ID     | Responsibility                                                                                                                  | Test                                               | Covers                                |
| ------ | ------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------- | ------------------------------------- |
| TS-101 | Regenerate the client from the contract                                                                                         | `npm run contracts:generate`, typecheck            | —                                     |
| TS-102 | `notes-tabs.ts` (tab ↔ person, labels with and without a person), `filters.ts`, `note-form.ts`, `labels.ts`, `category-form.ts` | Vitest unit                                        | FR-001, FR-002, FR-005                |
| TS-103 | Notes page: tabs, filters, groups, cards, quick add                                                                             | Testing Library with mocked fetch                  | FR-001…FR-004, FR-007, SC-001, SC-002 |
| TS-104 | Note dialog: create, edit, delete, 409                                                                                          | Testing Library                                    | FR-005, SC-003, SC-004                |
| TS-105 | Category settings                                                                                                               | Testing Library                                    | FR-006, SC-005                        |
| TS-106 | Routes, menu entry, home link, docs (architecture, ADR), structure check                                                        | `npm run architecture:structure`, lint, shell test | FR-008                                |

## Readiness

- [x] Behaviour, states and URLs defined; contract taken from the backend SDD.
- [x] Delivery: typecheck, lint, tests, format check and build green; the structure check passes for the feature
      (its one finding is `src/platform` at seven files, which this change did not add).
- [ ] Final frontend code audit: `audit.md`, findings fixed or accepted.
