# 0001. One Vite React app, no Nx

Status: accepted, 2026-11-26

## Context

The template's frontend is an Nx workspace with two apps (a Next.js storefront and a React admin) and a shared API
client library. This project has one app for two signed-in people and no public pages.

## Decision

One Vite React app in `web/`, with the template's structure kept inside it: `app/`, `platform/`, `features/`, and
the template's `libs/api-client` moved to `src/api-client/` unchanged in role. Plain npm scripts replace Nx targets.

## Consequences

- Fewer tools and configuration files; `npm run build` produces a static `dist/` for any host.
- No affected-project graph, which one app does not need.
- If a second app appears (for example a public page), move to the template's Nx workspace: `src/api-client/` becomes
  `libs/api-client` as is.
