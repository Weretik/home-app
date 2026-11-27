# Quality targets

The numbers a design, a review or an audit of home-app (API and web) is measured against. Vitaliy set the order of
importance and confirmed every threshold on 2026-11-27: **security → offline use → cost → ease of change → speed**.
Each target is a scenario: situation → what must happen → threshold → how it is checked. A feature on a hot path
states its expected load against these targets in its SDD; a review marks each target met, partly met, missed or
not measurable, with evidence.

The latest review against these targets: [architecture review, 2026-11-27, before](../../../docs/reviews/2026-11-27-architecture-before.md).

## Targets

| Id | Attribute | Situation | What must happen | Threshold | How it is checked | Current value |
| --- | --- | --- | --- | --- | --- | --- |
| SEC1 | Security | anyone but Masha and Vitaliy opens the site or the API | sees and changes nothing | 100 % of requests without a session get `401`, except the listed anonymous endpoints | `AuthorizationRulesTests` and a manual call without a token | met (2026-11-27) |
| SEC2 | Security | a session token is stolen from a device | the harm is limited in time | a session ends after 30 days without use; sign-out revokes it at once | session code and tests | met as written; see the review, finding F5 |
| SEC3 | Security | the key held by an outside service (scheduler, Telegram, CI) leaks | no family data is exposed | 0 outside keys can read family data | the list of keys and what each can do | missed (2026-11-27) |
| OFF1 | Offline use | no internet in the shop | the list opens, items and to-dos can be ticked | 100 % of the shopping and to-do screens work offline | manual check in airplane mode | met by code (2026-11-27); manual check pending |
| OFF2 | Offline use | an expense is recorded without internet | it shows at once and is not lost | sent within 1 minute after the network is back, exactly once, even if the app was closed | queue tests and a manual check | missed (2026-11-27) |
| COST1 | Cost | an ordinary month | hosting costs nothing | 0 ₴ on top of the already paid database | Azure bills | met (free tiers, 2026-11-27) |
| PERF1 | Speed | a section is opened on a warm server | data appears quickly | API P95 ≤ 0.4 s | telemetry | not measurable: no telemetry (2026-11-27) |
| PERF2 | Speed | the first request after the free tier sleeps | almost no wait | ≤ 3 s (about 1–2 s by eye) | time of the first request of the day | not measurable: no telemetry (2026-11-27) |
| MOD1 | Ease of change | a new section is added | it is clear where everything goes | a new section by the README recipe in one evening | time spent on the next section | recipe in place ([README](../../README.md#add-a-module)); time not measured |
| MOD2 | Ease of change | an architecture rule is broken by accident | it is noticed at once | CI is red in the same pull request | architecture tests and web structure checks | met (2026-11-27) |
| MOD3 | Ease of change | looking for where a piece of code lives | it is found quickly | any file at most 2 steps from the module map | try 3 random files | met (2026-11-27) |

## Load and data (for designs)

- Users: 2 people, under 1 request per second; `PublicWrite` limit 60 writes a minute per address.
- Scheduler: two keyed calls every 15 minutes (192 a day), each a few indexed reads and at most a handful of
  Telegram calls; the 15-minute tick is also the delivery delay of integration events
  ([ADR 0034](../decisions/0034-scheduler-github-actions-cron.md), [ADR 0043](../decisions/0043-external-cron-and-scheduler-heartbeat.md)).
- Volumes: about 100 expenses a month (1 200 a year), up to 30 plans a month, hundreds of notes and about ten note
  categories, `notifications.deliveries` about 5 rows a day.

## Notes

- The month summary reads one grouped sum per category and month since `LimitsFrom` (about 14 × 12 rows a year,
  [ADR 0027](../decisions/0027-limit-carry-over-like-the-sheet.md)); at these volumes it stays far inside PERF1.
  Measure before storing monthly totals.
- Sign-in ([ADR 0037](../decisions/0037-google-sign-in-with-server-sessions.md)) adds one indexed lookup of
  `identity.sessions` by token hash to every protected request and at most one update an hour per session.
- Notes ([ADR 0039](../decisions/0039-notes-module-model.md)): the list is one query with a `LIKE` word search
  without an index and `Take(1000)`; at these volumes a sequential scan stays inside PERF1. Measure before adding a
  trigram index or paging.
- When a value is measured, write it into "Current value" with its date and source.
