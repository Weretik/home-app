# 0044. Telemetry goes to Application Insights through OpenTelemetry, alerts stay free

Status: accepted, 2026-11-27.

## Context

The 2026-11-27 architecture review ([before](../../../docs/reviews/2026-11-27-architecture-before.md), finding F1)
found no telemetry and no alerts: the speed targets PERF1 (API P95 ≤ 0.4 s) and PERF2 (first request after sleep
≤ 3 s) cannot be measured, and an outage is noticed by Masha or Vitaliy. The cost target COST1 is 0 ₴ on top of the
database.

## Options

| Option | Cost | Effort | Gives |
| --- | --- | --- | --- |
| A. Application Insights through the Azure Monitor OpenTelemetry distro, with a daily cap | 0 ₴ under 5 GB a month (two people send a few MB) | one package, a few lines, portal setup | requests with durations (P95), dependencies (database, Telegram, Google, push), exceptions, logs, free smart detection of failure and slowness anomalies |
| B. App Service built-in metrics and log stream only | 0 ₴ | none | request count and average time, no percentiles per endpoint, no dependencies, no exceptions with context |
| C. Another vendor (Grafana Cloud, Honeycomb, Sentry) | free tiers | similar to A, plus one more outside account | similar, data outside Azure |
| Do nothing | 0 ₴ | none | the targets stay unmeasured |

Availability: Application Insights standard availability tests are charged per run, so they break COST1. The free
cron-job.org account (already the scheduler, [ADR 0043](0043-external-cron-and-scheduler-heartbeat.md)) can call
`/health` every 5 minutes and e-mail on failure.

## Decision

Option A.

- `BuildingBlocks.Api/Telemetry` adds `Azure.Monitor.OpenTelemetry.AspNetCore` (`UseAzureMonitor()`) only when
  `APPLICATIONINSIGHTS_CONNECTION_STRING` is set, which Azure does when Application Insights is attached to the App
  Service. Local runs, tests and CI send nothing.
- `/health` requests are not traced (`QuietPaths`): they carry no family data and the uptime check records them.
- Logs already written with `[LoggerMessage]` and the Mediator `LoggingBehavior` are exported as they are; no custom
  metrics yet.
- No family data is added to telemetry on purpose: request paths, status codes, durations and log messages only.
  Request bodies are never recorded.
- Alerts, all free: a cron-job.org job on `/health` every 5 minutes with e-mail on failure (outage); Application
  Insights smart detection e-mails on failure and performance anomalies (errors, slowness); a daily cap of 0.1 GB on
  the Application Insights resource (cost). Setup steps: [docs/deploy.md](../../../docs/deploy.md#telemetry-and-alerts).
- PERF1 and PERF2 are read from the Application Insights `requests` table (queries in the deploy guide) and written
  into the quality targets with a date.

## Consequences

- One more Azure resource (Application Insights with its Log Analytics workspace), still 0 ₴ under the cap.
- When the daily cap is reached, telemetry stops until the next day; the site keeps working.
- The `/health` ping every 5 minutes also keeps the free instance warmer, which helps PERF2 and spends a little of the
  60 CPU minutes a day.
- Revisit when the data passes 1 GB a month, when an alert is needed that smart detection cannot give (then a paid
  metric alert, about $0.10 a month each), or when the web app needs client-side error tracking.
