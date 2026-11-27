# 0005. Tests use real-format secrets, outside clients run for real, and the deploy reads every section

**Status:** accepted, 2026-11-27; amended 2026-11-27 by [API ADR 0037](../../api/docs/decisions/0037-google-sign-in-with-server-sessions.md) (reads with the scheduler key) and 2026-11-27 by [API ADR 0045](../../api/docs/decisions/0045-separate-deploy-check-key.md) (its own deploy check key)

## Context

The Telegram bot (#23) passed unit, architecture and integration tests and its deploy went green, yet the «Телеграм»
page answered 500 in production. A BotFather token has a colon (`123456:ABC…`); the client built the relative url
`bot123456:ABC…/getMe`, which .NET reads as an absolute uri with the scheme `bot123456`, and HttpClient refused it
(fixed in #24). Three gaps let it through:

1. The unit tests used a made-up token without a colon.
2. The integration tests replaced the whole Telegram client with a fake, so the code that builds the request never
   ran in a test.
3. The post-deploy smoke test only called `/health`, which proves the app and the database are up, not that a section
   can load.

## Decision

- **Secrets in tests have the real format** of the service that issues them (a Telegram token is
  `<digits>:<35 characters>`); a format that changes parsing is a test case of its own.
- **An outside client is tested as it runs in production:** the real client class and its `HttpClient` registration,
  with only the network replaced by a stub that records the request urls
  (`IntegrationTests/Api/Notifications/NotificationsTelegramHttpTests`). A fake of the whole client stays fine for
  testing the callers' logic, but does not count as a test of the client.
- **The deploy smoke test reads every section:** after `/health` reports the new commit, the API workflow calls the
  read endpoint each section page loads first (`/api/system/status`, expenses summary and budget, shopping, gifts and
  todos summaries, `/api/notifications/status`) and fails the deploy on any answer other than 200. A new section adds
  its endpoint to that list in the same change.

## Consequences

- A bad setting, a missing migration or an outside call that throws now turns the deploy red within minutes instead of
  being found by Vitaliy on the site. The smoke test cannot roll back by itself: the fix still goes through a pull
  request, but the failure is visible on the commit at once.
- The smoke test only reads, so it changes nothing in production; it costs a few requests per deploy.
- The list of endpoints is kept by hand in `.github/workflows/api.yml`; forgetting a new section is a review finding.

## Amendment, 2026-11-27: sign-in

Since sign-in ([API ADR 0037](../../api/docs/decisions/0037-google-sign-in-with-server-sessions.md)) every section
answers `401` without a session. The smoke test sends `X-Scheduler-Key` with the GitHub secret `SCHEDULER_KEY`; a
`GET` with that key reads as the read-only service `scheduler`. A last step checks that the API answers `401` without
a session and that sign-in is configured (a made-up Google token gets `401`, not `409`).

## Amendment, 2026-11-27: its own key

The scheduler key is also stored in cron-job.org, so it no longer reads anything ([API ADR
0045](../../api/docs/decisions/0045-separate-deploy-check-key.md)). The smoke test sends `X-Deploy-Check-Key` with the
GitHub secret `DEPLOY_CHECK_KEY` (= App Service `DeployCheck__Key`); a `GET` with it reads as the read-only service
`deploy-check`. It also checks that the scheduler key gets `401` on a read.
