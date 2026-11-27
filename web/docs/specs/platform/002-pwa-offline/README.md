# 002 — Installable app, offline data and queued writes (PWA)

- **App type:** React Web (Vite, MUI, React Query, React Router)
- **App:** `web/`, areas `src/app` (service worker, update prompt, offline status), `src/platform/offline`, the
  shopping and todos features (queued writes)
- **Architecture source:** [architecture](../../../architecture.md), [rules](../../../rules.md), [design rules](../../../design.md)
- **Status:** accepted for delivery
- **Owner:** Vitaliy · **Created:** 2026-11-27 · **Updated:** 2026-11-27
- **Decisions:** [web ADR 0019](../../../decisions/0019-service-worker-with-update-prompt.md) (service worker, precached
  shell, update prompt), [web ADR 0020](../../../decisions/0020-offline-cache-and-queued-writes.md) (IndexedDB cache,
  queued writes, last write wins)
- **Backend SDD:** [api notifications/002 — Web Push](../../../../../api/docs/specs/notifications/002-web-push/README.md)
  (the push payload the service worker shows)
- **Related:** [notifications/002 — push settings](../../notifications/002-push-settings/README.md)

On 2026-11-27 Vitaliy asked to make «Дім» a full PWA: it opens at once from the home screen, works without a network
with the data last seen, keeps the changes made offline and sends them when the network is back, offers a new version
when one is deployed, and shows the push notifications of the family bot. Written from the frontend SDD template
(Weretik/obsidian `Templates/frontend-sdd`), React variant, condensed.

## State

- Specification: accepted · Implementation: done · Verification: local checks green, audit in [audit.md](audit.md) ·
  Blockers: none

## Requirements and behaviour

| ID     | Behaviour                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| ------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| FR-001 | A service worker precaches the built shell (HTML, JS, CSS, fonts, icons): a second visit and every start from the home screen open without waiting for the network, also with no network at all. Every navigation is answered with the cached `index.html` (client routes work offline). API calls are never cached by the service worker.                                                                                                       |
| FR-002 | A new deploy is detected by the waiting service worker; a snackbar «Доступна нова версія» with «Оновити» activates it and reloads the page. Nothing reloads on its own, so a half-filled form is never lost.                                                                                                                                                                                                                                     |
| FR-003 | The manifest describes the installed app: name «Дім», Ukrainian, `standalone`, `start_url` and `scope` `/`, theme colours, 192 and 512 icons.                                                                                                                                                                                                                                                                                                    |
| FR-004 | The data of every successful read is kept in IndexedDB (`home-cache`) for 7 days and restored on start, so the shopping list, to-dos, budget, gifts and notes last seen show offline. The cache is dropped when the API contract changes (its hash is the cache version) and on every sign-in, sign-out or expiry (the whole query cache is cleared, ADR 0015).                                                                                  |
| FR-005 | Offline the top bar shows «Офлайн»; while queued changes wait it shows how many («Очікує: N»), and a snackbar says when a queued change was refused by the server.                                                                                                                                                                                                                                                                               |
| FR-006 | Queued writes: ticking a shopping item bought or back, adding shopping items, «Прибрати куплене», moving a task to done, cancelled or back, and adding a task. Offline they are kept (also across a closed app) and sent in order when the network returns; the optimistic change on the screen stays meanwhile. A queued add clears its field at once and says «Додасться, коли з'явиться мережа».                                              |
| FR-007 | Conflicts of queued writes: last write wins — a write refused because the record changed meanwhile (`409`/`412` with a version) reads the record again and sends once more with its new version; a record that was deleted meanwhile stays deleted (`404` drops the write, nothing is recreated). Creates carry their `Idempotency-Key`, so a repeat after a lost answer adds nothing twice. Other refusals drop the write and show it (FR-005). |
| FR-008 | Writes that are not queued (dialogs: expenses, gifts, notes, settings, edits) fail at once offline with the feature's usual failure message instead of waiting.                                                                                                                                                                                                                                                                                  |
| FR-009 | The service worker shows the push messages of the API (`{ title, body, url, tag }`) with the app icon; a tap focuses an open window of the app and opens the message's page (same origin only), or opens a new window. A malformed payload shows a generic «Дім» notification.                                                                                                                                                                   |
| FR-010 | Not indexed (FR of platform 001 unchanged); the service worker file is served with `no-cache`, so a deploy is seen at the next start.                                                                                                                                                                                                                                                                                                            |

## Design

### Structure

```
src/app/service-worker/   service-worker.ts (entry, own tsconfig.sw.json with the WebWorker lib),
                          push-notification.ts (+test: payload → notification, click target, window reuse)
src/app/pwa/              UpdatePrompt.tsx (+test), use-app-update.ts (virtual:pwa-register/react, hourly check),
                          OfflineStatus.tsx (+test: top bar chip + refused-write snackbar)
src/platform/client/      api.ts, member-api.ts, query-client.ts (gcTime 7 days, mutations networkMode 'always'),
                          locale.ts (moved from src/platform/, see «Platform folders» below)
src/platform/session/     session.ts, session-middleware.ts (+tests; moved)
src/platform/offline/     query-persistence.ts (+test: IndexedDB persister, what is kept, cache version),
                          queued-request.ts (the serialisable request, the ['queued-write'] key),
                          queued-write-api.ts (+test: send, last write wins, deleted stays deleted, retry, defaults),
                          use-queued-write.ts (+test: the hook features use), network-status.ts (+test: online state,
                          waiting count, refused writes), use-network-status.ts (the hooks over it)
```

