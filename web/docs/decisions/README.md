# Decisions

Architecture decision records: why the web app is built the way it is. They follow the frontend of
[fullstack-modular-template](https://github.com/Weretik/fullstack-modular-template), without its Nx workspace and
Next.js storefront.

| #    | Decision                                                                                                                                      |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------------- |
| 0001 | [One Vite React app, no Nx](0001-single-vite-app-without-nx.md)                                                                               |
| 0002 | [A React single-page app with Material UI and React Query, never indexed](0002-react-spa-mui-react-query.md)                                  |
| 0003 | [A typed API client generated from the backend contract](0003-typed-client-from-openapi.md)                                                   |
| 0004 | [Feature folders with boundaries checked in CI](0004-feature-folders-checked-in-ci.md)                                                        |
| 0005 | [The «Синий плащ» theme, light/dark/system, and a Berry-style dashboard shell](0005-blue-coat-theme-and-berry-style-shell.md)                 |
| 0006 | [Section pages as tabs, with the month and filters in the URL](0006-section-pages-as-tabs-with-state-in-the-url.md)                           |
| 0007 | [Editing in dialogs, with an Idempotency-Key, If-Match versions and signed colours](0007-editing-in-dialogs-with-idempotency-and-versions.md) |
| 0008 | [The home page as a registry of per-section summaries](0008-home-page-as-a-registry-of-section-summaries.md)                                  |
| 0012 | [A live shared list: optimistic ticks, polling and «кто я» per device](0012-live-shared-list-with-optimistic-ticks-and-polling.md)            |

## Decisions that must have a record

- A new app, or a new framework for an app.
- Hosting or rendering strategy of an app.
- A state management or data transport approach.
- A rule that every change must follow.

New record: copy the newest file, take the next number, and keep the sections Context, Decision, Consequences. A record
that is replaced stays, with its status changed to `superseded by NNNN`.
