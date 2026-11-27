# 0037. Google sign-in for the family, server-side sessions, everything closed by default

Status: accepted, 2026-11-27. Supersedes [0015](0015-expenses-open-until-identity.md) and closes 0009.

## Context

The site served the family's expenses, savings, gifts, to-dos and shopping list without sign-in
([0015](0015-expenses-open-until-identity.md)); anyone who found the API address could read and change them. On
2026-11-27 Vitaliy asked for sign-in. Constraints: two users only (Masha and Vitaliy), both with Google accounts;
everything must stay free; the web app (Azure Static Web Apps) and the API (App Service) are on different sites, so a
cookie set by the API is a third-party cookie that Safari and Chrome block; the Telegram webhook, the scheduler cron
and the deploy checks call the API without a person.

Options considered:

- **Azure Static Web Apps or App Service built-in authentication (Easy Auth).** Free, but custom Google providers
  and role assignment need the paid Standard plan on Static Web Apps; App Service Easy Auth protects the API with a
  cookie on the API's own domain (third-party for the web app) or with tokens that the web app would still have to
  obtain. It also moves the allowlist into the portal, outside tests.
- **Google ID token sent with every request.** No sessions, but the token lives one hour, so the app would need
  Google's silent re-prompt on every expiry, and sign-out would not end anything on the server.
- **Our own signed JWT after a Google sign-in.** Long sessions, but a signing key to keep secret and rotate, and no
  way to end one session without a deny list.
- **Google sign-in once, then a server-side session with an opaque bearer token (chosen).** The web app gets a Google
  ID token from Google Identity Services, the API verifies it (audience = our client id) and the allowlist, and
  stores a session in the new Identity module; the web app sends the session token as a bearer header. No secret
  beyond what exists, sessions can be ended one by one, removing an e-mail from the allowlist locks that person out at
  once, and the header works across sites.

## Decision

- A new module `Identity` (five projects, schema `identity`, table `sessions`) owns sign-in, sign-out, the current
  member and the authentication of every request. Design: [Identity SDD](../specs/identity/001-google-sign-in/README.md).
- Sign-in: `POST /api/identity/sign-in` with the Google ID token, verified by `Google.Apis.Auth`
  (`GoogleJsonWebSignature`) against `Identity:Google:ClientId`; the e-mail must be verified and on
  `Identity:AllowedEmails`. No client secret is needed. Without a client id sign-in answers `409` and the site stays
  closed.
- Sessions: an opaque 32-byte random token, stored only as its SHA-256 hash; 30 days after the last use (sliding,
  written at most once an hour); `POST /api/identity/sign-out` revokes the current one. Every request also checks
  that the e-mail is still on the allowlist.
- Closed by default: the authorization fallback policy requires an authenticated user, so every endpoint of every
  module, including new ones, needs a session. The few anonymous endpoints (`/health`, `/api/system/status`, the
  sign-in, the scheduler ticks and the Telegram webhook, which keep their secret headers) say `.AllowAnonymous()` in
  their own file and are listed in an architecture test with the reason.
- The post-deploy smoke checks ([root ADR 0005](../../../docs/decisions/0005-post-deploy-smoke-reads-every-section.md))
  read every section with the scheduler key: a `GET` with the right `X-Scheduler-Key` authenticates as the read-only
  service `scheduler`; the same header on any other method authenticates nobody.
- Both members may do everything; there are no roles.

## Consequences

- The family's data is no longer readable or writable without a Google account on the allowlist; the rate limits of
  [0012](0012-rate-limits-on-anonymous-writes.md) stay as a second line.
- Every protected request costs one indexed lookup of the session; acceptable for two users.
- The scheduler key now also grants reading every section. It was already the key that triggers the bot and the
  outbox; it lives only in the App Service and the GitHub secret `SCHEDULER_KEY`. Rotating it rotates both.
- The web app keeps the token in `localStorage` ([web ADR 0015](../../../web/docs/decisions/0015-sign-in-gate-with-google-button-and-bearer-token.md)):
  a script injected into the site could read it. The site loads no third-party scripts besides Google's sign-in, and
  a stolen token is ended by signing out on that device or by removing the e-mail from the allowlist.
- Integration tests sign in through the real endpoint with a fake Google verifier; the default test client is
  signed in, and a test asks for an anonymous client explicitly.
- Rows in other modules still carry no author; the member's claims (`email`, `name`) make that possible later.
