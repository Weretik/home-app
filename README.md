# Home

A family website that replaces Google Sheets and Apps Script Telegram bots: expenses, shopping list, to-dos (Masha,
Vitaliy, shared) and a gift wishlist. Built on
[fullstack-modular-template](https://github.com/Weretik/fullstack-modular-template). For now it holds the platform
only: the API reports its health and the web app shows it; the feature modules come next.

**Stack:** .NET 10 · ASP.NET Core · EF Core · PostgreSQL · Mediator · React 19 · Vite · TypeScript · Material UI ·
React Query

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
