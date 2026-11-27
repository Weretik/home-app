# 0045. The deploy check reads with its own key; the scheduler key only runs the ticks

Status: accepted, 2026-11-27. Amends [ADR 0037](0037-google-sign-in-with-server-sessions.md) and root
[ADR 0005](../../../docs/decisions/0005-post-deploy-smoke-reads-every-section.md).

## Context

Since sign-in (ADR 0037) the post-deploy smoke test read every section with the scheduler key (`X-Scheduler-Key` =
`OutboxTick:Key`): a `GET` with it authenticated as the read-only service `scheduler`. The same key is stored outside
GitHub, in the cron-job.org jobs that call the ticks (ADR 0043). A leak of that outside account therefore exposed all
family data read-only, although the scheduler itself never needs to read anything (finding F2 of the
[2026-11-27 architecture review](../../../docs/reviews/2026-11-27-architecture-before.md); audit note AF-001 of spec
identity/001).

## Decision

- Two keys with one job each:
  - the **scheduler key** (`OutboxTick:Key`, header `X-Scheduler-Key`) only runs `POST /internal/outbox/tick` and
    `POST /internal/notifications/tick`; it no longer authenticates any read;
  - the **deploy check key** (`DeployCheck:Key`, header `X-Deploy-Check-Key`) authenticates a `GET` or `HEAD` as the
    read-only service `deploy-check` (scheme `DeployCheckKey`), compared in constant time. Any other method gets no
    identity from it, and an empty key turns it off.
- The deploy check key lives only in the App Service setting `DeployCheck__Key` and the GitHub secret
  `DEPLOY_CHECK_KEY` of the API workflow. It is never given to an outside service.
- The smoke test reads every section with the deploy check key and also checks that the scheduler key gets `401` on a
  read.

## Options considered

- **Keep one key** (no work): a leak of cron-job.org keeps exposing every section. Rejected.
- **A signed-in test member for the smoke test:** needs a Google account and a refresh flow in CI; much more work for
  the same result. Rejected.
- **Two keys** (chosen): one setting, one secret, one handler; the outside account can only trigger ticks, which are
  idempotent.

## Consequences

- A leaked scheduler key lets someone run the ticks early, nothing more: deliveries have unique keys and the outbox
  handlers are idempotent.
- One more secret to set once (`openssl rand -hex 32`, the same value in App Service and GitHub, see
  [docs/deploy.md](../../../docs/deploy.md)). Without it the smoke test fails with a clear error.
- Revisit if the smoke test needs to write (it should not) or if an MCP server or another client needs a service
  identity: that gets its own key and scheme, never this one.
