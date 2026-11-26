# Final frontend code audit — «Дела» (todos)

- **Date:** 2026-11-26 · **Kind:** final feature audit (skill `architecture:frontend-code-audit`, checklists
  `Architecture/Checklists/frontend/checks/feature.md` (T21) and `ci.md`)
- **Branch:** `claude/project-thread-7oue5f`, uncommitted working tree
- **SDD:** [README.md](README.md) (FR-001…FR-010), backend rules
  [api/docs/specs/todos/requirements.md](../../../../../api/docs/specs/todos/001-todos-module/requirements.md) (R-001…R-016)

## Scope

`web/src/features/todos/**`, `web/src/features/home/todos-summary/**`, `web/src/app/root/App.tsx`,
`web/src/features/home/page/summary-sections.ts`, `web/docs/architecture.md`, `web/docs/decisions/0011-*`,
`web/docs/specs/todos/`. Measured against `web/docs/rules.md`, `web/docs/design.md`, `web/docs/architecture.md`, ADR
0006/0007/0011 and the Expenses feature as the reference implementation.

## Method

1. Read the SDD, backend rules, web rules, design rules, architecture and ADRs; read every production file in scope
   and the generated contract (`api/docs/contracts/openapi.json`) for headers and responses of each todos operation.
2. Checked: folder axiom (≤ 6 production files, no mixed or one-file folders), file sizes (components ≤ 150 lines,
   hooks/modules ≤ 200), feature isolation, API only through `*-api.ts`, React Query keys and invalidation,
   `Idempotency-Key` creation/reuse/renewal, `If-Match` on every edit, optimistic done with rollback, pending locks,
   loading/empty/error states, 404/409 handling, Kyiv today for the due groups, theme tokens, icon-button
   `aria-label` + tooltip, phone layout.
3. **Runtime smoke run** (local only, nothing saved to the repository): API (`dotnet run`, Development) against a
   throwaway `postgres:17-alpine` container, Vite dev server with `VITE_API_URL=http://localhost:5000`, Playwright
   (Chromium) at 360×780 and 1280×800 in light and dark (`prefers-color-scheme`). Each run: open `/todos`, quick-add
   a long title, open it, add two steps, tick one, set status «В работе», «Сегодня», «Важно», save; tick done and
   «Отменить»; tick done again; «Архив» → «Вернуть»; «Маша» chip + a search with no hits → «Сбросить фильтр»;
   `/todos/settings` add / rename / delete a category; `/?s=todos` and tick the task in «В фокусе». Collected console
   errors and warnings, failed requests and HTTP ≥ 400, and `scrollWidth > clientWidth` on every page. A separate run
   provoked a 409: the task was changed through the API while its dialog was open, then saved.
4. Fixed the clear findings, added tests, re-ran the smoke run and the full check set.

## Step 0 — quality targets

The repository has no quality-targets document (no Core Web Vitals, screen load time or API P95/P99 targets), so no
performance verdict is given (AF-001). Proposed, to confirm: `/todos` first usable list ≤ 2 s on a mid phone over 4G
(measure: Playwright trace / Lighthouse mobile on the deployed site), done tick visible ≤ 100 ms (optimistic, already
met), API `GET /api/todos/tasks` P95 ≤ 300 ms (App Service metrics).

## Findings

