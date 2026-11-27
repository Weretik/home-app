# Quality targets

The numbers a design, a review or an audit of the backend is measured against. Every value below is **proposed, to
confirm** with Vitaliy; none is measured yet ("not measured"). A feature on a hot path states its expected load
against these targets in its SDD.

| Target | Proposed value | Current value | How to measure |
| --- | --- | --- | --- |
| P95 / P99 of a read (`GET /api/expenses…`, summary included) | 300 ms / 800 ms, warm instance | not measured | App Service request metrics (Azure Monitor), or `curl -w '%{time_total}'` over 100 calls |
| P95 / P99 of a write (create, edit, delete, record) | 400 ms / 1 000 ms, warm instance | not measured | as above |
| First request after the free tier sleeps | under 20 s, accepted | not measured | time the first call of the day |
| Peak load | 2 people, under 1 request per second; `PublicWrite` limit 60 writes a minute per address | not measured | App Service request count |
| Data volume | about 100 expenses a month, 1 200 a year; up to 30 plans a month | about 2 months of rows (import of October 2026 on) | `SELECT count(*)` per table |
| Availability | best effort on the free tier; a day of downtime is acceptable | not measured | health endpoint `GET /api/system/status` |
| RPO / RTO | RPO 24 h, RTO 1 day | not verified: the backup schedule of the PostgreSQL host (adm.tools) is not recorded | confirm the host's backups, then restore one to a scratch database once |
| Freshness of synced data | none: no data is synced from outside since the one-time sheet import | — | — |
| Monthly cost | 0 extra: free App Service and Static Web Apps; PostgreSQL inside the already paid adm.tools plan | 0 extra | the bills ([hosting](../../../docs/decisions/0002-azure-free-hosting.md), [database](../../../docs/decisions/0003-postgres-at-hosting-provider.md)) |
| Alerts | none named; failures are seen by the two users | none | — |

## Notes

- The month summary reads one grouped sum per category and month since `LimitsFrom` (about 14 × 12 rows a year,
  [ADR 0027](../decisions/0027-limit-carry-over-like-the-sheet.md)); at the volumes above it stays far inside the read
  target. Measure before storing monthly totals.
- When a number is confirmed or measured, change "proposed" to the agreed value and fill the current value with its
  date and source.
- The Telegram bot adds scheduler load, not user load: two keyed calls every 15 minutes (192 a day), each a few
  indexed reads and at most a handful of Telegram calls; the 15-minute tick is also the delivery delay of integration
  events ([ADR 0034](../decisions/0034-scheduler-github-actions-cron.md)). `notifications.deliveries` grows by about
  5 rows a day (under 2 000 a year).
- Sign-in (ADR 0037) adds one indexed lookup of `identity.sessions` by token hash to every protected request, and at
  most one update an hour per session; at two users this stays far inside the read target. `identity.sessions` holds
  a few rows per device; ended ones are deleted 30 days later at the member's next sign-in.
