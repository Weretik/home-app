# 0002. A React single-page app with Material UI and React Query, never indexed

Status: accepted, 2026-11-26

## Context

The app is used by two signed-in people, mostly on phones. It needs lists and forms, not server rendering or SEO.

## Decision

Build it as a React single-page app with Vite, Material UI, React Query for server state and React Router for pages,
as the template's admin app. It sends `noindex` in its HTML and disallows everything in `robots.txt`. It is hosted as
static files (Azure Static Web Apps today) and calls the API on its own origin.

## Consequences

- Simple static hosting; no server runtime for the front.
- Server data is cached, refetched and invalidated by React Query instead of hand-written state.
- The API origin is baked in at build time (`VITE_API_URL`), and the API allows the app's origin through CORS.
