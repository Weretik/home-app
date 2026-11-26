# 0003. A typed API client generated from the backend contract

Status: accepted, 2026-11-26

## Context

Hand-written request types drift from the API and break at runtime instead of at build time.

## Decision

Generate TypeScript types from the backend's `api/docs/contracts/openapi.json` with `openapi-typescript` into
`src/api-client/generated/`, and call the API only through the `openapi-fetch` client created in `src/platform/api.ts`.
The generated file is committed; CI regenerates it and fails when it differs. Screens never use raw `fetch`.

## Consequences

- A backend contract change breaks the web build where it matters.
- Paths, parameters and bodies are checked by the compiler.
- Regenerate the client after every backend contract change (`npm run contracts:generate`).
