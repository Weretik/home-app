# AGENTS.md: home website

A short entry point for AI agents. This is a small family project: keep things simple, without the ceremony of work
projects. The code and `docs/` define how things actually work; read only what the task needs.

## Product

A website for two people (Masha and Vitaliy) that replaces Google Sheets and Telegram bots: expenses, shopping list,
to-dos (Masha / Vitaliy / Shared), gift wishlist. So far there is only `/health` and a status page; there is no
authentication or database yet, and the PostgreSQL provider is not chosen. The site is private: not indexed, no SEO.

## Where things are

- [Architecture](docs/architecture.md): folders, how the front calls the API, configuration.
- [Deployment](docs/deploy.md): Azure, workflows, which secrets live where.
- [Decisions (ADRs)](docs/decisions/README.md): why things are the way they are. A new lasting decision (database,
  authentication, hosting, a new state library) gets a short ADR in the same change.

## Commands

- API: `cd api && dotnet build HomeApp.slnx && dotnet test HomeApp.slnx` (fast, no Docker).
- Focused test: `dotnet test HomeApp.slnx --filter "FullyQualifiedName~<Name>"`.
- Web: `cd web && npm run typecheck && npm test && npm run format:check`; `npm run format` fixes style.

## Rules

**Language.** All documentation, comments and commit messages are in English. User-facing UI text is in Russian.

**API (`api/`).** One project, `HomeApp.Api`. Code is split by feature: `Features/<Area>/` (`Expenses`, `Shopping`,
`Todos`, `Gifts`), with endpoints, model and data access side by side. Shared wiring (CORS, health, database) lives in
`Platform/`. Minimal APIs, endpoints grouped with `MapGroup("/api/<area>")`. Errors are returned as ProblemDetails.
There are no five-project modules, mediator, outbox or Result types here; add them only when truly needed, with an ADR.

**Web (`web/`).** Code by feature: `src/features/<area>/`, app wiring in `src/app/`, the API client and environment in
`src/platform/`. Call the API only through `src/platform/api-client.ts`, never with a raw `fetch` in components. No
secrets in the front: everything in `VITE_*` ends up in the build. The build stays a plain Vite static build with no
ties to Azure.

**Folders.** At most 6 code files per folder (tests next to them do not count); a seventh file means the folder should
be split by meaning. No dump folders such as `utils`, `helpers`, `common`, `shared`; name files after what they do.

**Configuration and secrets.** Settings come from `appsettings.json` and environment variables (`Cors__Origins__0`,
`ConnectionStrings__Default`). Secrets: locally in `dotnet user-secrets`, in Azure as App Service environment variables,
in CI as GitHub Actions secrets. Secrets are never committed.

**Tests.** Every endpoint has at least one test through `WebApplicationFactory`; front logic is tested with Vitest and
Testing Library. No full E2E suite.

**Scope of work.** A small fix that restores behavior needs no spec: fix it, run the test, describe it briefly. A new
area (for example, "expenses") starts with a short plan in the task or PR; no separate SDD.

**Git.** Commits, pushes, branches and PRs only on direct request. Leave other people's uncommitted changes alone.
`main` deploys automatically, so only changes that pass the checks go to `main`.
