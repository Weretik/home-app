# 0046. A session ends 90 days after sign-in at the latest; a member can sign out everywhere

Status: accepted, 2026-11-28. Amends [ADR 0037](0037-google-sign-in-with-server-sessions.md).

## Context

A session lived 30 days after its last use and slid on every use (ADR 0037), so a token in regular use never
expired. A copied token, or a phone that was lost while still signed in, kept full access until the member signed out
on that very device, which they no longer have. This is finding F5 of the
[2026-11-27 architecture review](../../../docs/reviews/2026-11-27-architecture-before.md) (quality target SEC2).

## Decision

- **An upper limit.** A session still slides 30 days after its last use, but never past 90 days after the sign-in
  (`Session.MaxAge`, `EndsAtLatestUtc`). `IsActiveAt` checks both, so sessions made before this change are capped
  too. Signing in again with Google takes a tap, so the cost is one sign-in a quarter per device.
- **Sign out everywhere.** `POST /api/identity/sign-out-everywhere` (`signOutEverywhere`) revokes every active session
  of the signed-in member, the calling one included, in one save. Another member's sessions stay. A token that is
  already gone answers `401` like every closed endpoint. The rate limit is `PublicWrite`, as for sign-out.
- The web profile offers «Вийти на всіх пристроях» after a confirmation. Unlike «Вийти» it is not best effort: if the
  server does not confirm, the device stays signed in and the error is shown.

## Options considered

- **Keep the sliding session only** (no work): a stolen token in use never ends. Rejected.
- **A shorter sliding lifetime** (7 days): the family would sign in again after every week off. Rejected.
- **Refresh tokens with rotation:** detects a copied token, but needs a second token, storage and races in the web
  app. Too much for two people. Revisit if the API gets outside clients (the MCP server uses its own OAuth flow).

## Consequences

- No schema change: `created_at_utc` and `expires_at_utc` already exist.
- Every device signs in again at least once every 90 days.
- A lost phone is closed from any other device in two taps.
- The token stays in `localStorage`. A Content Security Policy is still to do (the "CSP later" part of F5).
