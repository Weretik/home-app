# 0043. cron-job.org calls the ticks, GitHub cron is the backup, and the API keeps a heartbeat

Status: accepted, 2026-11-27. Amends [ADR 0034](0034-scheduler-github-actions-cron.md).

## Context

On the first night of the bot (26 → 27.11) GitHub ran the `*/15 * * * *` schedule of `scheduler.yml` only once, at
05:20 UTC (07:20 Kyiv), before the 08:00–11:59 morning window. No tick came inside the window, so no morning digest
was planned and nothing reached Telegram. Nothing showed it: the page looked normal and the deploy was green. GitHub
documents that scheduled runs may be delayed or dropped under load; for a new repository with little activity it drops
most of them.

## Decision

- The main scheduler is a free [cron-job.org](https://cron-job.org) account with two jobs every 15 minutes:
  `POST /internal/notifications/tick` and `POST /internal/outbox/tick`, each with the header
  `X-Scheduler-Key` = `OutboxTick__Key` (setup in [docs/deploy.md](../../../docs/deploy.md#scheduler)). The
  GitHub cron stays as a backup; both are safe together because deliveries have unique keys and outbox handlers are
  idempotent.
- Every keyed notifications tick first records its time in `notifications.scheduler_heartbeat` (one row seeded by the
  migration, one set-based update in its own command `RecordTick`, before `RunTick`, so a failing plan still counts).
- `GET /api/notifications/status` returns `lastTickAtUtc` and `schedulerSilent` (no tick for more than an hour, or
  never). The «Телеграм» page shows a red alert when it is silent, and the deploy prints a GitHub warning (not a
  failure, because right after a first deploy no tick has come yet).

## Consequences

- One more outside account (cron-job.org), which ADR 0034 avoided; the cost is still zero.
- A stopped scheduler is visible on the page within an hour and on the next deploy, instead of on a quiet morning.
- One small table and one extra UPDATE per tick.
