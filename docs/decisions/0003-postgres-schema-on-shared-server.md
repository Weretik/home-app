# 0003. PostgreSQL: own schema `home` on an existing shared server

**Status:** accepted, 2026-11-26

## Context

Vitaliy already has a PostgreSQL database at a hosting provider (adm.tools) for another personal project. It has no
free-tier limits, so it is more convenient than a new free database (Neon, Supabase, Azure).

## Decision

- Use that server, with everything of this project in a separate schema, `home`.
- EF Core with one context, `HomeDbContext`: default schema `home`, migrations history in `home.__ef_migrations_history`.
- A dedicated database user that owns only the `home` schema, where the hosting allows it.
- The connection string comes only from `ConnectionStrings:Default`; the SQL and network steps are in
  [deploy.md](../deploy.md#database).

## Consequences

No new service and no limits. The other project and this one share the server's resources and backups, so a heavy
query in one affects the other, and restoring a backup must be done per schema. Remote access is already open (the other project
connects from Cloud Run); SSL settings follow the other project. Moving to another PostgreSQL later means dumping one schema
(`pg_dump --schema=home`) and changing the connection string.
