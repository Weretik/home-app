# 003 — Session limit and sign out everywhere

- **Module:** `Identity` (`api/src/Modules/Identity`), no schema change
- **Architecture source:** [architecture](../../../architecture/README.md), [rules](../../../rules.md),
  [identity 001](../001-google-sign-in/README.md)
- **Status:** implemented, audited
- **Owner:** Vitaliy · **Created:** 2026-11-28
- **Decision:** [ADR 0046](../../../decisions/0046-session-max-age-and-sign-out-everywhere.md)
- **Frontend SDD:** [web/docs/specs/identity/003-sign-out-everywhere](../../../../../web/docs/specs/identity/003-sign-out-everywhere/README.md)
- **Why:** finding F5 of the [2026-11-27 architecture review](../../../../../docs/reviews/2026-11-27-architecture-before.md), quality target SEC2.

Written from the backend SDD template (Weretik/obsidian `Templates/backend-sdd`), condensed to one page.

## Scope

In:
- an upper session lifetime;
- one endpoint that ends every session of the member.

Out:
- a list of devices;
- ending one other device;
- refresh tokens;
- a Content Security Policy in the web app (later).

## Rules

| ID    | Rule                                                                                                                                                                         |
| ----- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| R-001 | A session ends 90 days after its sign-in at the latest, however often it is used. Sliding (30 days after the last use) never moves the expiry past that moment.           |
| R-002 | The limit applies to sessions made before the change too: `IsActiveAt` checks `CreatedAtUtc + 90 days`.                                                                    |
| R-003 | An ended session is deleted 30 days after its end (the existing R-012 of identity 001). The end is now the earliest of the revoke, the expiry and the 90-day limit.        |
| R-004 | `POST /api/identity/sign-out-everywhere` revokes every active session of the caller's e-mail, the calling one included. Other members' sessions stay.                     |
| R-005 | A repeat, or a call with an ended token, gets `401` from the fallback policy and changes nothing.                                                                           |

## Contract

`POST /api/identity/sign-out-everywhere`, operation `signOutEverywhere`, tag `Identity`, no body.

- `204` on success.
- `401` without an active session.
- `429` from the `PublicWrite` rate limit, the same as sign-out. It is a signed-in write of one member, so no
  dedicated policy is needed.
- No `Idempotency-Key`: a repeat is harmless (R-005).

## Design

- `Session.MaxAge` (90 days) and `EndsAtLatestUtc`. `IsActiveAt` adds `now < EndsAtLatestUtc`. `Touch` takes the
  earlier of `now + Lifetime` and `EndsAtLatestUtc`, and `IsStaleAt` counts from that end too.
- `Features/Sessions/Commands/SignOutEverywhere/` holds the command, the handler and the validator. The handler:
  1. reads the e-mail of the token's session (projection);
  2. loads the member's unrevoked sessions in one query (a member has a few);
  3. revokes each and calls `SaveChanges` once.
- `Endpoints/SignOutEverywhereEndpoint.cs`, mapped after sign-out.

## Tests

- Unit `SessionTests`:
  - a session touched every day ends at day 90;
  - a session that slid before the limit ends at the limit and is stale 30 days later.
- Unit `SignInValidatorTests`: the token rules of the new command.
- Integration `IdentityApiTests.Sign_Out_Everywhere_EndsEverySessionOfTheMember_AndNoOtherMembers`:
  - both of Masha's devices get `401`;
  - Vitaliy's session stays;
  - a repeat gets `401`.

## Audit

| Check             | Result                                                                                                                       |
| ----------------- | ---------------------------------------------------------------------------------------------------------------------------- |
| Layers            | ✅ The rule lives on the `Session` root. The use case is in its own folder. The endpoint only maps.                           |
| Folders           | ✅ `Commands/` has 3 use-case folders, `SignOutEverywhere/` 3 files, `Endpoints/` 4 files.                                    |
| Data              | ✅ No query in a loop, one `SaveChanges`, no schema change.                                                                   |
| Contract          | ✅ `WithName`, `WithSummary`, `Produces`, `ProducesProblem` on the endpoint; `openapi.json` is regenerated in CI and diffed.   |
| Rate limit        | ✅ `PublicWrite`.                                                                                                             |
| Idempotency       | ✅ A repeat cannot do the work twice: the token is revoked by then.                                                           |
| Anonymous list    | ✅ Unchanged; the endpoint needs a session.                                                                                   |
