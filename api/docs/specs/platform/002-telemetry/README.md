# 002 — Telemetry and free alerts

- **Service:** `api/`, `BuildingBlocks.Api` and the host
- **Architecture source:** [architecture](../../../architecture/README.md), [quality targets](../../../architecture/quality-targets.md)
- **Status:** accepted for delivery
- **Owner:** Vitaliy · **Created:** 2026-11-27 · **Updated:** 2026-11-27
- **Decision:** [ADR 0044](../../../decisions/0044-telemetry-in-application-insights.md)
- **Review finding:** F1 of the [2026-11-27 review](../../../../../docs/reviews/2026-11-27-architecture-before.md)

The speed targets PERF1 and PERF2 cannot be measured and nobody is told when the site is down. Written from the backend
SDD template (Weretik/obsidian `Templates/backend-sdd`), condensed: no contract, schema or data change.

## Scope

| ID     | Requirement |
| ------ | ----------- |
| FR-001 | When `APPLICATIONINSIGHTS_CONNECTION_STRING` is set, the API sends requests (with durations), dependencies (database, outgoing HTTP), exceptions and logs to Application Insights through OpenTelemetry. |
| FR-002 | When it is not set (local runs, tests, CI), nothing is registered and nothing is sent. |
| FR-003 | `/health` requests are not traced. |
| FR-004 | No request or response bodies and no family data are added to telemetry; only what the instrumentation and the existing log messages record. |
| FR-005 | Free alerts: an uptime job on `/health` every 15 minutes with e-mail after two failures; a 0.1 GB daily cap (smart detection e-mails optional, not set up) ([deploy guide](../../../../../docs/deploy.md#telemetry-and-alerts)). |
| FR-006 | The deploy guide has the queries that read PERF1 and PERF2; the measured values go into the quality targets with a date. |

## Design

- `BuildingBlocks.Api/Telemetry/TelemetryRegistration.AddTelemetry` calls `AddOpenTelemetry().UseAzureMonitor()`
  (package `Azure.Monitor.OpenTelemetry.AspNetCore`) and sets the ASP.NET Core trace filter to
  `QuietPaths.ShouldTrace`. `PlatformRegistration` in the host calls it once.
- Rate limit: no new endpoint. Idempotency: nothing is written. Errors: unchanged.
- Data volume: two people and 192 scheduler calls a day, a few MB a day; under the 5 GB free allowance a month.

## Tests

| ID     | Check |
| ------ | ----- |
| TS-001 | `TelemetryRegistrationTests`: no connection string → nothing registered; with one → the trace filter skips `/health` and keeps `/api/...`. |
| TS-002 | `TelemetryRegistrationTests.Only_TheHealthPing_IsQuiet` covers `/health`, `/health/ready`, `/healthy`, the status and tick paths. |
| TS-003 | Architecture tests pass (folders, one type per file, packages only in `Directory.Packages.props`); `openapi.json` unchanged. |
| TS-004 | After deploy: requests appear in Application Insights → Live metrics; the uptime job is green. |
