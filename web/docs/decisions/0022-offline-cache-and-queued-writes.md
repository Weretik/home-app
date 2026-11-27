# 0022. The query cache is kept in IndexedDB; a few writes wait for the network

Status: accepted, 2026-11-27

Specification: [web SDD platform/003](../specs/platform/003-pwa-offline/README.md). Builds on web ADR 0009 (a write
refreshes its module), 0011 and 0012 (optimistic ticks), 0015 (the cache is cleared on every session change).

## Context

The shopping list is used in shops, the to-dos on the go. With no network the app showed empty pages and every tick
failed. Masha and Vitaliy agreed on the conflict rule: the last write wins, and a record deleted meanwhile stays
deleted.

## Decision

- **Offline data.** React Query's cache is persisted with `PersistQueryClientProvider` and an IndexedDB persister
  (`idb-keyval`, database `home-cache`): every query that has data (also one whose last refresh failed, such as the
  polled shopping list after a failed poll) and paused queued writes, for 7 days (queries'
  `gcTime` is 7 days too). The cache's `buster` is a hash of `src/api-client/generated/api.ts`, injected by
  `vite.config.ts`, so a new API contract drops data of an old shape. Without IndexedDB (jsdom, some private windows)
  nothing is kept and the app works as before. A session change clears the in-memory cache (ADR 0015) and removes
  the kept one.
- **Queued writes.** A write that is worth keeping offline is a queued write: a serialisable request `{ method, path,
body?, headers?, module, label, refetch? }` sent through the one API client (`src/platform/offline/
queued-write-api.ts`). The mutation runs under the key `['queued-write', …]` with `networkMode: 'online'`, so it
  pauses offline, is kept in IndexedDB with the cache, and is resumed after a restart by mutation defaults registered
  for that key (they send the request and refresh its module). Features call `useQueuedWrite(toRequest, options)` with
  their own input and keep their optimistic `onMutate`/`onError`.
- **Which writes.** Ticking a shopping item, adding shopping items (also «Повернути до списку»), «Прибрати куплене»
  (now optimistic), moving a task (done, cancelled, back), the quick add of a task, and adding an expense (amended
  2026-11-28, [SDD expenses/007](../specs/expenses/007-offline-expense-add/README.md): the dialog closes at once and
  the expense waits under «Очікують мережі»). Other dialog writes (expense edits and deletes, incomes, the jar,
  gifts, notes, settings) stay plain mutations with `networkMode: 'always'`, so offline they fail at once with
  the feature's usual message instead of waiting with a dialog open.
- **Conflicts: last write wins.** `2xx` is done; `404`/`410` counts as done (a deleted record stays deleted, nothing is
  recreated); `409`/`412` with `refetch` reads the record, takes its `version` and sends once more with `If-Match` set
  to it; any other `4xx` drops the write; a network failure retries with backoff (a `5xx` three times). Creates keep
  their `Idempotency-Key` in the request, so a repeat after a lost answer adds nothing twice.
- **A request without an answer.** `apiCall` turns the browser's `TypeError: Failed to fetch` into a `NetworkError`
  whose message is Ukrainian («Немає звʼязку з сервером. Перевірте інтернет і спробуйте ще раз.»), so no screen shows
  the browser's text. A read or write that gets one counts the app offline (`src/platform/client/connectivity.ts`):
  the browser may still say it is online on a network without internet. Reads then keep their data and pause instead
  of showing an error, and queued writes wait; after 15 s the app counts itself online again and the paused requests
  retry (the browser's `online` event ends it at once).
- **Everything kept, synced.** The app does not wait for a page to be opened: once the signed-in member is known it
  loads every main page's default data (`app/pwa/offline-data.ts`: the shopping list, history and settings, the
  to-dos and their archive for the person and for everyone, the budget's current month, incomes, jar, forecast and
  limits, gifts and their settings, each «Про нас» tab, the home summaries, the notifications page), with the
  pages' own query options (`*Query` in each feature's `*-api.ts`), so a page finds its data under its key. It loads
  again when the network is back, when the app is shown and every 5 min while open
  (`platform/offline/cache/offline-sync.ts`); data younger than a minute is not asked again. Android's Periodic
  Background Sync is not used: it would need the session token in the service worker and Chrome runs it rarely.
- **No data kept yet.** A page or card that has nothing kept and no network shows `OfflineNotice` («Немає звʼязку, а
  збережених даних тут ще немає…») instead of loading forever.
- **No English.** `ApiError` keeps the API's message only when it is Ukrainian (the module errors are); a framework's
  default text gets a Ukrainian one by status.
- **Status.** The top bar shows «Офлайн» and the number of waiting writes («Очікує: N»); a refused queued write made
  offline shows «Не вдалося зберегти: <label>». A queued add clears its field at once and says «Додасться, коли
  з’явиться мережа».

## Consequences

- Amended 2026-11-27 after the first test on a phone: the shopping list had been dropped from the kept cache by a
  failed poll, and the page showed «Failed to fetch»; hence «every query that has data» and «a request without an
  answer» above.

- Every main page shows its data offline, opened before or not; filtered views and other months show what was last
  loaded for them, or `OfflineNotice`. Checked on a build in Chromium on all 20 pages, with the API unreachable and
  with the device offline.
- The sync asks about 30 small reads at start and every 5 min while the app is open.
- A queued write overwrites a change the partner made meanwhile (agreed); a stale write is resent only once, so two
  devices cannot loop.
- A write in flight while the app is closed (not paused) is not kept; the module refreshes at the next start and the
  idempotent APIs make a manual repeat safe.
- A new queued write needs only a `toRequest` and a label; the persister and the replay need no change.