- **Platform folders.** `src/platform` held six files, and a folder holds either files or subfolders, so adding
  `offline/` moved the existing files into `client/` and `session/` (imports, the structure rules' paths, ESLint
  messages and the docs naming them were updated; no behaviour changed).
- `network-status.ts` holds the plain store and `use-network-status.ts` the hooks, because a module that exports a
  hook is a `use-*.ts` (structure rule `hookFileNames`); the request type and key live in `queued-request.ts` so the
  sender, the hook, the status and the persister share them without an import cycle.
- Build: `vite-plugin-pwa` with `strategies: 'injectManifest'`, `registerType: 'prompt'`, `injectRegister: false`,
  `manifest: false` (the hand-written `public/site.webmanifest` stays); Workbox precaching and a `NavigationRoute`.
- `public/staticwebapp.config.json`: `Cache-Control: no-cache` route headers for `/service-worker.js`, `/index.html`
  (also `/`) and `/site.webmanifest`; the worker and the manifest are excluded from the SPA fallback next to
  `/assets/*`, and `.webmanifest` is served as `application/manifest+json`. Azure SWA allows a wildcard only at the end
  of a path, and `injectManifest` bundles Workbox into the worker (no `workbox-*.js` files), so no `workbox-*` pattern
  is listed. Route headers do not apply to a request answered by the fallback; the service worker serves those
  navigations from its precache anyway.
- `maximumFileSizeToCacheInBytes` is 4 MB (the main chunk is about 1 MB today), so a growing bundle is never silently
  left out of the precache.

### State and data

- `PersistQueryClientProvider` with an IndexedDB persister (`idb-keyval`); `gcTime` of queries 7 days so what is kept
  is not collected; only successful queries and paused queued writes are kept; `buster` = the hash of
  `src/api-client/generated/api.ts`, injected at build time. After restore the paused writes are resumed.
- A queued write is a serialisable request `{ method, path, body?, headers?, module, label, refetch? }` run by a
  mutation default registered for the key `['queued-write']` (`networkMode: 'online'`, so it pauses offline and
  resumes after a restart). The feature keeps its own `onMutate`/`onError`/`onSettled` for the open page; after a
  restart only the request is replayed and the module prefix is refreshed.
- `useQueuedWrite(toRequest, options)` hides the request from the feature: it is called with the feature's input
  (`mutate(input)`); the persisted variables are `{ request, input }`. Its `isSaving` is `isPending && !isPaused`, so
  a button is not locked by a write waiting offline. Without its own `onSettled` it refreshes the request's module.
- Retries: a network failure is retried with backoff until it passes (paused while offline), a `5xx` three times, a
  `4xx` never.
- The refused-write snackbar shows refusals of writes made offline (the screen that made them may be gone) and of
  writes restored after a restart; a write made online is refused on its own screen as before.
- Labels (for «Не вдалося зберегти: …»): «позначку «куплено»», «повернення до списку», «товари «…»», «прибирання
  кошика», «позначку «зроблено»», «скасування справи», «повернення справи», «справу «…»».
- The chip is one element: «Офлайн», «Офлайн · N» (named «Офлайн, очікує: N») or «Очікує: N»; its icon hides below
  `sm`, so the header fits 360 px.
- Only paused queued writes are kept; a write in flight while the app closes is not (it is idempotent, and the module
  refreshes at the next start).
- Other mutations get `networkMode: 'always'` (FR-008).

## Tasks and traceability

| ID     | Responsibility                                                        | Test                                                                      | Covers                 |
| ------ | --------------------------------------------------------------------- | ------------------------------------------------------------------------- | ---------------------- |
| TS-301 | Service worker, plugin, manifest, SWA headers, `tsconfig.sw.json`     | build, `npm run typecheck`, deploy smoke (`/service-worker.js`, manifest) | FR-001, FR-003, FR-010 |
| TS-302 | Push payload and click target                                         | Vitest unit                                                               | FR-009                 |
| TS-303 | Update prompt                                                         | Testing Library (mocked register)                                         | FR-002                 |
| TS-304 | IndexedDB persistence, cache version, what is kept                    | Vitest unit (fake IndexedDB-free storage)                                 | FR-004                 |
| TS-305 | Queued write: send, `409`→reread→resend, `404` dropped, refused shown | Vitest unit (mocked fetch)                                                | FR-006, FR-007         |
| TS-306 | Shopping tick/add/clear and todos move/add through the queue          | existing feature tests + new offline tests                                | FR-006                 |
| TS-307 | Offline chip, pending count, refused snackbar                         | Testing Library                                                           | FR-005                 |
| TS-308 | Docs, structure check, sizes                                          | `npm run architecture:structure`                                          | —                      |

## Readiness

- [x] Behaviour, states and conflict rule defined (Vitaliy agreed: last write wins, deleted stays deleted).
- [x] Delivery: typecheck, lint, tests, structure check, build green locally (CI on the pull request).
- [x] Final frontend code audit: [audit.md](audit.md).
