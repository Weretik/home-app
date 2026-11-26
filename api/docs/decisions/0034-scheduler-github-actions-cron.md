# 0034. The scheduler is a GitHub Actions cron

Status: accepted, 2026-11-26

## Context

[ADR 0006](0006-outbox-with-scheduled-tick.md) leaves the outbox and every scheduled job to an external scheduler
that calls an HTTP endpoint with a shared key. Until the Telegram bot nothing needed it. Now digests must go out at
fixed Kyiv times and the outbox must be drained. The API is on App Service F1 (no Always On, no WebJobs that run
continuously). Options:

- GitHub Actions `schedule` (cron): free and unlimited for a public repository, which `Weretik/home-app` is; runs can
  start several minutes late, and GitHub disables schedules in a repository without activity for 60 days.
- Azure Functions timer trigger: precise, but needs a storage account (a small but real monthly cost) and another
  resource to deploy and watch.
- An external cron service (cron-job.org and the like): free, but one more account outside GitHub and Azure.

Vitaliy chose GitHub Actions (2026-11-26).

## Decision

- `.github/workflows/scheduler.yml` runs on cron `*/15 * * * *` and on `workflow_dispatch`. Each run calls
  `POST /internal/notifications/tick`, then `POST /internal/outbox/tick`, both with
  `X-Scheduler-Key: ${{ secrets.SCHEDULER_KEY }}`, which equals the App Service setting `OutboxTick__Key`.
- When the secret is not set, the workflow ends with a notice instead of failing.
- Digests have windows of several hours (see the module page), so a late or skipped run is caught up by the next one
  inside the window; outside the window a digest is skipped rather than sent at an odd hour.

## Consequences

- No cost and nothing else to host. A run that wakes the sleeping API takes up to a minute; the workflow retries the
  call a few times.
- Times are accurate to roughly a quarter of an hour, which is enough for reminders.
- If the repository becomes private, the minutes count against the free quota (2 000 a month): the schedule would then
  need a longer interval or another scheduler, with a new ADR.
