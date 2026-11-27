# Code audit — Identity module (Google sign-in)

- **Feature:** [identity/001-google-sign-in](README.md)
- **Scope:** the new module `src/Modules/Identity` (five projects, `Initial` migration); the host registration
  (`Program.cs`, `MediatorSetup`, `Host.Api.csproj`, `PlatformRegistration`, `ApiDocumentation`); `AllowAnonymous`
  on the health, tick, webhook and import endpoints; `Directory.Packages.props` (`Google.Apis.Auth`); `api.yml`
  (bundle, smoke steps); the test host and the new unit, architecture and integration tests; docs this change touched.
- **Stage:** final audit after implementation (skill `backend-code-audit`).
- **Date:** 2026-11-27
- **Verdict:** **pass with open findings.** No blocker. Every finding is Low or Medium and fixed or accepted with a
  reason.

## Step 0: quality targets

[quality-targets.md](../../../architecture/quality-targets.md) exists; every value is still *proposed, to confirm*
and *not measured* (recorded there, not repeated here). This change adds one indexed lookup per protected request and
at most one write an hour per session; a note with these numbers was added to the file. No speed verdict beyond that.

## Boundaries and layers

- [x] `Identity.Application` references only its Domain, Contracts and `BuildingBlocks.Application`; Google lives only
  in `Identity.Infrastructure/GoogleTokenVerifier` behind the port `IGoogleTokenVerifier`. No other module references
  Identity; other modules only get the fallback policy. `ModuleBoundaryTests`, `LayerDependencyTests` pass.
- [x] Domain (`Session`, `MemberProfile`, `IdentityErrors`) has no EF Core or ASP.NET Core; `Session` keeps its rules
  (expiry, slide, revoke, stale) behind methods with private setters; unit tested.
- [x] Schema `identity` with its own history table; no foreign keys; `DataOwnershipTests` and the migration check
  pass.
- [x] Use cases `Features/Sessions/Commands/SignIn|SignOut` with command, handler, validator; endpoints map a request
  record to the command and the result through `ToHttpResult`. `GET /me` reads the claims the handler set and runs
  no use case (nothing to do in Application).
- [x] `SessionAuthenticator` is an Application service, not a use case: it runs inside the authentication handler
  before any endpoint (design, "Application"). Scoped, used by a per-request handler; no singleton holds it
  (`ValidateScopes` passes in the test host).
- [x] Host only composes (`AddIdentityModule`, `MapIdentityModule`); the module owns the schemes and the policy.
- [x] Folder axiom: no folder over six code files, no empty, wrapper or one-file folder (architecture test passes;
  `FamilyAllowlist` and `FamilyMember` moved to `Features/Members` so `Features/` does not wrap one folder).

## Access (the point of the feature)

| Check | Evidence |
| --- | --- |
| Every endpoint closed by default | fallback policy in `IdentityModule`; `AuthorizationRulesTests.The_IdentityModule_SetsTheFallbackPolicy` |
| Anonymous only from the list, each with a reason | `AuthorizationRulesTests.Only_TheListedEndpoints_AreAnonymous`, `Nothing_ProtectsOrOpens_AnEndpoint_ExceptTheFallbackPolicy_AndThatList` |
| Every documented `GET` is `401` without or with a forged token, with a problem body | `IdentityProtectionApiTests.Every_Read_NeedsASession` (27 reads) |
| Writes too | `IdentityProtectionApiTests.A_Write_NeedsASession_Too` |
| Scheduler key reads, never writes, never `/me`; a wrong key reads nothing | `IdentityProtectionApiTests` |
| Allowlist on every request, expiry, slide, sign-out of one device, not configured → closed | `IdentityApiTests` |
| Tokens stored only as SHA-256; constant-time compare of the scheduler key; no client secret | `SessionTokens`, `SchedulerKey`, design |

## Data and idempotency

| Write | Commits | Repeat or race |
| --- | --- | --- |
| `signIn` | one `SaveChanges` (new session + stale sessions removed) | a repeat stores a second session, harmless; no `Idempotency-Key` on purpose (design, "Contract"). A token-hash collision is impossible in practice (256-bit random). |
| `signOut` | one `SaveChanges` | revoke is idempotent; a second call answers `401` because the token no longer authenticates. |
| session slide (authenticator) | at most one `SaveChanges` an hour per session | two parallel requests may both write the same new expiry; last write wins and the values are equal within milliseconds. No guard needed. |

## Findings

| ID | Severity | Finding | Evidence | Status |
| --- | --- | --- | --- | --- |
| AF-001 | Medium | The scheduler key now also reads every section; a leaked `SCHEDULER_KEY` exposes the data read-only. | `SchedulerKeyAuthenticationHandler` | accepted: needed for the deploy smoke checks (root ADR 0005); the key was already a production secret in two places only; recorded in ADR 0037 "Consequences". Resolved 2026-11-27 by ADR 0045: the deploy reads with its own `DEPLOY_CHECK_KEY`, the scheduler key reads nothing. |
| AF-002 | Medium | The web app keeps the session token in `localStorage`, readable by any script on the site. | web ADR 0015 | accepted: cross-site cookies are blocked between the two Azure hosts; the site runs only its bundle and Google's script; sign-out and the allowlist end a stolen session. |
| AF-003 | Low | Personal data: e-mail, name and picture URL of the two members are stored per session. | `identity.sessions` | accepted: needed to show who is signed in; ended sessions are deleted 30 days later at the next sign-in (R-012); nothing else is kept. |
| AF-004 | Low | Sessions of a member who never signs in again are not cleaned up. | `SignInHandler` | accepted: at most a few rows per device of two people; no background work on the free tier (ADR 0006). |
| AF-005 | Low | The authenticator reads the session tracked (not `AsNoTracking`). | `SessionAuthenticator` | accepted: it may update the row (slide) in the same request; one row by unique index. |
| AF-006 | Low | Notifications SDD R-019 still said site writes are anonymous. | `specs/notifications/001-telegram-bot/requirements.md` | fixed: R-019 notes sign-in since 2026-11-27. |
| AF-007 | Low | No action journal of sign-ins. | — | accepted: two users; `created_at_utc` and `last_seen_at_utc` per session answer "when and from how many devices". |

## Architecture records

- [x] ADR 0037 (accepted) records the decision, options and consequences; ADR 0015 marked superseded; 0009 closed in
  the index; root ADR 0005 amended for the smoke checks; web ADR 0015 for the client side.
- [x] C4: Google Identity Services in the context diagram, the Identity module in the component diagram (the planned
  box removed).
- [x] Module page `modules/identity.md`, rules (closed by default, enforcement row), deploy guide (Google steps,
  variables), AGENTS.md, API README configuration.

## Verification

- `dotnet build HomeApp.slnx`: success, 0 warnings; `openapi.json` regenerated (three Identity operations).
- Unit tests: 392 passed (21 new for Identity).
- Architecture tests: 85 passed (3 new).
- Integration tests (Testcontainers, PostgreSQL 17): 171 passed (66 new, including the 27-read sweeps), the existing
  ones running through a signed-in client.
