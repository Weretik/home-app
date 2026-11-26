# AI navigation: home

A short entry point for working in this repository. The code and its documentation define the actual architecture,
rules and commands; read only the sources the current task needs. This file applies to the whole repository.

## Product

A private site for two people (Masha and Vitaliy) that replaces Google Sheets and Telegram bots: expenses, shopping
list, to-dos (Masha / Vitaliy / Shared), gift wishlist. Today: the platform (health and status) and the Expenses module (expenses, incomes and the «Копилка» savings jar,
imported from the old Google Sheet and now added, edited and deleted on the site, plus spending limits with the sheet's
carry-over of overspend, expenses marked outside the limits, planned expenses for a month forecast, the savings goal
and a computed month summary that replace the sheet's «Бюджет» and «Сводка» tabs, see
[api/docs/modules/expenses.md](api/docs/modules/expenses.md)); no authentication yet, writes are open and rate
limited until Google sign-in ([ADR 0015](api/docs/decisions/0015-expenses-open-until-identity.md)). Not indexed, no SEO. UI text is Russian; everything else (docs, comments, commits)
is English.

## Project context

The repository follows [fullstack-modular-template](https://github.com/Weretik/fullstack-modular-template) in full; do
not simplify it because the project is small.

- API: [README](api/README.md) (commands, adding a use case or a module), [architecture](api/docs/architecture/README.md),
  [C4](api/docs/architecture/c4.md), [modules](api/docs/modules/expenses.md), [specs](api/docs/sdd/specs/README.md), [rules](api/docs/rules.md), [decisions](api/docs/decisions/README.md),
  [contract](api/docs/contracts/openapi.json).
- Web: [README](web/README.md), [architecture](web/docs/architecture.md), [rules](web/docs/rules.md),
  [design rules](web/docs/design.md) (every screen follows them), [specs](web/docs/specs/README.md), [decisions](web/docs/decisions/README.md).
- Repository-wide: [decisions](docs/decisions/README.md) (structure, hosting, database), [deployment](docs/deploy.md).

A new lasting decision (a module, an external system, a way of running work, hosting, authentication, a state or
transport approach) gets an ADR in the same change, in the half it belongs to, plus a C4 update for the backend.

## Context search order

1. The half's architecture and rules, then the module or feature folder.
2. Search code in `api/src`, `api/tests`, `web/src` first; documentation only in the matching section.
3. Package versions come from `api/Directory.Packages.props` and `web/package.json`, not from documentation.
4. Keep `rg` scoped to a path.

## Commands

- API build (regenerates `api/docs/contracts/openapi.json`): `cd api && dotnet build HomeApp.slnx`.
- API focused test: `dotnet test api/tests/UnitTests --filter "FullyQualifiedName~<Name>"`; architecture tests:
  `dotnet test api/tests/ArchitectureTests` (fast).
- Integration tests need Docker (`dotnet test api/tests/IntegrationTests`); do not run them unless asked, CI does.
- Web: `cd web && npm run typecheck && npm run lint && npm test && npm run architecture:structure`.
- After a backend contract change: `cd web && npm run contracts:generate`, commit the generated file with the change.

A small fix that restores documented behavior needs no SDD, file map or audit: fix it, run the focused test, describe
it briefly.

## Feature delivery order (mandatory)

Every feature, above all a new one, goes in this order ([ADR 0004](docs/decisions/0004-sdd-first-feature-delivery.md)):

1. **SDD first:** backend and frontend specifications from the templates in
   [Weretik/obsidian](https://github.com/Weretik/obsidian) (`Templates/backend-sdd`, `Templates/frontend-sdd`), stored
   in [api/docs/sdd/specs](api/docs/sdd/specs/README.md) and [web/docs/specs](web/docs/specs/README.md); ADRs for every new lasting decision in the same change.
2. **Implementation** from the SDD, with tests and docs in the same change.
3. **Architecture audit** (`backend-code-audit`, `frontend-code-audit`, obsidian `Architecture/Checklists`): modules,
   layers, folders and files, component split and size, contract, data, idempotency. Fix or record every finding.
4. **Merge** only after the audit and green CI, on the user's explicit approval.

## Task routing

| Task | Skill or process | Project rules |
| --- | --- | --- |
| New or changed backend feature: specification | `backend-feature-spec` | [API rules](api/docs/rules.md) |
| Implementing an accepted backend specification | `backend-feature-delivery` | [API README](api/README.md#add-a-use-case) |
| New or changed web feature: specification / implementation | `frontend-feature-spec` / `frontend-feature-delivery` | [Web rules](web/docs/rules.md) |
| One feature across API and web | `full-stack-feature-orchestrator` (only on explicit request for several agents) | both halves' rules |
| HTTP contract change | `backend-api-contract-sync`, then `frontend-api-contract-sync` | [ADR 0008](api/docs/decisions/0008-openapi-generated-from-code.md), [web ADR 0003](web/docs/decisions/0003-typed-client-from-openapi.md) |
| Schema, migrations, seed data | `backend-database-change` | [API rules, Data](api/docs/rules.md#data), [deployment](docs/deploy.md#database) |
| Code audit / remediation plan | `backend-code-audit`, `frontend-code-audit` / `*-remediation-plan` | architecture and rules |
| Verification before review / PR text | `backend-change-verification`, `frontend-change-verification` / `*-pr-handoff` | rules, Tests sections |
| Docs out of sync with code | `backend-docs-sync`, `frontend-docs-sync` | this file and the halves' docs |

## Boundaries

**API contract:** `api/docs/contracts/openapi.json` is generated from the code on every build and is the only
machine contract; never write it by hand. Every endpoint has `WithName` (unique camelCase), `WithSummary`,
`Produces<T>` for each body and `ProducesProblem`; the architecture tests and CI fail when the committed file differs
from the code. Merge order for a contract change: API, then regenerate the web client, then web.

**Folders:** at most 6 code files per folder; the seventh splits it by meaning into named subfolders of at least two
files. No empty folders, wrapper folders or one-file folders; only generated code (`Migrations/`, `generated/`) is
exempt. Tested in both halves. In `web/`, a component (`.tsx`) has at most 150 lines and any other module (`.ts`) at
most 200 (tests and generated code exempt); a longer file is split by responsibility (checked by
`npm run architecture:structure`).

**Modules:** a module uses another one only through its `Contracts`; `Domain` knows no EF Core or ASP.NET Core; one
schema per module, no foreign keys across schemas. **Web features** never import each other and reach the API only
through `src/platform/api.ts` from their `*-api.ts`.

**Repeats and side effects:** creates that must not run twice take an `Idempotency-Key`; side effects outside the
transaction go through the outbox, and their handlers are safe to run twice.

**Configuration and secrets:** `ConnectionStrings__Database`, `Cors__Origins__0`, `OutboxTick__Key` (and
`Expenses__Import__Key` only during an import) come from App Service environment variables; production migrations use
the `DATABASE_CONNECTION_STRING` secret of the GitHub `production` environment; locally from `dotnet user-secrets`; in CI from GitHub Actions secrets. Nothing secret in
`web/` (everything `VITE_*` ends up in the build). Never commit secrets.

Commits, pushes, branches, PRs and other remote actions only on the user's direct request. Keep unrelated working tree
changes. `main` deploys automatically, so only changes that pass the checks go to `main`.
