# Backend rules

Short, stable rules for every change. The ones marked **(tested)** are enforced by `tests/ArchitectureTests` and
fail the build when broken.

## Structure

- A module is five projects: `Api`, `Application`, `Contracts`, `Domain`, `Infrastructure`. **(tested)**
- A module uses another module only through that module's `Contracts`. **(tested)**
- `Domain` references no EF Core and no ASP.NET Core; `Application` references no `Infrastructure` or `Api`.
  **(tested)**
- `Host.Api` holds composition only and references only module `Api` projects and shared infrastructure. **(tested)**
- Domain properties have no public setters; state changes through methods that check the rules. **(tested)**
- Folders follow features and aggregates, never kinds of classes: `Features/Products/Commands/CreateProduct/`, not
  `Handlers/` or `Services/`. Names such as `Shared`, `Common`, `Helpers`, `Utils`, `Services`, `Entities` are not
  allowed as folders. **(tested)**
- **A folder holds at most six code files.** When a seventh arrives, split the folder by meaning into named
  subfolders of at least two files each. No empty folders, no folder that only wraps one subfolder, no new one-file
  folders. Only generated EF `Migrations/` are exempt. This applies to `src/` and `tests/`. **(tested)**
- One type per file; the file has the type's name.
- All projects live under `src/` or `tests/`, and solution folders mirror the disk.

## Use cases

- One use case is one folder under `Commands/` or `Queries/` with its command or query, its validator and its
  handler. Every use case returns a `Result`, and every use case with input has a validator. **(tested)**
- Commands change state and return little; queries never change state.
- The HTTP request record belongs to the endpoint and is mapped to the command; the command is not the request.
- Every input has a FluentValidation validator. Validators check shape (required, length, range); the domain checks
  business rules.
- A command saves once (`SaveChangesAsync` a single time). There are no repositories that save on their own; the
  module's `DbContext` port is the unit of work.

## Errors

- A broken business rule returns `Result` / `Result<T>` with a typed `Error` (`Validation`, `NotFound`, `Conflict`,
  `Forbidden`, `Unauthorized`) and a stable code such as `catalog.slug_taken`.
- Exceptions are for bugs and outages. One global handler logs them and answers 500 without internals.
- A unique index violation becomes a `Conflict` result in the handler, so a race never returns 500.

## API

- Minimal APIs, one endpoint per file, grouped by feature.
- Every endpoint has a unique camelCase `WithName`, a `WithSummary`, and `Produces...` for every response it can
  give. **(tested on the generated OpenAPI document)**
- Errors are RFC 9457 problem responses with `code` and, for validation, `errors` by field.
- An anonymous endpoint that writes, and every sign-in, opts into a rate limit policy
  (`RequireRateLimiting(RateLimitPolicies.PublicWrite)`) and lists `429` in its `Produces...`.
- The OpenAPI document is generated on build and committed; CI fails when it differs from the code. Never edit it by
  hand.

## Data

- One schema per module and no foreign keys across schemas.
- Reads use projections or `AsNoTracking` and page in SQL (page size at most 100).
- No query and no `SaveChanges` inside a loop.
- Search splits the input into words; each word is an escaped `LIKE` on a lower-cased,
  `pg_trgm`-indexed column.
- Shared state gets a named race guard: unique index, row version, or conditional `UPDATE`.
- Migrations are a separate step before a deploy and must keep the previous release working (add first, remove
  later).

## Repeats and side effects

- A create that moves money or stock, numbers a document, or sends something outside takes an `Idempotency-Key`
  stored in the same transaction under a unique index. A repeat returns the first result; the same key with other
  data answers 409.
- Side effects outside the transaction (notifications, calls to other systems) go through the outbox, and their
  handlers are safe to run twice.
- Work that runs on a schedule is an HTTP endpoint called by an external scheduler, not a background thread.

## Process

- Write the test first: domain tests and handler tests in `UnitTests`, a few end-to-end checks in
  `IntegrationTests`.
- Add an abstraction only when a third case needs it.
- A lasting decision (a module, an external system, a way of running work, hosting) gets an ADR and a C4 update in
  the same change.
- Package versions only in `Directory.Packages.props`.
