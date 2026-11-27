# 0019. A service worker precaches the shell and waits for «Оновити»

Status: accepted, 2026-11-27

Specification: [web SDD platform/003](../specs/platform/003-pwa-offline/README.md); push payload:
[api notifications/002](../../../api/docs/specs/notifications/002-web-push/README.md).

## Context

«Дім» is opened from the home screen of two phones many times a day, often in a shop with a weak network. Without a
service worker every start waits for the network, a start with no network shows the browser's error page, and push
messages of the family bot cannot be shown. A new deploy must reach the phones, but a page that reloads on its own
loses a half-filled form.

## Decision

- `vite-plugin-pwa` with `strategies: 'injectManifest'`: our own `src/app/service-worker/service-worker.ts` (built to
  `/service-worker.js`) with the Workbox precache manifest injected at build time. It precaches the built shell (HTML,
  JS, CSS, fonts, icons, the manifest), cleans outdated caches, and answers every navigation with the cached
  `index.html` (`NavigationRoute`), so client routes open offline. API calls go to another origin and are never cached
  by the service worker; the data offline is web ADR 0022.
- `registerType: 'prompt'`, `injectRegister: false`: the app registers the worker itself through
  `virtual:pwa-register/react` (`src/app/pwa/use-app-update.ts`) and checks for a new deploy every hour. A waiting
  version shows the snackbar «Доступна нова версія · Оновити»; the tap sends `SKIP_WAITING` and reloads. Nothing reloads
  on its own.
- `manifest: false`: the hand-written `public/site.webmanifest` stays the manifest («Дім», `uk`, `id`, `scope` and
  `start_url` `/`, `standalone`, the 192 and 512 icons).
- The worker shows the API's push messages (`{ title, body, url, tag }`, the app icon, a malformed payload as a
  generic «Дім»), and a tap focuses an open window of the site and navigates it to the message's page (same origin
  only, else `/`) or opens a new window. The payload and click rules are pure (`push-notification.ts`) and unit-tested.
- The worker has its own TypeScript project (`tsconfig.sw.json`, the `WebWorker` lib); `npm run typecheck` checks
  both.
- Azure Static Web Apps serves `/service-worker.js`, `/index.html` and `/site.webmanifest` with `Cache-Control:
no-cache` and keeps them out of the SPA fallback, so a deploy is seen at the next start.

## Consequences

- The second visit and every start from the home screen open at once, also with no network.
- A deploy reaches an open app within an hour (or at the next start) as a prompt; a person who never taps «Оновити»
  keeps the old version until all its windows close.
- The precache holds the whole bundle (about 1 MB plus fonts); `maximumFileSizeToCacheInBytes` is raised to 4 MB so
  a growing bundle is not silently left out.
- `npm run dev` registers nothing (`devOptions` off); the worker is tried with `npm run build && npm run preview`.
