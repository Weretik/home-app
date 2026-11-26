# Code audit — 001 shopping list

- **Kind:** final feature audit (local review of the affected boundaries, not the full 00–06 cycle).
- **Scope:** `web/src/features/shopping`, `web/src/features/home/shopping-summary`, the registry entry in
  `features/home/page/summary-sections.ts`, the routes in `app/root/App.tsx`, `tools/check-structure.mjs` and the two
  files split for it.
- **Sources:** `AGENTS.md`, `web/docs/{architecture,rules,design}.md`, web ADRs 0002–0012, this SDD, the generated
  contract `api/docs/contracts/openapi.json`.
- **Date:** 2026-11-26.

## Step 0 — quality targets

The repository has no quality-targets document (no `quality-targets.md` in `api/docs` or `web/docs`), so there is no
performance verdict. Proposed client targets, **to confirm** (AF-6):

| Target                                                  | Proposed                               | How to measure                                            |
| ------------------------------------------------------- | -------------------------------------- | --------------------------------------------------------- |
| Список usable after opening (phone, 4G, warm cache)     | ≤ 1.5 s                                | Chrome DevTools throttled «Fast 4G», mid phone profile    |
| Feedback on a tick                                      | ≤ 100 ms (optimistic, no request wait) | DevTools performance trace of a tap                       |
| The other person's change visible                       | ≤ 20 s while open, at once on return   | polling interval, `refetchOnWindowFocus` + `staleTime: 0` |
| API P95 the list depends on (`GET /api/shopping/items`) | ≤ 300 ms                               | App Service metrics                                       |

## Checks run

| Check                                                       | Result                                                                                        |
| ----------------------------------------------------------- | --------------------------------------------------------------------------------------------- |
| `npm run architecture:structure` (now with line limits)     | Structure is fine                                                                             |
| `npm run lint`, `npm run typecheck`, `npm run format:check` | clean                                                                                         |
| `npm test`                                                  | 28 files, 109 tests passed                                                                    |
| `npm run build`                                             | built; one chunk 768 kB (the existing single-bundle warning)                                  |
| `npm run contracts:generate` twice                          | identical output; `contracts:check` differs from `HEAD` only by this uncommitted regeneration |

## Folder and size axiom

No folder over six production files, none mixing files and subfolders, no one-file or wrapper folders, no dump names
(largest: `data/`, `quick-add/`, `department-settings/`, `keyword-settings/` with 6). Largest files:
`KeywordsCard.tsx` 130, `DepartmentsCard.tsx` 123, `ItemRow.tsx` 109 lines; largest module `item-writes-api.ts`. The
structure check has no baseline of exceptions.

## Findings

| ID   | Severity | Status   | Where                                                                                                    | Rule / source                                                    | Evidence and impact                                                                                                                                           | Fix / target                                                                                                   | Task             | Verification                                                         |
| ---- | -------- | -------- | -------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------- | ---------------- | -------------------------------------------------------------------- |
| AF-1 | Medium   | verified | `web/tools/check-structure.mjs`                                                                          | task limits; rules.md Structure                                  | The line limits (component 150, module 200) were not checked; `expenses/data/writes-api.ts` had 212 and `home/expenses-summary/view/LimitsCard.tsx` 172 lines | check added; jar writer moved to `savings-api.ts`; `expenses-summary/limits/` split; rules.md states the limit | EN-1             | structure check passes; Expenses and Home tests pass                 |
| AF-2 | Medium   | verified | `shopping/data/shopping-api.ts` `useShoppingList`                                                        | R-5, ADR 0012                                                    | The app default `staleTime` 30 s meant a return to the tab within 30 s did not reload, so a change by the other person could stay hidden                      | `staleTime: 0` on the list                                                                                     | TS-3             | code review; typecheck; list tests pass                              |
| AF-3 | Low      | verified | `ItemDialog.tsx`, `DepartmentDialog.tsx`, `KeywordDialog.tsx`, `ListSettingsCard.tsx`, `HistoryPage.tsx` | one-responsibility signals (> 3 `useState`, logic in components) | The same check → send → field-error state was repeated in four forms with 4–5 `useState`; HistoryPage held the return-to-list write                           | `page-parts/use-checked-form.ts`, `history-page/use-return-item.ts`                                            | TS-6, TS-7, TS-8 | page tests for all four forms and history pass                       |
| AF-4 | Low      | verified | `list-page/DepartmentGroup.tsx`                                                                          | design.md (theme spacing via `sx`)                               | An inline `style={{ marginRight: 8 }}`                                                                                                                        | `Box sx={{ mr: 1 }}`                                                                                           | TS-5             | lint, tests                                                          |
| AF-5 | Low      | verified | `history-api.ts`, `departments-api.ts`, `keywords-api.ts`                                                | ADR 0003 (the generated file wins)                               | The generated contract pages history by `page`/`pageSize` and requires an `Idempotency-Key` on department and keyword creates, unlike the brief               | follow the generated contract; key per opened form, renewed on «key reused»                                    | EN-2             | typecheck against the generated types; Settings tests assert the key |
| AF-6 | Info     | open     | repository                                                                                               | quality-attributes step 0                                        | No client quality targets                                                                                                                                     | proposed numbers above; the owner confirms and records them                                                    | —                | —                                                                    |
| AF-7 | Info     | accepted | repository                                                                                               | feature review: main path e2e                                    | No end-to-end runner in the project                                                                                                                           | page tests cover add → tick → clear and undo                                                                   | —                | `ListPage.test.tsx`                                                  |
| AF-8 | Info     | accepted | `keyword-settings/KeywordsCard.tsx`                                                                      | > 3 `useState` signal                                            | Four `useState`: search, department filter, rows shown, open dialog — UI state of one card, 130 lines                                                         | accepted: no data logic in the card                                                                            | —                | —                                                                    |

## Feature review list

- Access rights: ➖ no identity yet (API ADR 0015); the backend rate-limits writes.
- Double submit: every submit and action button is disabled while pending; adds, department and keyword creates send
  an `Idempotency-Key` per attempt (reused on retry, renewed after success or «key reused»); the tick is idempotent on
  the server and needs no lock; the query client never retries mutations.
- Boundaries: no imports between features (the summary keeps its own labels); API only from `*-api.ts` through
  `src/platform/api.ts` (checked).
- ADR: [web ADR 0012](../../../../decisions/0012-live-shared-list-with-optimistic-ticks-and-polling.md) records the
  live list; the web app is already a container on the backend C4 diagram, no new app.
- Feature map: `web/docs/architecture.md` lists the routes, the folders and the home summary.
- Scenarios: SC-1…SC-16 traced in [traceability](../traceability.md).

## Summary

5 findings fixed and verified (2 medium, 3 low), 1 open info (quality targets to confirm, does not block delivery), 2
accepted. No open finding blocks the feature.