| ID     | Severity | Rule / source                                                            | Evidence (file:line, before the fix)                                                                                                                                                                                                                                                                          | Status                                                                                                                                                                                                          |
| ------ | -------- | ------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| AF-001 | Low      | Audit step 0, quality targets                                            | No quality-targets document in `api/docs` or `web/docs`.                                                                                                                                                                                                                                                      | Accepted: project-wide, outside this feature; proposed numbers above.                                                                                                                                           |
| AF-002 | High     | FR-004, ADR 0011 (undo restores the previous status), R-010              | `todos/task-list/use-done-tick.ts:23-30`: «Отменить» started `GET` + `PUT status` at once, even while the tick's `PUT` was still in flight: it could read the pre-tick version and race the tick (the undo or the tick then fails with 409), and it also ran after a failed tick.                             | **Fixed**: the undo waits for the tick's promise and does nothing when the tick failed. Test ««Отменить» waits for the tick to be saved…» in `ActiveTasksPage.test.tsx`.                                        |
| AF-003 | Medium   | web rules «API» (show the problem's message in Russian), FR-008, FR-009  | `home/todos-summary/view/FocusList.tsx:36`: a failed tick in «В фокусе» showed the raw API detail (English, e.g. «Someone changed or deleted this…»).                                                                                                                                                         | **Fixed**: `describeTickError` in `todos-summary-api.ts` maps 404/409/429 and no connection to Russian. Test «says in Russian when a focus task changed elsewhere».                                             |
| AF-004 | Medium   | FR-007, FR-009, ADR 0007 (stale edit reloads and says so)                | `todos/writes/failures-api.ts:32-57`: a category 409 said «Задачу изменили…», a category 404 said «Этой категории уже нет: выберите другую» or «Этой задачи уже нет».                                                                                                                                         | **Fixed**: `describeWriteError(error, action, 'category')` names the category; the settings dialogs pass it. Unit tests `writes/failures-api.test.ts`.                                                          |
| AF-005 | Medium   | ADR 0007 (`If-Match` with the version the screen shows), SC-005 analogue | `todos/settings/CategoriesSettingsPage.tsx:104-110`: the edit and delete dialogs kept the category object captured when they opened, so after a 409 (the list reloads) every retry sent the same stale version and failed again until the dialog was closed.                                                  | **Fixed**: the dialogs get the category as the reloaded list holds it now (`latest(...)`); the typed values stay.                                                                                               |
| AF-006 | Medium   | FR-010, design «Pages» (phone first)                                     | `todos/task-dialog/SegmentedField.tsx:39-45`: `fullWidth` + `flexWrap` made every option 100 % wide once the row wrapped, so at 360 px «Приоритет» was four stacked full-width buttons with broken group borders (smoke screenshot).                                                                          | **Fixed**: a grid of separate bordered buttons, two columns for four options on a phone, one row from `sm`; checked again at 360 px light and dark.                                                             |
| AF-007 | Low      | Hard limits (component ≤ 150, module ≤ 200 lines), SDD «Size limits»     | `docs/specs/todos/README.md` said the limits are checked by `tools/check-structure.mjs`; the script checks folders, imports and the API boundary only. Measured by hand: largest component `TaskDialog.tsx` 139, largest module `task-form.ts` 114 / `use-task-form.ts` 108; every file is within the limits. | Fixed (documentation); adding a line-count rule to the structure check is recommended as a separate, repository-wide change.                                                                                    |
| AF-008 | Low      | Design «Mark only problems», FR-006                                      | `todos/task-row/TaskMeta.tsx:112`: an archived row showed «Сделано сегодня» and again a «Сделано» chip.                                                                                                                                                                                                       | **Fixed**: the status chip shows only «В работе» / «Отложено» (active statuses); the archive's first line names the status and date.                                                                            |
| AF-009 | Low      | SDD «Design / Structure»                                                 | The SDD structure listed `data/todos-writes-api.ts`, `task-list/TaskFilters` and omitted `writes/`, `task-filters/`, `feedback/`, and the `['todos', 'task', id]` key.                                                                                                                                        | Fixed: SDD structure and keys match the code.                                                                                                                                                                   |
| AF-010 | Low      | web rules «API» (Russian text)                                           | `todos/feedback/ListStatus.tsx:65`, `writes/failures-api.ts` fallbacks: «Не удалось загрузить: …» and «Не удалось сохранить: …» append the API's English detail for unexpected errors.                                                                                                                        | Accepted: identical to Expenses (`expenses/month-view/ListStatus.tsx:66`, `expenses/data/writes-api.ts:211`); every expected code is mapped to Russian. A repository-wide decision if it should change.         |
| AF-011 | Low      | Optimistic update rollback (ADR 0011)                                    | `todos/writes/status-api.ts:51-63`: two ticks in a row each snapshot the lists; if the first fails, its rollback briefly brings back the second (successful) row.                                                                                                                                             | Accepted: `onSettled` invalidates `['todos']`, so the list is correct after the refetch; a per-row rollback is not worth the complexity for two users.                                                          |
| AF-012 | Low      | Idempotency rules (one key per attempt)                                  | `todos/writes/category-writes-api.ts:30`: `POST /api/todos/categories` sends no `Idempotency-Key`.                                                                                                                                                                                                            | Accepted: the contract takes none (backend design: the unique name answers 409). A retry after a lost answer creates no duplicate but says «Категория с таким названием уже есть»; noted for the backend owner. |
| AF-013 | Info     | Checklist CI-36 / feature-review (main path in e2e)                      | No end-to-end tests exist in the repository.                                                                                                                                                                                                                                                                  | Accepted: project-wide; covered here by Testing Library page tests and the manual Playwright smoke run.                                                                                                         |
| AF-014 | Info     | Checklist FE-24 (bundle)                                                 | `npm run build`: one JS chunk 765 kB (236 kB gzip), Vite's > 500 kB warning.                                                                                                                                                                                                                                  | Accepted: pre-existing for the whole app (no route splitting yet), not introduced by this feature; a candidate for `React.lazy` per section later.                                                              |
| AF-015 | Info     | Console clean                                                            | Smoke run: only React Router v7 «Future Flag» warnings, emitted by the app shell on every page.                                                                                                                                                                                                               | Accepted: pre-existing, outside the feature.                                                                                                                                                                    |

## Checks that passed (no finding)

- **Folders and sizes:** every todos folder has 2–6 production files, no mixed, empty or one-file folders
  (`npm run architecture:structure`); all files within the size limits (AF-007).
- **Boundaries:** `features/todos` and `features/home/todos-summary` import no other feature; only `*-api.ts` files
  import the API client; `home/todos-summary` repeats its labels instead of importing todos.
- **React Query:** keys `['todos', 'tasks', view, query]`, `['todos', 'task', id]`, `['todos', 'categories']`,
  `['todos', 'summary']`; every task, step, status and category write invalidates `['todos']`; step writes keep
  `isPending` until the refetch, so «Сохранить» sends the new version.
- **Idempotency:** task create (dialog and quick add) takes the key from `useIdempotencyKey`: same body → same key,
  changed body → new key, renewed after success and after `todos.idempotency_key_reused`; React Query does not retry
  mutations.
- **If-Match:** edit, delete, status change, category edit/delete send the version; 409 reloads (the task dialog loads
  the fresh task into the form, verified at runtime), 404 reloads and says the task is gone.
- **Pending locks:** quick add, dialog «Сохранить» (also while a step saves), delete confirmations, category dialog,
  focus ticks, «Показать ещё», step add.
- **States:** skeleton rows, designed empty state, filtered empty with «Сбросить фильтр», error with «Повторить» on
  both lists, settings and summary.
- **Kyiv today:** groups and labels use `kyivToday()` (`Intl` with `Europe/Kyiv`, unit-tested across midnight); the
  summary uses the API's `today`.
- **Design:** theme tokens and `mainChannel` tints only, no literal colours; icon-only buttons (add, full form, gear,
  step add/delete, category edit/delete) have `aria-label` and a tooltip; dialog full screen below `sm`.
- **Runtime:** no horizontal overflow on `/todos`, the dialog, the filtered empty list, `/todos/archive`,
  `/todos/settings` and `/?s=todos` at 360 and 1280 px in both schemes; no failed requests or HTTP errors.

## Backend

No backend defect found. AF-012 (no `Idempotency-Key` on category create) is a design note for the backend owner.

## Verification after the fixes

- `npm run typecheck` — pass
- `npm run lint` — pass
- `npm test` — 28 files, 126 tests pass (5 new: undo ordering, focus-tick 409 text, three `describeWriteError` cases)
- `npm run architecture:structure` — «Structure is fine.»
- `npm run format:check` — pass
- `npm run build` — pass (chunk-size warning, AF-014)
- Smoke run repeated after the fixes: all four configurations clean (only AF-015 warnings); the 409 run reloaded the
  form and the retry saved.

## Follow-up audit: day headings, date filter, «На сегодня» (2026-11-26)

Scope: `data/day-groups.ts`, `url-state/date-filter.ts`, `task-filters/DateFilter.tsx`, `archive/ArchiveGroups.tsx`,
the changed list pages, the home `todos-summary` card. Checked against `web/docs/rules.md`, `design.md` and
`npm run architecture:structure` (15 rules green).

- Structure: new pure logic sits in `data/` and `url-state/` with Vitest tests; components stay under 150 lines and one
  per file; no feature imports another (the home card keeps its own API call).
- Contract: the new parameters come from the regenerated client; `TaskQuery` mirrors them.
- URL state: `?when=` outside the list's choices is ignored, a broken `from` / `to` is dropped and a reversed period
  is swapped, so a hand-edited link never sends a request the API refuses.
- AF-NOTE-1 (Low, recorded): the active list pages by priority (50 per page), so with more than 50 active tasks a
  later page can add rows to a day heading already shown. Today the family has 9 active tasks; revisit if lists grow.
