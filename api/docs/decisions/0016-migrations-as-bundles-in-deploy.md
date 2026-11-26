# 0016. Production migrations run as EF bundles in the deploy job

Status: accepted, 2026-11-26

## Context

The API never migrates the database on start in production ([0005](0005-schema-per-module.md)), and the free App
Service has no deployment slots. With the first module, something must apply migrations before a new version starts.

## Decision

- The build job makes one self-contained EF Core migrations bundle per module (`dotnet ef migrations bundle`) and
  uploads them as the `migrations` artifact.
- The deploy job runs every bundle against the GitHub secret `DATABASE_CONNECTION_STRING` (environment
  `production`) before it deploys the API. A missing secret or a failed migration stops the deploy, so the running
  version keeps working.
- A migration must keep the previous release working (add first, remove later).
- A new module adds its bundle line to the workflow.

## Consequences

- The GitHub runner needs network access to the database server, which the hosting provider allows.
- The connection string exists in two places, the App Service and the GitHub environment; a password change updates
  both.
