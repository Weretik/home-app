# 0020. The query cache is kept in IndexedDB; a few writes wait for the network

Status: accepted, 2026-11-27

Specification: [web SDD platform/002](../specs/platform/002-pwa-offline/README.md). Builds on web ADR 0009 (a write
refreshes its module), 0011 and 0012 (optimistic ticks), 0015 (the cache is cleared on every session change).

## Context

The shopping list is used in shops, the to-dos on the go. With no network the app showed empty pages and every tick
failed. Masha and Vitaliy agreed on the conflict rule: the last write wins, and a record deleted meanwhile stays
deleted.

## Decision

- **Offline data.** React Query's cache is persisted with `PersistQueryClientProvider` and an IndexedDB persister
  (`idb-keyval`, database `home-cache`): only successful queries and paused queued writes, for 7 days (queries'
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
  (now optimistic), moving a task (done, cancelled, back), and the quick add of a task. Dialog writes (expenses,
  gifts, notes, settings, edits) stay plain mutations with `networkMode: 'always'`, so offline they fail at once with
  the feature's usual message instead of waiting with a dialog open.
- **Conflicts: last write wins.** `2xx` is done; `404`/`410` counts as done (a deleted record stays deleted, nothing is
  recreated); `409`/`412` with `refetch` reads the record, takes its `version` and sends once more with `If-Match` set
  to it; any other `4xx` drops the write; a network failure retries with backoff (a `5xx` three times). Creates keep
  their `Idempotency-Key` in the request, so a repeat after a lost answer adds nothing twice.
- **Status.** The top bar shows «Офлайн» and the number of waiting writes («Очікує: N»); a refused queued write made
  offline shows «Не вдалося зберегти: <label>». A queued add clears its field at once and says «Додасться, коли
  з’явиться мережа».

## Consequences

- The last seen shopping list, to-dos, budget, gifts and notes show offline; pages never opened show their loading
  state until the network is back.
- A queued write overwrites a change the partner made meanwhile (agreed); a stale write is resent only once, so two
  devices cannot loop.
- A write in flight while the app is closed (not paused) is not kept; the module refreshes at the next start and the
  idempotent APIs make a manual repeat safe.
- A new queued write needs only a `toRequest` and a label; the persister and the replay need no change.
