# 001 — Sign-in with Google («Вхід»)

- **App type:** React Web (Vite, MUI, React Query, React Router)
- **App:** `web/`, feature `src/features/identity`, session store `src/platform/session.ts`
- **Architecture source:** [architecture](../../../architecture.md), [rules](../../../rules.md), [design rules](../../../design.md)
- **Status:** accepted for delivery
- **Owner:** Vitaliy · **Created:** 2026-11-27 · **Updated:** 2026-11-27
- **Backend SDD:** [api/docs/specs/identity/001-google-sign-in](../../../../../api/docs/specs/identity/001-google-sign-in/README.md)
  (rules R-001…R-012, contract `signIn`, `signOut`, `getCurrentMember`)
- **Decision:** [web ADR 0015](../../../decisions/0015-sign-in-gate-with-google-button-and-bearer-token.md)

Only Masha and Vitaliy may use the site. Before anything else the app shows a sign-in screen with the «Sign in with
Google» button; after it, the site works as before, and every API call carries the session token. The session lasts
30 days after the last visit, so in practice each device signs in once. The header gets the member's avatar with
«Вийти». Interface text is Ukrainian, like the rest of the site ([web ADR 0014](../../../decisions/README.md)).
Written from the frontend SDD template (Weretik/obsidian `Templates/frontend-sdd`), React variant, condensed.

## State

- Specification: accepted · Implementation: done (2026-11-27) · Verification: [audit.md](audit.md) · Blockers: the Google
  OAuth client id (`VITE_GOOGLE_CLIENT_ID` repository variable) set by Vitaliy before the merge.

## Requirements and behaviour

| ID     | Behaviour                                                                                                                                                                                                                                                                                                                                                             |
| ------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| FR-001 | Without a stored session token every route shows the sign-in screen instead of the app: the brand, «Вхід до сімейного сайту», the Google button (`theme` follows the colour scheme, Ukrainian `locale`), and «Вхід лише для родини». The URL stays, so after sign-in the same page opens.                                                                             |
| FR-002 | The Google button gives an ID token; the app sends `POST /api/identity/sign-in` `{ credential }`. On `200` it stores `token` (localStorage `home.session`) and shows the app. While the request runs the button area shows a progress indicator.                                                                                                                      |
| FR-003 | Errors on the screen, by problem code: `identity.not_a_member` → «Ця пошта не має доступу. Увійдіть акаунтом Віталія або Маші.»; `identity.google_token_invalid` → «Google не підтвердив вхід. Спробуйте ще раз.»; `identity.not_configured` → «Вхід ще не налаштовано на сервері.»; `429` → the problem message; any other → «Не вдалося увійти. Спробуйте ще раз.». |
| FR-004 | Without `VITE_GOOGLE_CLIENT_ID` at build time the screen shows «Вхід ще не налаштовано» instead of the button. When Google's script cannot load: «Не вдалося завантажити вхід Google. Перевірте з'єднання.» with «Спробувати знову».                                                                                                                                  |
| FR-005 | Every API request carries `Authorization: Bearer <token>`. A `401` from any request while a token is stored removes the token, clears the query cache, and shows the sign-in screen with «Сесія закінчилась, увійдіть знову».                                                                                                                                         |
| FR-006 | The header shows the member's avatar (Google picture, else the first letter) as a button; its menu shows the name and e-mail and «Вийти». «Вийти» sends `POST /api/identity/sign-out` (best effort), removes the token, clears the query cache and shows the sign-in screen without a message.                                                                        |
| FR-007 | The member comes from `GET /api/identity/me` (key `['identity', 'me']`); the avatar shows a neutral placeholder while it loads.                                                                                                                                                                                                                                       |
| FR-008 | Another tab of the same browser follows a sign-in or sign-out (the `storage` event).                                                                                                                                                                                                                                                                                  |
| FR-009 | Works at 360 px without horizontal scroll, both colour schemes, design tokens only.                                                                                                                                                                                                                                                                                   |

## Design

### Structure (`src/features/identity`, at most 6 production files a folder)

```
platform/
  session.ts             the token store: read, save, clear(reason), subscribe (localStorage + storage event)
  api.ts                 the client + a middleware: bearer header, 401 → clear('expired')
features/identity/
  data/      identity-api.ts (useSignIn, useSignOut, useCurrentMember), google-identity.ts (loads the GIS script
             once, initialize + renderButton), sign-in-errors.ts (problem code → text), use-session.ts (the store as
             React state)
  sign-in/   SessionGate (children or the sign-in screen), SignInPage (layout, texts), GoogleButton (the button
             host, progress, retry)
  member/    MemberMenu (avatar button, menu with name, e-mail, «Вийти»), MemberAvatar
```

### State and data

- The token is the only client state outside React Query: `platform/session.ts`, read synchronously at start, so the
  first render already knows whether to show the app. `useSession` wraps it with `useSyncExternalStore`.
- Server state in React Query under a new prefix `moduleKeys.identity = ['identity']`; sign-in and sign-out clear the
  whole cache (`queryClient.clear()`), so one member never sees the previous member's cached data.
- `signIn` sends no `Idempotency-Key` (backend SDD, "Contract").
- The Google script is loaded from `https://accounts.google.com/gsi/client?hl=uk` (Ukrainian button and One Tap) by `google-identity.ts` only on the
  sign-in screen; `auto_select` is on, so a returning member on a known device signs in with one tap.

### Routes and shell

- `app/root/App.tsx`: the routes are wrapped in `SessionGate`; there is no `/sign-in` route (FR-001 keeps the URL).
- `app/shell/TopBar.tsx`: `MemberMenu` right of the colour mode menu.

### Build

- `VITE_GOOGLE_CLIENT_ID` (public, a repository variable) in `web.yml` and `.env.example`.

## Tasks and traceability

| ID     | Responsibility                                                                  | Test                                    | Covers         |
| ------ | ------------------------------------------------------------------------------- | --------------------------------------- | -------------- |
| TS-301 | Regenerate the client from the contract                                         | `npm run contracts:generate`, typecheck | —              |
| TS-302 | `session.ts`: save, read, clear with reason, storage event                      | Vitest unit                             | FR-005, FR-008 |
| TS-303 | `api.ts` middleware: bearer header, `401` clears the session                    | Vitest unit with a stubbed fetch        | FR-005         |
| TS-304 | `sign-in-errors.ts`: every code to its text                                     | Vitest unit                             | FR-003         |
| TS-305 | Gate and sign-in screen: no token → screen, success → app, errors, no client id | Testing Library, fake Google script     | FR-001…FR-004  |
| TS-306 | Member menu: name, e-mail, «Вийти» clears and calls the API                     | Testing Library                         | FR-006, FR-007 |
| TS-307 | Shell, routes, `web.yml`, docs, structure check                                 | `npm run architecture:structure`, tests | FR-009         |

## Readiness

- [x] Behaviour, states and routes defined; contract taken from the backend SDD.
- [x] Delivery: typecheck, lint, tests, structure check green.
- [x] Final frontend code audit: [audit.md](audit.md).
