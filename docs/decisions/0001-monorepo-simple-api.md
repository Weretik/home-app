# 0001. One repository, the API as a single project

**Status:** accepted, 2026-11-26

## Context

The project is based on [fullstack-modular-template](https://github.com/Weretik/fullstack-modular-template): a modular
monolith with five projects per module, a mediator, an outbox, Nx and a client generated from OpenAPI. For a family
site with two users that is too much: every small change would touch many files.

## Decision

- One repository: `api/` and `web/` side by side, each half with its own path-filtered workflow.
- The API is a single project, `HomeApp.Api`, with features in `Features/<Area>/` folders, Minimal APIs, no mediator
  and no Result types.
- The front is plain Vite without Nx; the API is called through one module, `src/platform/api-client.ts`.
- Kept from the template: central NuGet package versions, `TreatWarningsAsErrors`, CORS from configuration, `/health`,
  docker-compose with PostgreSQL, Dependabot, and the "at most 6 files per folder" rule.

## Consequences

The code is easier to read and change. If one area grows (for example, expenses with AI receipt parsing), it can move
into a separate project following the template. A typed client generated from OpenAPI can be added later, once there
are more than five or six endpoints.
