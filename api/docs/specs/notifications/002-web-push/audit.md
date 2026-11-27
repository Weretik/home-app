# Code audit — Web Push next to Telegram

- **Feature:** [notifications/002-web-push](README.md)
- **Scope:** `Notifications.Domain/Push`, `Deliveries/` (channel, keys); `Notifications.Application/Push`,
  `Sending/PushMessage*`, `Digests/Texts/PushTexts`, `Digests/Planning` (page), `Features/Push/*`,
  `Features/Tick/RunTickHandler`, status delivery item; `Notifications.Infrastructure/Push`, persistence and the
  `WebPush` migration; `Notifications.Api/Endpoints/Push`; tests; `api.yml` smoke; docs.
- **Stage:** final audit after implementation (skill `backend-code-audit`).
- **Date:** 2026-11-27
- **Verdict:** **pass with accepted findings.** No blocker.

## Boundaries and layers

- [x] Application references only its own Domain/Contracts, `BuildingBlocks.Application`, the Contracts it already
  used and `Identity.Contracts` (for `ICurrentMember`, ADR 0040). `ModuleBoundaryTests`, `LayerDependencyTests` pass.
- [x] Domain knows no EF Core or HTTP; the protocol (encryption, VAPID, HTTP) lives only in `Infrastructure/Push`
  behind `IWebPush`.
- [x] No `throw` in Domain or Application (`HandlerRulesTests`); the "retry when every device failed" signal is
  thrown by the Infrastructure sender, as the Telegram client does.
- [x] Use cases in `Features/Push/Commands|Queries/<UseCase>/` with handler and validator; each command saves once;
  queries read without tracking.
- [x] One endpoint per file with `WithName`, `WithSummary`, `Produces`, `ProducesProblem`; writes rate limited; all
  endpoints behind the fallback policy (signed-in member); the scheduler key may read the status (smoke test).

## Data and migration

- [x] `WebPush` migration only adds: table `push_subscriptions` (unique endpoint, index on person) and
  `deliveries.channel` with default `'Telegram'`, so rows planned by the previous release during the deploy are right.
- [x] Idempotency: deliveries keep one unique key per channel; the subscription save is an upsert by its unique
  endpoint; the outbox handler is safe to repeat (a repeat pushes again at most; `sent_at_utc` is set once more).

## Security

- [x] The private key is read from configuration only; never logged; not in committed settings
  (`ConfigurationRulesTests`). Logs name the push service host, never the endpoint path.
- [x] A person lists, removes and tests only their own devices; another person's id answers 404 like a missing one.
- [x] Encryption verified against the RFC 8291 Appendix A vector and an independent decryption in the test.

## Folders and files

- [x] Every folder ≤ 6 code files: Application `Push/` 6, `Sending/` 5, `Digests/Texts/` 6, Infrastructure `Push/`
  5, Api `Endpoints/Push/` 5; integration tests split into `Notifications/Telegram/` and `Notifications/Push/`.

## Findings

| ID | Severity | Status | Finding | Decision |
| --- | --- | --- | --- | --- |
| BF-01 | low | accepted | Two first saves of the same endpoint at the same instant: the second hits the unique index and answers 500. | Needs the same browser to save twice within milliseconds; the page saves again at its next start. A catch would need a second save in the handler (rule `A_Command_SavesOnce`). |
| BF-02 | low | accepted | A partial failure (one device accepted, another refused for a transient reason) is not retried for the refused device. | Retrying would push the accepted device twice; recorded in ADR 0042 and R-108. |
| BF-03 | info | accepted | The sender posts to devices one after another. | Two people, a handful of devices; parallel sends add nothing measurable on F1. |

## Evidence

- Unit 489, architecture 93 green locally; CI on the pull request ran unit, architecture and integration (204) green
  for the backend commit.
