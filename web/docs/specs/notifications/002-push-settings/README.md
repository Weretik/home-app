# 002 — Push notifications: settings per device

- **App type:** React Web (Vite, MUI, React Query, React Router)
- **App:** `web/`, feature `src/features/notifications`
- **Architecture source:** [architecture](../../../architecture.md), [rules](../../../rules.md), [design rules](../../../design.md)
- **Status:** accepted for delivery
- **Owner:** Vitaliy · **Created:** 2026-11-27 · **Updated:** 2026-11-27
- **Backend SDD:** [api notifications/002 — Web Push](../../../../../api/docs/specs/notifications/002-web-push/README.md)
  (`getPushStatus`, `savePushSubscription`, `deletePushSubscription`, `sendPushTest`)
- **Related:** [platform/002 — PWA](../../platform/002-pwa-offline/README.md) (service worker that shows the pushes),
  [001 — Telegram page](../001-telegram-page/README.md)

Every message of the family bot now also comes as a push notification to the devices where the person turned it on.
The «Телеграм» page becomes «Сповіщення»: a new «Push-сповіщення» card turns push on or off for this device, sends a
test and lists the person's devices; the Telegram cards stay as they are.

## State

- Specification: accepted · Implementation: done · Verification: local checks green, audit in
  [../../platform/002-pwa-offline/audit.md](../../platform/002-pwa-offline/audit.md) · Blockers: VAPID keys in the
  App Service settings (until then the card says push is not set up)

## Requirements and behaviour

| ID     | Behaviour                                                                                                                                                                                                                                                                                                                                                 |
| ------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| FR-101 | The service page `/notifications` is called «Сповіщення» in the menu and on the home page card; its first card is «Push-сповіщення».                                                                                                                                                                                                                      |
| FR-102 | Not configured (`configured: false`): the card says push is not set up yet (`WebPush__PublicKey`, `WebPush__PrivateKey` in the App Service settings) and offers nothing else.                                                                                                                                                                             |
| FR-103 | The browser cannot do push: on an iPhone or iPad outside the installed app the card says to add «Дім» to the home screen (Share → «На екран Додому») and open it from there; in another browser without push it says this browser does not support push.                                                                                                  |
| FR-104 | «Сповіщення на цьому пристрої» switch: on asks the browser's permission, subscribes with the API's public key and saves the subscription (`PUT`); off unsubscribes the browser and removes the device (`DELETE`). A denied permission says how to allow it in the browser settings and leaves the switch off. While a change runs the switch is disabled. |
| FR-105 | «Надіслати тест» (when this device or another of the person's devices is subscribed) sends a test push and says how many devices received it, or the problem.                                                                                                                                                                                             |
| FR-106 | «Мої пристрої»: each device with its label («Android · Chrome», «iPhone · Safari», …), «цей пристрій» for the current one, and when it was last seen; «Видалити» removes another device (the switch handles this one).                                                                                                                                    |
| FR-107 | On every start of the signed-in app, a browser that already has permission and a subscription saves it again (keeps `last seen`, moves it to the person now signed in), once per start and silently.                                                                                                                                                      |
| FR-108 | «Останні відправлення» shows the channel of each delivery (Telegram or Push).                                                                                                                                                                                                                                                                             |
| FR-109 | Works at 360 px without horizontal scroll, both colour schemes, design tokens only; Ukrainian texts.                                                                                                                                                                                                                                                      |

## Design

### Structure (`src/features/notifications`, at most 6 production files a folder)

```
notifications/
  data/  + push-api.ts (status query, save, delete, test)
  push/  PushCard.tsx (loading, error), PushSettings.tsx (blockers, switch, test, devices), DeviceRow.tsx, use-push-device.ts (this browser: support, permission,
         subscribe, unsubscribe), push-support.ts (+test: support and iOS hint, device label, key decoding),
         PushSync.tsx (FR-107, rendered by the app layout)
```

### State and data

- Server state under `moduleKeys.notifications`: `['notifications', 'push']`; every push write invalidates the prefix.
- The browser subscription comes from `navigator.serviceWorker.ready` → `pushManager`; `applicationServerKey` is the
  API's public key decoded from base64url.
- `PushSync` is mounted by `app/shell/AppLayout` (the app may import features).

## Tasks and traceability

| ID     | Responsibility                                                   | Test                                           | Covers                 |
| ------ | ---------------------------------------------------------------- | ---------------------------------------------- | ---------------------- |
| TS-401 | Regenerate the client from the contract                          | `npm run contracts:generate`, typecheck        | —                      |
| TS-402 | `push-support.ts`: support, iOS hint, labels, key decoding       | Vitest unit                                    | FR-103, FR-106         |
| TS-403 | Card: not configured, unsupported, on/off, denied, test, devices | Testing Library, mocked fetch and push manager | FR-102…FR-106          |
| TS-404 | `PushSync` saves an existing subscription once                   | Testing Library                                | FR-107                 |
| TS-405 | Menu label, home card, deliveries channel; docs; structure check | tests, `npm run architecture:structure`        | FR-101, FR-108, FR-109 |

## Readiness

- [x] Behaviour and states defined; contract taken from the backend SDD.
- [x] Delivery: typecheck, lint, tests, structure check, build green locally; CI on the pull request.
- [x] Final frontend code audit: [platform 002 audit](../../platform/002-pwa-offline/audit.md#notifications-002--push-settings-same-audit-pass).
