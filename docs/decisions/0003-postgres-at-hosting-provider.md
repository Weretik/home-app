# 0003. PostgreSQL: a separate database at the existing hosting provider

**Status:** accepted, 2026-11-26

## Context

Vitaliy already pays for hosting (adm.tools) whose plan includes PostgreSQL with several databases and no free-tier
limits; another personal project already uses it from Google Cloud Run. That is more convenient than a new free
database (Neon, Supabase, Azure).

## Decision

- A separate database for this project on that server, owned by its own user created by the panel.
- EF Core with one context, `HomeDbContext`: default schema `home`, migrations history in `home.__ef_migrations_history`.
  The schema is kept even in a separate database, so the data can later move to a shared database unchanged.
- The connection string comes only from `ConnectionStrings:Default`; the steps are in [deploy.md](../deploy.md#database).

## Consequences

No new service and no limits. The server's resources are shared with the other project, so a heavy query in one
affects the other. Remote access is already open; SSL settings follow the other project. Moving to another PostgreSQL
later means `pg_dump --schema=home` and a new connection string.
