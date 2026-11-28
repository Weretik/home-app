# Identity module

Sign-in for the family. Masha and Vitaliy sign in with their Google accounts; the API keeps a session per device and
requires one for every endpoint of every module, except the few listed below. The module was added in
[ADR 0037](../decisions/0037-google-sign-in-with-server-sessions.md) from the specification
[specs/identity/001-google-sign-in](../specs/identity/001-google-sign-in/README.md); it ends the open period of
[ADR 0015](../decisions/0015-expenses-open-until-identity.md). [Spec 002](../specs/identity/002-who-is-signed-in/README.md)
adds who the member is in the home ([ADR 0038](../decisions/0038-allowlist-position-says-who-the-member-is.md)).

Code: [`src/Modules/Identity`](../../src/Modules/Identity). Schema: `identity`.

## Model

| Concept | Meaning |
| --- | --- |
| `Session` | One signed-in device: the SHA-256 of its bearer token, the member's e-mail, name and picture, created, last seen, expires (30 days after the last use, slid at most once an hour, never past 90 days after the sign-in, [ADR 0046](../decisions/0046-session-max-age-and-sign-out-everywhere.md)), revoked. |
| `FamilyAllowlist` | The e-mails that may sign in (`Identity:AllowedEmails`), compared trimmed and lower-case. Entry 0 is Vitaliy, entry 1 is Masha (`PersonOf`, [ADR 0038](../decisions/0038-allowlist-position-says-who-the-member-is.md)). |
| `FamilyPerson` | Who a member is in the home: `Vitaliy` or `Maria`, as `person` on `GET /me` and on the sign-in response (null for a later entry). |
| `MemberProfile` / `FamilyMember` | Who a session belongs to; `FamilyMember` is the form the API returns. |

## How it works

1. The web app shows Google's «Sign in with Google» button (Google Identity Services, our OAuth client id) and gets
   an ID token.
2. `POST /api/identity/sign-in` verifies it with `Google.Apis.Auth` (signature, issuer, expiry, audience = the client
   id, `email_verified`), checks the allowlist, stores a session under the hash of a new random token, deletes the
   member's sessions that ended more than 30 days ago, and returns the token once.
3. The web app sends `Authorization: Bearer <token>` with every call. The `Session` authentication scheme finds the
   session by hash, checks that it is active and that its e-mail is still on the allowlist, and slides its expiry.
4. The authorization fallback policy requires an authenticated user for every endpoint that does not say
   `.AllowAnonymous()`. `POST /api/identity/sign-out` revokes the current session;
   `POST /api/identity/sign-out-everywhere` revokes every session of the member (a lost phone, a copied token).
5. A `GET` with the right `X-Deploy-Check-Key` authenticates as the read-only service `deploy-check` (scheme
   `DeployCheckKey`), so the deploy can check that every section reads; the header does nothing on other methods. The
   scheduler key (`X-Scheduler-Key`) only runs the ticks and reads nothing
   ([ADR 0045](../decisions/0045-separate-deploy-check-key.md)).

## Who is signed in, for other modules

`Identity.Contracts` exposes `ICurrentMember.Get()`, which returns `SignedInMember(Email, Name, Person)` with
`HouseholdPerson` `Vitaliy` or `Maria` (by the allowlist order), or `null` for the deploy check key and outside a request
([ADR 0040](../decisions/0040-current-member-through-identity-contracts.md)). `Identity.Api` implements it
(`Authentication/CurrentMember`, scoped, over `IHttpContextAccessor`). Notes uses it for the author of a note; no other
module reads claims or `HttpContext.User`, which `ModuleBoundaryTests.Only_TheIdentityModule_ReadsClaims` checks.

## Anonymous endpoints

| Endpoint | Why it is open |
| --- | --- |
| `GET /health`, `GET /api/system/status` | deploy and uptime checks, no family data |
| `GET /openapi/v1.json` | the contract is committed to the public repository anyway |
| `POST /api/identity/sign-in` | the way in (rate limit `SignIn`) |
| `POST /internal/outbox/tick`, `POST /internal/notifications/tick` | the scheduler key header |
| `POST /api/notifications/telegram/webhook` | Telegram's secret header |
| `POST /api/expenses/imports/sheet` | the import key header (unset in production) |

`AuthorizationRulesTests` fails when another file calls `AllowAnonymous`, and `IdentityProtectionApiTests` calls every
`GET` of the OpenAPI document without a token and expects `401`.

## HTTP API

All under `/api/identity`, tag `Identity`.

| Operation | What it does |
| --- | --- |
| `POST /sign-in` (`signIn`) | `{ credential }` → `200 { token, expiresAtUtc, member }`; `401 identity.google_token_invalid`, `403 identity.not_a_member`, `409 identity.not_configured`, `429` |
| `POST /sign-out` (`signOut`) | `204`; the token stops working at once |
| `POST /sign-out-everywhere` (`signOutEverywhere`) | `204`; every session of the member ends, this one included; other members stay signed in |
| `GET /me` (`getCurrentMember`) | `200 { email, name, pictureUrl, person }` (person: `Vitaliy`, `Maria` or null, spec 002) |

Every other operation of the API answers `401` with a problem body without a session.

## Configuration

| Setting | Where | Meaning |
| --- | --- | --- |
| `Identity:Google:ClientId` | App Service `Identity__Google__ClientId` | the Google OAuth client id (Web application); not a secret. Empty: sign-in answers `409` and the site stays closed. |
| `Identity:AllowedEmails` | App Service `Identity__AllowedEmails__0`, `__1` | the family's Google e-mails: `__0` Vitaliy, `__1` Masha |
| `DeployCheck:Key` | App Service `DeployCheck__Key` = GitHub secret `DEPLOY_CHECK_KEY` | the read-only deploy checks; never given to an outside service. Empty: the header authenticates nobody. |

Setup steps: [deployment](../../../docs/deploy.md#sign-in-with-google).

## Database

`identity.sessions` (unique `token_hash`, index on `email`). Migration `Initial`; the deploy runs the `identity`
bundle first.

## Tests

- Unit: `Session` (start, touch, revoke, stale), `SessionTokens`, `FamilyAllowlist`, `CurrentMember`, `SignInHandler` with a fake
  verifier, `SessionAuthenticator`.
- Architecture: `AuthorizationRulesTests` (the anonymous list).
- Integration: `IdentityApiTests` (sign-in, refusals, sign-out, sign-out everywhere, `me`, allowlist on every request, expiry slide),
  `IdentityProtectionApiTests` (every `GET` is `401` without a token, the deploy check key reads but does not write, the scheduler key reads nothing). The
  test host signs in through the real endpoint with a fake Google verifier; `ApiFactory.CreateHttpsClient()` is
  signed in, `CreateAnonymousClient()` is not.
