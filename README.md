# Home

A family website that replaces Google Sheets and Apps Script Telegram bots: expenses, shopping list, to-dos (Masha,
Vitaliy, shared) and a gift wishlist. For now the repository holds only the scaffold: an API with `/health` and a page
that calls it.

**Stack:** .NET 10 · ASP.NET Core · PostgreSQL (schema `home` on a shared server) · EF Core · React 19 · Vite · TypeScript

```
api/    ASP.NET Core API, one project, features in folders (api/src/HomeApp.Api/Features/<Area>/)
web/    React + Vite, a plain static build into web/dist
docs/   Architecture, deployment, decisions (ADRs)
```

`api/` and `web/` do not depend on each other: the front only knows the API address (`VITE_API_URL`), and the API
allows the front's origin through CORS. Each half has its own workflow that runs only when its folder changes.

| | Where |
| --- | --- |
| How it works | [docs/architecture.md](docs/architecture.md) |
| Deployment and secrets | [docs/deploy.md](docs/deploy.md) |
| Decisions | [docs/decisions/README.md](docs/decisions/README.md) |
| Rules for AI agents | [AGENTS.md](AGENTS.md) |

## Run locally

You need the .NET 10 SDK, Node.js 22 and Docker (for the database).

```bash
# Database (optional: without it /health reports database: Unhealthy)
cd api
docker compose up -d

# API on http://localhost:5000
dotnet run --project src/HomeApp.Api

# Front on http://localhost:5173 (in a second terminal)
cd web
cp .env.example .env.local
npm ci
npm run dev
```

Local secrets live in `dotnet user-secrets` (`cd api/src/HomeApp.Api && dotnet user-secrets set "Key" "value"`) and
never reach the repository.

## Checks

```bash
cd api && dotnet test HomeApp.slnx
cd web && npm run typecheck && npm test && npm run format:check
```
