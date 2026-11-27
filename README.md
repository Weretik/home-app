# Home

[![API](https://github.com/Weretik/home-app/actions/workflows/api.yml/badge.svg)](https://github.com/Weretik/home-app/actions/workflows/api.yml)
[![Web](https://github.com/Weretik/home-app/actions/workflows/web.yml/badge.svg)](https://github.com/Weretik/home-app/actions/workflows/web.yml)
![.NET 10](https://img.shields.io/badge/.NET_10-512BD4?logo=dotnet&logoColor=white)
![React 19](https://img.shields.io/badge/React_19-20232A?logo=react&logoColor=61DAFB)
![Azure](https://img.shields.io/badge/Azure-free_tier-0078D4?logo=microsoftazure&logoColor=white)

**A family service that replaced a set of Google Sheets and Apps Script Telegram bots with one app:** the household
budget with limits and a savings jar, a shared shopping list, to-dos, a gift wishlist, notes about each other and a
family Telegram bot. It is an installable app that works offline, signs in with Google, runs in production on Azure for
two people and costs $0 in hosting.

It is also a pet project for architecture practice and AI features: every change is measured against the
[quality targets](api/docs/architecture/quality-targets.md), reviewed before and after
([architecture reviews](docs/reviews/2026-11-27-architecture-before.md)), and an MCP server is the next AI feature.

It is also a full-size example of how I build business systems: a .NET 10 modular monolith with a React front end,
a specification before every feature, architecture rules checked by tests in CI, and a typed contract between the
two halves. Next step: an MCP server, so the home can be managed from Claude.

![Home page: the month at a glance](docs/screenshots/home.png)

<sub>Screenshots use made-up demo data.</sub>

## What it does

| Section | What replaced what |
| --- | --- |
| **Budget** | Expenses, incomes and the savings jar; monthly and quarterly spending limits with carry-over of overspend, planned expenses for the month forecast, and a computed month summary. Replaces the budget spreadsheet and its summary tabs. |
| **Shopping** | One live list for both phones: quick add from free text («milk, bread, eggs 10»), automatic store departments, a cart that empties itself, history and frequent items. |
| **To-dos** | Tasks for each person or both, with steps, statuses, priorities, categories and an archive. |
| **Gifts** | A wishlist with variants, an archive of what was given and personal details. A gift with a giver and a buy-by date becomes a to-do and closes itself when the to-do is done. |
| **About us** | Notes on what each of us likes, dislikes or should know about the other («not the milk from that shop, the one from this one»), by categories, with who wrote each note. New, no sheet before it. |
| **Telegram bot and push** | Morning to-dos, gift reminders, the evening shopping list, weekly and monthly expenses summaries, «✅» buttons to close a task from the chat. The same messages also arrive as Web Push notifications on the phone. |
| **Sign-in and offline** | Google sign-in for the two family accounts only. Installable as an app on the phone; every page keeps its last data offline, and ticks and quick adds wait for the network. |

<table>
  <tr>
    <td><img src="docs/screenshots/expenses.png" alt="Budget: expenses of the month"></td>
    <td><img src="docs/screenshots/shopping.png" alt="Shopping list grouped by department"></td>
  </tr>
  <tr>
    <td><img src="docs/screenshots/todos.png" alt="To-dos grouped by due date"></td>
    <td><img src="docs/screenshots/gifts.png" alt="Gift wishlist per person"></td>
  </tr>
  <tr>
    <td colspan="2" align="center"><img src="docs/screenshots/notes.png" alt="About us: likes, dislikes and facts" width="50%"></td>
  </tr>
</table>

## Architecture

```mermaid
flowchart LR
    subgraph Azure["Azure (free tiers)"]
        Web["Web app<br/>React 19 · Vite · MUI<br/>Static Web Apps"]
        Api["API: .NET 10 modular monolith<br/>App Service"]
    end
    subgraph Modules["Modules (one PostgreSQL schema each)"]
        Identity
        Expenses
        Shopping
        Todos
        Gifts
        Notes
        Notifications
    end
    User(("Family<br/>browser / phone")) -- "Google sign-in" --> Web
    Web -- "typed client<br/>generated from OpenAPI" --> Api
    Api --- Modules
    Modules --> DB[("PostgreSQL")]
    Cron["cron-job.org + GitHub Actions<br/>every 15 min"] -- "outbox + digest tick" --> Api
    Api <-- "webhook / sendMessage" --> TG["Telegram"]
    Api -- "Web Push (VAPID)" --> User
    Claude["Claude (planned)"] -. "MCP" .-> Api
```

- **Modular monolith.** One deployable API; each module (Identity, Expenses, Shopping, Todos, Gifts, Notes, Notifications) has five
  projects (`Api`, `Application`, `Contracts`, `Domain`, `Infrastructure`) and its own database schema. Modules talk
  only through their `Contracts`; there are no foreign keys across schemas.
- **Closed by default.** Google ID tokens are verified on the server, only the two family e-mails may sign in, and
  every endpoint requires a session unless it says otherwise
  ([ADR 0037](api/docs/decisions/0037-google-sign-in-with-server-sessions.md)). Sessions are stored as token hashes and
  can be revoked per device.
- **Offline first on the phone.** An installable PWA: the query cache lives in IndexedDB, so every page opens with
  its last data without a network, and shopping ticks and quick adds queue until it returns
  ([web ADR 0022](web/docs/decisions/0022-offline-cache-and-queued-writes.md)).
- **Web Push without third parties.** RFC 8291 encryption and VAPID signing with `System.Security.Cryptography` only,
  no Firebase and no extra package ([ADR 0042](api/docs/decisions/0042-web-push-next-to-telegram.md)).
- **Reliable side effects.** Telegram and push messages and cross-module events go through a transactional outbox; creates
  take an `Idempotency-Key`, edits use optimistic concurrency, so a repeated tap or retry never doubles a record.
- **Contract first.** `api/docs/contracts/openapi.json` is generated from the code on every build; CI fails if it
  drifts, and the web app's typed client is generated from it.
- **Every rule has a test.** Module boundaries, layer dependencies, folder sizes, endpoint metadata and naming are
  checked by architecture tests on the backend and structure checks on the frontend
  ([ADR 0029](api/docs/decisions/0029-every-rule-checked-by-a-test.md)). This is what keeps AI-assisted changes
  honest: an agent follows [AGENTS.md](AGENTS.md), and CI proves the result.
- **Specification before code.** Each feature starts as a backend and a frontend SDD, and every lasting decision is
  an ADR: 43 for the API, 22 for the web app, 6 for the repository.
- **Architecture audits with fixes.** Every finished feature gets an architecture audit of modules, layers, contract,
  data and idempotency; each finding is either fixed with evidence (code, test) or recorded with an owner. Example:
  [the Expenses audit](api/docs/audits/2026-11-26-expenses-budget-forecast.md) split an oversized handler into pure,
  unit-tested calculations and added integration tests for the concurrent-edit race.

More: [C4 diagrams](api/docs/architecture/c4.md), [backend architecture](api/docs/architecture/README.md),
[web architecture](web/docs/architecture.md).

## Delivery and hosting

- **CI/CD on GitHub Actions.** A pull request runs build, unit, architecture and integration tests (PostgreSQL in
  Testcontainers) and the frontend checks; `main` deploys. The API signs in to Azure through **OIDC**, with no stored
  passwords or publish profiles. Database migrations run as bundles in the deploy, then smoke tests read every
  section.
- **$0 hosting.** API on App Service F1, web on Static Web Apps Free. App Service F1 has no always-on background jobs,
  so an external cron (cron-job.org, with GitHub Actions as a backup) is the app's clock for the outbox and the bot.
  The API records a heartbeat of every tick, and the site and the deploy warn when the clock goes silent
  ([ADR 0043](api/docs/decisions/0043-external-cron-and-scheduler-heartbeat.md)).
- **Tests:** 492 unit and 93 architecture tests on the backend plus integration tests against a real PostgreSQL;
  608 tests on the frontend.

## Stack

| Area | Technologies |
| --- | --- |
| Backend | C#, .NET 10, ASP.NET Core minimal APIs, EF Core, PostgreSQL, Mediator, FluentValidation, Google.Apis.Auth |
| Frontend | React 19, TypeScript, Vite, Material UI, TanStack Query (persisted in IndexedDB), openapi-fetch, PWA (Workbox) |
| Tests | xUnit, NetArchTest, Testcontainers, Vitest, Testing Library |
| Cloud and delivery | Azure App Service, Azure Static Web Apps, GitHub Actions, OIDC, Dependabot |
| Integrations | Google sign-in, Telegram Bot API (webhook), Web Push (VAPID), cron-job.org |

## Roadmap

- [ ] **MCP server**: manage the home from Claude (add an expense, read the month summary, update the shopping list).
- [ ] **Telegram bot webhooks**: more commands to add and change records right from the chat.
- [x] **Google sign-in** for the family, everything closed by default
  ([ADR 0037](api/docs/decisions/0037-google-sign-in-with-server-sessions.md)).

## Repository map

Built on [fullstack-modular-template](https://github.com/Weretik/fullstack-modular-template).

```
api/    .NET 10 modular monolith: BuildingBlocks, Host.Api, Modules/<Name>/ (five projects each), tests
web/    React single-page app: app/, platform/, features/, a client generated from the API's OpenAPI document
docs/   Repository-wide decisions and deployment
```

`api/` and `web/` know nothing about each other except one file: the OpenAPI document `api/docs/contracts/openapi.json`.
Each half has its own README, rules, decisions and workflow.

| | API | Web |
| --- | --- | --- |
| Start here | [api/README.md](api/README.md) | [web/README.md](web/README.md) |
| Architecture | [Modules, layers, rules](api/docs/architecture/README.md), [C4](api/docs/architecture/c4.md) | [Folders and data flow](web/docs/architecture.md) |
| Rules | [api/docs/rules.md](api/docs/rules.md) | [web/docs/rules.md](web/docs/rules.md) |
| Decisions | [api/docs/decisions](api/docs/decisions/README.md) | [web/docs/decisions](web/docs/decisions/README.md) |
| Specifications | [api/docs/specs](api/docs/specs/README.md) | [web/docs/specs](web/docs/specs/README.md) |

Repository-wide: [decisions](docs/decisions/README.md) (structure, hosting, database) and [deployment](docs/deploy.md).
Rules for AI agents: [AGENTS.md](AGENTS.md).

## Quick start

You need the .NET 10 SDK, Node.js 22 and Docker.

```bash
# 1. Database
cd api
docker compose up -d

# 2. API on http://localhost:5000 (OpenAPI at /openapi/v1.json in Development)
dotnet run --project src/Host/Host.Api

# 3. Web on http://localhost:5173 (in a second terminal)
cd web
cp .env.example .env.local
npm ci
npm run dev
```

## Author

Built by [Vitaliy Tsiupin](https://www.linkedin.com/in/vitaliythupin/), full-stack .NET + React developer. The same
approach runs in the production systems I build for businesses.

## License

[MIT](LICENSE) © 2026 Vitaliy Tsiupin. You may use, copy and change this code, also commercially, as long as the
copyright notice and the license text stay with every copy or substantial part of it.
