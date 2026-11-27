# 0015. A sign-in gate with the Google button and a bearer token in localStorage

Status: accepted, 2026-11-27

## Context

The API now requires a signed-in family member for everything ([API ADR 0037](../../../api/docs/decisions/0037-google-sign-in-with-server-sessions.md)).
The web app (Azure Static Web Apps) and the API (App Service) are on different sites, so a cookie set by the API would
be a third-party cookie that Safari and Chrome block. Masha and Vitaliy mostly use phones and should sign in once per
device, not every hour.

## Decision

- Google Identity Services renders the «Sign in with Google» button (script `accounts.google.com/gsi/client`,
  loaded only on the sign-in screen, `auto_select` on, except right after «Вийти», when automatic sign-in is turned off so the member is not signed straight back in). The ID token it returns goes once to
  `POST /api/identity/sign-in`; the app keeps only the API's session token.
- The token lives in `localStorage` (`home.session`) behind one small store, `src/platform/session.ts`, read
  synchronously at start; `src/platform/api.ts` adds `Authorization: Bearer` to every call, and any `401` while a token
  is stored clears it and shows the sign-in screen with «Сесія закінчилась».
- A gate (`SessionGate` in `features/identity`) wraps all routes in `app/root/App.tsx`: no token → the sign-in screen
  at the same URL, so the page the person opened is shown after sign-in. There is no `/sign-in` route.
- Signing in or out clears the whole React Query cache, so no cached data of one session survives into the next.
- The Google client id is public and comes from the build variable `VITE_GOOGLE_CLIENT_ID`.

## Consequences

- The token is readable by any script running on the site. The site runs only its own bundle and Google's sign-in
  script; a stolen token is ended by «Вийти» on that device or by removing the e-mail from the API's allowlist.
- No cookies, no CSRF exposure: the API only trusts the header.
- `features/identity` is the only feature that knows about sign-in; other features keep calling the API through their
  `*-api.ts` and never see the token.
- Tests render the gate with a stored token or with a fake Google script; the real script is never loaded in tests.
