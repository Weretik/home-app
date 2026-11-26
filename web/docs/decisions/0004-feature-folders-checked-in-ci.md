# 0004. Feature folders with boundaries checked in CI

Status: accepted, 2026-11-26

## Context

Without Nx there are no module boundary lint rules, but the app should keep the same shape as the template: one
folder per feature, wiring separate from screens, the API reached through one client.

## Decision

`tools/check-structure.mjs` runs in CI and fails when a folder breaks the folder rules, when a feature imports another
feature, or when anything other than `src/platform/` and a feature's `*-api.ts` imports the API client. ESLint forbids
the global `fetch` outside `src/api-client/`.

## Consequences

- The rules in [rules.md](../rules.md) are enforced, not only written.
- Something two features need moves to `platform/` (wiring) or a named shared UI folder, not to a dump folder.
