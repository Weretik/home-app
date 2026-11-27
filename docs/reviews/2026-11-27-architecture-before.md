# Architecture review, 2026-11-27: before

The baseline review of home-app (API and web) before the improvement work. Level: Quick (questions whose failure
costs data, money or downtime, plus a pass over every quality target). Code at `main` `6f4a02e`. Method: obsidian
`Architecture/ArchitectPlan/analysis-by-steps.md` (context → targets in numbers → boundaries → internal structure →
check against targets → priorities and decisions). The same review is repeated after the fixes for a before → after
comparison.

## Context

A private site for two people (Masha and Vitaliy): expenses and incomes, shopping list, to-dos, gifts, notes about
each other, a Telegram bot and Web Push. The most valuable data is the family finances and the notes. One developer
maintains it. Order of importance: security → offline use → cost → ease of change → speed. The site is also Vitaliy's
pet project for architecture practice and AI features (an MCP server comes next).

Quality targets with their thresholds: [quality-targets.md](../../api/docs/architecture/quality-targets.md).

## Boundaries

| Outside party | What flows | Protection | Who holds the key | What the key can do |
| --- | --- | --- | --- | --- |
| Masha's and Vitaliy's browsers and phones | all family data, offline copy in IndexedDB | Google sign-in, e-mail allowlist, session token | the device (`localStorage`) | everything the member can do, for as long as the session is used |
| Google | ID token at sign-in | token signature, audience | Google | sign-in only |
| cron-job.org (main scheduler), GitHub Actions cron (backup) | tick calls every 15 minutes | `X-Scheduler-Key` header | cron-job.org, GitHub secret `SCHEDULER_KEY` | run the ticks **and read every section** (F2) |
| GitHub Actions deploy | migrations, post-deploy smoke | `DATABASE_CONNECTION_STRING`, `SCHEDULER_KEY` | GitHub `production` environment | **read and change all data** (F3) |
| Telegram | digests out, button presses in | bot token; webhook secret header | App Service setting | send as the bot; the webhook secret is derived from the token (F6) |
| Web Push services | digest texts | VAPID keys, RFC 8291 encryption | App Service setting | push only |
| PostgreSQL at adm.tools | all data | connection string | App Service setting, GitHub secret | everything |
| Azure App Service and Static Web Apps | the running code and the built web app | Azure account, GitHub OIDC | Azure | everything (accepted: the host) |

## Internal structure

- 7 modules (Identity, Expenses, Shopping, Gifts, Todos, Notes, Notifications), each with `Api`, `Application`,
  `Contracts`, `Domain`, `Infrastructure` (`Every_Module_HasAllFiveLayers`).
- 58 backend architecture tests: module boundaries, layers, schema per module, at most 6 files per folder, one use
  case per folder, idempotency keys and versions, rate limits, authorization, OpenAPI, no committed secrets. The web
  checks feature isolation, import layers and file sizes (`npm run architecture:structure`).
- From the module map to the "create a note" handler: module map → `notes.md` → by the rule "one use case = one
  folder" `Notes.Application/Features/Notes/Commands/CreateNote/CreateNoteHandler.cs`.

## Targets

