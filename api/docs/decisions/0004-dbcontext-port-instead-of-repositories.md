# 0004. A DbContext port per module instead of repositories

Status: accepted, 2026-11-16

## Context

Generic repositories over EF Core repeat what `DbContext` already offers, hide query shape and often call
`SaveChanges` on their own, which breaks the "one save per command" rule.

## Decision

Each module defines a port in `Application` (for example `ICatalogDbContext`) that exposes the module's
`DbSet`s and `SaveChangesAsync`. `Infrastructure` implements it with the module's `DbContext`. Handlers write
LINQ directly, with projections and `AsNoTracking` for reads, and save once.

## Consequences

- Queries are visible where they are used and easy to tune.
- Unit tests use the EF Core in-memory provider behind the same port; integration tests use real PostgreSQL.
- `Application` depends on EF Core abstractions (`Microsoft.EntityFrameworkCore`) but not on the Npgsql driver;
  the architecture tests keep it that way.
