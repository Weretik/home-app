# Backend

A .NET 10 modular monolith on PostgreSQL, built on
[fullstack-modular-template](https://github.com/Weretik/fullstack-modular-template). One deployable API (`Host.Api`)
hosts the modules; each module owns its own schema, its own use cases and its own public surface. Modules so far:
[Expenses](docs/modules/expenses.md).

- [Architecture](docs/architecture/README.md): modules, layers and what may reference what.
- [C4 diagrams](docs/architecture/c4.md): the system, its containers and the inside of the API.
- [Rules](docs/rules.md): the conventions every change follows; most are checked by tests.
- [Decisions](docs/decisions/README.md): why it is built this way.
- [Modules](docs/modules/expenses.md): one page per module (model, layers, schema, endpoints).
- [API contract](docs/contracts/openapi.json): generated from the code on every build.

## Layout

```
src/
  BuildingBlocks/   Result and Error, validation pipeline, outbox, persistence helpers, problem responses
  Modules/          one folder per module, five projects each (Expenses)
    <Name>/         <Name>.Api, .Application, .Contracts, .Domain, .Infrastructure
  Host/Host.Api/    Composition only: registers modules, middleware, OpenAPI
tests/
  UnitTests/          domain rules and handlers, no database
  ArchitectureTests/  boundaries, layer dependencies, folder and naming rules, OpenAPI rules
  IntegrationTests/   the real API against PostgreSQL in Testcontainers
docs/
```

Package versions live in `Directory.Packages.props`, shared build settings in `Directory.Build.props` (nullable
reference types, warnings as errors, XML docs).

## Commands

| What | Command |
| --- | --- |
| Start PostgreSQL | `docker compose up -d` |
| Run the API on http://localhost:5000 | `dotnet run --project src/Host/Host.Api` |
| Build (regenerates `docs/contracts/openapi.json`) | `dotnet build HomeApp.slnx` |
| Unit tests | `dotnet test tests/UnitTests` |
| Architecture tests | `dotnet test tests/ArchitectureTests` |
| Integration tests (need Docker) | `dotnet test tests/IntegrationTests` |
| One test class | `dotnet test tests/UnitTests --filter FullyQualifiedName~ValidationBehaviorTests` |
| Restore local tools (`dotnet-ef`) | `dotnet tool restore` |

In Development the API applies every module's migrations on start (`Database:InitializeOnStartup`). In other
environments migrations run as a separate step before the deploy (see [Deploy](#deploy)).

## Configuration

| Key | Meaning |
| --- | --- |
| `ConnectionStrings:Database` | PostgreSQL connection string; all modules share the database, each in its own schema. Empty: the API runs without a database and `/health` has no `database` check. |
| `Database:InitializeOnStartup` | Apply migrations and seed data on start. Development only. |
| `Cors:Origins` | Browser origins allowed to call the API with credentials. |
| `OutboxTick:Key` | Shared secret for `POST /internal/outbox/tick`; empty disables the endpoint. |
| `Expenses:Import:Key` | Shared secret for the one-time sheet import (`X-Import-Key`); empty (the default) disables it. Set only while importing. |
| `RateLimiting:PublicWrite`, `:SignIn` | `PermitLimit` and `WindowSeconds` per client address for anonymous writes and sign-in ([ADR 0012](docs/decisions/0012-rate-limits-on-anonymous-writes.md)). |
| `ForwardedHeaders:Enabled`, `:KnownProxies`, `:KnownNetworks` | Read the client address and scheme from a trusted reverse proxy (see [Deploy behind a reverse proxy](#deploy-behind-a-reverse-proxy)). |

Keep real values in user secrets or environment variables (`ConnectionStrings__Database=...`), never in the
repository.

## Add a use case

A use case is one folder in the module's `Application` project and one endpoint file in its `Api` project.
For example, "mark a shopping item as bought":

```
Shopping.Application/Features/Items/Commands/MarkItemBought/
  MarkItemBoughtCommand.cs     record MarkItemBoughtCommand(Guid Id) : ICommand<Result>
  MarkItemBoughtValidator.cs   FluentValidation rules for the input shape
  MarkItemBoughtHandler.cs     loads the item, calls the domain method, saves once
Shopping.Api/Endpoints/Items/
  MarkItemBoughtEndpoint.cs    maps the route, sends the command, returns result.ToHttpResult(http)
```

1. Write the test first: a domain test for the new rule and a handler test in `tests/UnitTests`.
2. Put the rule in the domain (`item.MarkBought()` returns a `Result`); return a typed error from
   `ShoppingItemErrors` when it fails.
3. The handler loads, calls the domain, and calls `SaveChangesAsync` once.
4. The endpoint gives the operation a unique camelCase `WithName`, a `WithSummary`, and lists what it produces
   (`Produces<T>`, `ProducesValidationProblem`, `ProducesProblem`). The HTTP request record is separate from the
   command.
5. Build. The OpenAPI document changes; commit it with the code. The frontend picks it up with
   `npm run contracts:generate` in `web/`.

A folder holds at most six code files. When a seventh arrives, split the folder by meaning (for example
`Features/Items/Commands` and `Features/Items/Queries`).

## Add a module

1. Create the five projects under `src/Modules/<Name>/` with `dotnet new classlib` (and add them to
   `HomeApp.slnx` in a solution folder that mirrors the disk). `Api` references its own `Application` and
   `Infrastructure`; `Application` references its own `Domain`, its own `Contracts` and other modules' `Contracts`
   only.
2. Give the module a schema: a `<Name>DbContext : ModuleDbContext` with `Schema = "<name>"`, an
   `I<Name>DbContext` port in `Application`, and a registration method in `Infrastructure`.
3. Add a `<Name>Module` class in `Api` with `Add<Name>Module` and `Map<Name>Module`, then call both from
   `Program.cs`, and add the `Application` assembly to `Composition/MediatorSetup.cs`.
4. Create the first migration:
   `dotnet ef migrations add Initial --project src/Modules/<Name>/<Name>.Infrastructure --startup-project src/Host/Host.Api --context <Name>DbContext --output-dir Persistence/Migrations`.
5. The architecture tests find the module by its folder. Write an ADR
   if the module brings a new external system or way of running work.

## Talk to another module

Only through its `Contracts` project. If, for example, `Expenses` needs the names of shopping items, `Shopping.Contracts`
exposes an interface and `Shopping.Application` implements it; `Expenses.Application` depends on the interface only. No module reads another module's tables, and there are no foreign keys between schemas.

To react to something that happened in another module without waiting for it, publish a message through the outbox
(below) instead of calling the other module inside the request.

## Outbox and the scheduled tick

A use case adds a message to its module's outbox table in the same `SaveChanges` as its data, so either both are
stored or neither is. Delivery happens later:

1. A scheduler (Cloud Scheduler, cron, or anything that can send HTTP) calls `POST /internal/outbox/tick` every few
   minutes with the header `X-Scheduler-Key: <OutboxTick:Key>`.
2. Each module claims up to 20 due messages with one conditional `UPDATE` (so two ticks never take the same
   message), runs their handlers, and records the result.
3. A failed message is retried after 1, 4, 9 and 16 minutes, then left for a person to look at; delivered
   messages are deleted after seven days.

There is no background thread, so the API can scale to zero between requests. A handler must be safe to run twice
(check state before acting), because a crash after the work and before the record is always possible.

To add a message: put the record in the publishing module's `Contracts`, a handler implementing
`IOutboxMessageHandler<T>` in the receiving module's `Application`, and register it with
`services.AddOutboxMessage<TMessage, THandler>("<module>.<event-name>")`. The name is stored in the table, so never
change it once released.

## Idempotent requests

A create that must not happen twice (for example adding an expense from a flaky phone connection) takes an
`Idempotency-Key` header. The key is stored with the row under a unique index in the same transaction, with a
fingerprint of the request body. A repeat with the same body returns the first result; the same key with a different
body answers `409 Conflict`; two racing copies end with one row. See
[ADR 0007](docs/decisions/0007-idempotency-key-for-creates.md).

## Errors

Business rules return `Result` or `Result<T>` with an `Error(code, message, type)`. `ToHttpResult` turns the error
type into a status (`Validation` 400, `Unauthorized` 401, `Forbidden` 403, `NotFound` 404, `Conflict` 409) and a
problem response with `code` and, for validation, `errors` per field. Exceptions are for bugs and outages only;
`GlobalExceptionHandler` logs them and answers 500 without details.

## Deploy

The API runs on Azure App Service (Linux, F1). The workflow, secrets and App Service settings are in
[../docs/deploy.md](../docs/deploy.md). Behind the App Service proxy:

1. **Forwarded headers.** Set `ForwardedHeaders__Enabled=true` and `ForwardedHeaders__KnownNetworks__0=0.0.0.0/0`
   (the App Service front end has no fixed address and the app is reachable only through it), so rate limits see the
   real client address.
2. **HTTPS.** Turn on HTTPS Only; outside Development the API adds HSTS.
3. **CORS.** The web app's origin in `Cors__Origins__0`.
4. **Database.** Migrations run as a separate step before the new version starts: the workflow builds one
   `dotnet ef migrations bundle` per module and runs it with the `DATABASE_CONNECTION_STRING` secret
   ([ADR 0016](docs/decisions/0016-migrations-as-bundles-in-deploy.md)). A new module adds its bundle to
   `.github/workflows/api.yml`. Keep `Database:InitializeOnStartup` off and each migration compatible with the
   previous release.
5. **Secrets.** `ConnectionStrings__Database` and `OutboxTick__Key` come from App Service environment variables,
   never from `appsettings.json`.
6. **Health.** `GET /health`; the workflow's smoke test calls it after every deploy.

## Extracting a module into a service

Because modules already behave like separate services that share a process, extracting one is mostly moving code:
move its schema to its own database, create a new host that calls `Add<Name>Module` and `Map<Name>Module`, and replace
in-process `Contracts` implementations with HTTP clients. Do this when a module needs to scale, deploy or fail
independently, not before.