| Target | Mark | Evidence |
| --- | --- | --- |
| SEC1 every request without a session gets 401, except the list | ✅ | fallback policy in Identity; `AuthorizationRulesTests.Only_TheListedEndpoints_AreAnonymous` lists 7 files with a reason each |
| SEC2 a session ends after 30 days without use, sign-out revokes it | ⚠️ | `Session.Lifetime` 30 days, sliding hourly, `Revoke` on sign-out; met as written, but see F5 |
| SEC3 0 outside keys can read family data | ❌ | F2, F3, F6 |
| OFF1 shopping and to-do screens work offline | ✅ | `web/src/platform/offline/cache`, `queue`, used by shopping and todos; manual airplane-mode check pending |
| OFF2 an offline expense is sent within 1 minute, exactly once | ❌ | F4 |
| COST1 0 ₴ on top of the database | ✅ | App Service F1 and Static Web Apps Free ([ADR 0002](../decisions/0002-azure-free-hosting.md)) |
| PERF1 API P95 ≤ 0.4 s | ❓ | no telemetry (F1) |
| PERF2 first request after sleep ≤ 3 s | ❓ | no telemetry (F1) |
| MOD1 a new section in one evening | ✅ (recipe), time not measured | [api/README.md](../../api/README.md#add-a-module) |
| MOD2 CI red on a broken rule | ✅ | architecture tests above |
| MOD3 any file at most 2 steps from the module map | ✅ | path above; the map itself is stale (F8) |

## Findings

Severity: Critical (data or money already at risk), High (will hurt), Medium, Low.

| Id | Finding | Evidence | Target | Severity |
| --- | --- | --- | --- | --- |
| F1 | No telemetry and no alerts: speed targets cannot be measured, an outage is noticed by the users | no Application Insights or OpenTelemetry in `api/src`; no alert rules named in `docs/deploy.md` | PERF1, PERF2 | High |
| F2 | The scheduler key, held by cron-job.org, also reads every section (the post-deploy smoke uses it) | `SchedulerKeyAuthenticationHandler.cs`, [ADR 0005](../decisions/0005-post-deploy-smoke-reads-every-section.md) | SEC3 | Medium |
| F3 | A database connection string with full rights lives in GitHub for migrations; whether the `production` environment requires approval is not visible in the code | `.github/workflows/api.yml` (`DATABASE_CONNECTION_STRING`) | SEC3 | Medium |
| F4 | Expenses have no offline queue: an expense recorded without internet is not saved | `web/src/features/expenses` does not use `platform/offline/queue`; [web ADR 0022](../../web/docs/decisions/0022-offline-cache-and-queued-writes.md) queues only shopping and to-dos | OFF2 | Medium |
| F5 | A session has no upper lifetime: a stolen token in regular use never expires; there is no "sign out everywhere"; the token is in `localStorage` without a Content Security Policy | `Session.cs`, `SignOutHandler.cs`, web `home.session` | SEC2 | Medium |
| F6 | The Telegram webhook secret is derived from the bot token: one leak gives both phishing as the bot and forged button presses | `TelegramWebhookSecret.For(Token)` | SEC3 | Low |
| F7 | The DDD checklist rules (typed ids, value objects as `readonly record struct`, marked aggregate roots) are not adopted: not in `AGENTS.md`, not tested; ids are plain `Guid` | `api/docs/rules.md`, `tests/ArchitectureTests` | MOD2 | Medium, decision pending |
| F8 | The module map and AGENTS.md are stale: Identity shown as "planned", the bot described as run by a GitHub Actions cron | `api/docs/architecture/README.md`, `AGENTS.md` | MOD3 | Low, fixed with this review |

## Decisions

Priority = (impact + likelihood) × (6 − cost), each scored 1–5; dependencies first. Vitaliy chose the order on
2026-11-27. Each item is its own pull request.

| Order | Finding | Impact | Likelihood | Cost | Priority | Decision | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | F1 telemetry and alerts: Application Insights with a daily cap (free up to 5 GB a month), a free uptime job on `/health` every 15 minutes ([ADR 0044](../../api/docs/decisions/0044-telemetry-in-application-insights.md)) | 3 | 5 | 2 | 32 | now | fixed in #51; smart detection e-mails skipped by Vitaliy, the uptime job covers outages |
| 2 | F2 two keys: the scheduler key runs the ticks only; a separate read key for the post-deploy smoke stays in GitHub ([ADR 0045](../../api/docs/decisions/0045-separate-deploy-check-key.md)) | 4 | 2 | 1 | 30 | now | fixed in #52 |
| 3 | F3 required approval for the `production` environment and a database role for migrations only | 5 | 1 | 1 | 30 | risk accepted 2026-11-28 by Vitaliy: one approval click per deploy and a second database role are not worth it for a two-person site; only he can merge to `main` | accepted, revisit when a third person gets write access to the repository or the hosting, or when the database holds data that cannot be re-entered |
| 4 | F4 offline queue for expenses, reusing `platform/offline/queue` ([web SDD expenses/007](../../web/docs/specs/expenses/007-offline-expense-add/README.md)) | 3 | 4 | 2 | 28 | now | fixed in #54 |
| 5 | F5 a session ends 90 days after sign-in at the latest; "sign out everywhere"; CSP later | 4 | 2 | 2 | 24 | now (CSP later) | open |
| — | F6 a separate random webhook secret | 3 | 1 | 1 | 20 | later | open |
| — | F7 decide whether the DDD rules apply here; if yes, write them down and test them | — | — | — | — | later, Vitaliy decides | open |
| — | F8 refresh the module map and AGENTS.md | 1 | 5 | 1 | — | now | fixed in #48 |

## Strengths

- Closed by default, with every anonymous endpoint listed with its reason and tested.
- Almost every structural rule is enforced in CI, in both halves.
- One predictable place for every use case, so code is found without searching.
- Idempotency keys on creates and versions on edits are enforced by a test.
