# Identity — scope, rules and scenarios

## Goal

As Masha or Vitaliy, I want to sign in to the site with my Google account once on each device and stay signed in, so
that nobody else can see or change our expenses, gifts, to-dos and shopping list.

## In scope

- Sign-in with a Google ID token obtained by the web app from Google Identity Services (the «Sign in with Google»
  button); no client secret, no redirect.
- A family allowlist of e-mail addresses in configuration (`Identity:AllowedEmails`).
- Server-side sessions with an opaque bearer token: sliding lifetime, sign-out, revocation by removing an e-mail
  from the allowlist.
- Every endpoint of every module requires a signed-in member by default (fallback authorization policy).
- Anonymous on purpose: `/health`, `/api/system/status`, the sign-in itself, the Telegram webhook and the scheduler
  ticks (both keep their secret headers).
- Read-only access for the post-deploy smoke checks with the scheduler key (`X-Scheduler-Key`), `GET` only.

## Out of scope

- Roles or per-person permissions: both members may do everything.
- Recording who created or changed a row in other modules (possible later through the member's claims).
- Other providers (Apple, password), two-factor, a member list editable on the site.
- Signing the Telegram bot's users in (chats are linked from Telegram, [ADR 0036](../../../decisions/0036-telegram-chats-linked-from-telegram.md)).

## Actors

- Masha, Vitaliy: members, each with one Google account on the allowlist.
- Google: issues the ID token (`aud` = our OAuth client id) and publishes its signing keys.
- GitHub Actions: the deploy smoke checks (scheduler key) and the cron ticks (unchanged).
- Anyone else: gets `401` from every protected endpoint and `403` from the sign-in.

## Business rules

| ID | Rule |
| --- | --- |
| R-001 | Sign-in takes a Google ID token. It is accepted only when Google's signature is valid, it has not expired, its audience is `Identity:Google:ClientId`, its issuer is Google, and `email_verified` is true; otherwise `401` `identity.google_token_invalid`. |
| R-002 | The token's e-mail must be on `Identity:AllowedEmails` (trimmed, case-insensitive); otherwise `403` `identity.not_a_member`, and no session is stored. |
| R-003 | Without `Identity:Google:ClientId` sign-in answers `409` `identity.not_configured`; protected endpoints still answer `401`, so a missing setting never opens the site. |
| R-004 | A successful sign-in stores a session and returns its token once: 32 random bytes, base64url. Only the SHA-256 hash of the token is stored (unique). The response also holds the member (e-mail, name, picture) and the expiry. |
| R-005 | A session lasts 30 days after its last use (sliding). Using it moves the expiry forward, writing at most once an hour. An expired or revoked session authenticates nobody. |
| R-006 | Every request to a protected endpoint needs `Authorization: Bearer <token>` of an active session whose e-mail is still on the allowlist; otherwise `401` with a problem body. Removing an e-mail from the allowlist locks that member out at once. |
| R-007 | Sign-out revokes the current session (`204`); the token stops working at once. Other devices stay signed in. |
| R-008 | `GET /api/identity/me` returns the signed-in member (e-mail, name, picture). |
| R-009 | Every endpoint is protected unless it is explicitly anonymous; the anonymous ones are listed in one place and checked by a test, so a new endpoint is never public by accident. |
| R-010 | A `GET` with the correct `X-Scheduler-Key` (= `OutboxTick:Key`) is authenticated as the read-only service `scheduler`; any other method with that header alone is `401`. Without a configured key the header authenticates nobody. |
| R-011 | Sign-in uses the `SignIn` rate limit (5 a minute per address); sign-out uses `PublicWrite`. |
| R-012 | A sign-in also deletes the member's sessions that expired or were revoked more than 30 days ago, so the table does not grow without bound. |

## Acceptance scenarios

| ID | Scenario |
| --- | --- |
| SC-001 | Vitaliy opens the site, presses «Увійти через Google», picks his account: the site opens; `GET /api/expenses` with his token answers `200`. |
| SC-002 | A stranger signs in with their Google account: `403` `identity.not_a_member`; no session is stored. |
| SC-003 | A request without a token, with a made-up token, or with an expired token answers `401` on every module. |
| SC-004 | Vitaliy presses «Вийти»: the same token answers `401`; Masha's phone stays signed in. |
| SC-005 | Masha opens the site 20 days after her last visit: still signed in; after 31 days without a visit: `401`, she signs in again. |
| SC-006 | Vitaliy removes Masha's e-mail from the App Service settings: her next request answers `401`. |
| SC-007 | The deploy's smoke check reads every section with `X-Scheduler-Key`: `200`; a `POST` with that header: `401`; the same reads without any header: `401`. |
| SC-008 | `/health`, the Telegram webhook and the ticks behave exactly as before. |

## Non-functional requirements

| Requirement | Value | Check |
| --- | --- | --- |
| Free | no paid service: Google Identity Services and Azure free tiers only | ADR 0037 |
| Secrets | no client secret; tokens stored only as hashes; nothing secret in `web/` (the client id is public) | `ConfigurationRulesTests`, design |
| Secure by default | fallback policy requires a member; anonymous endpoints listed with a reason | `AuthorizationRulesTests` (architecture), `IdentityProtectionApiTests` (integration) |
| Cost per request | one indexed lookup of the session by hash; at most one write an hour per session | design |
