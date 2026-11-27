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
| 0009 | [A write refreshes every query of its backend module](0009-writes-refresh-their-backend-module.md)                                            |
| 0010 | [Every web rule has a check, or a written reason why it is review only](0010-every-rule-has-a-check.md)                                       |
| 0011 | [Todos: a grouped list with quick add, an optimistic done tick, steps saved at once](0011-todos-grouped-list-quick-add-optimistic-done.md)    |
| 0012 | [A live shared list: optimistic ticks, polling and «кто я» per device](0012-live-shared-list-with-optimistic-ticks-and-polling.md)            |
| 0013 | [A deep link opens one record: `?gift=<id>`](0013-deep-link-opens-a-record.md)                                                                |
| 0014 | [The interface is Ukrainian, strings in place, one locale constant](0014-ukrainian-interface.md)                                              |
| 0015 | [A sign-in gate with the Google button and a bearer token in localStorage](0015-sign-in-gate-with-google-button-and-bearer-token.md)          |
| 0016 | [Pages start on the signed-in person](0016-pages-start-on-the-signed-in-person.md)                                                            |
| 0017 | [«Про нас»: a section of its own with person tabs, «about me» first](0017-notes-section-person-tabs-me-first.md)                              |
| 0018 | [A limit change says from which month it applies](0018-limit-change-applies-from-a-month.md)                                                  |
| 0019 | [The phone's bottom bar ends with «Ще»: a sheet with every other page](0019-phone-more-sheet-in-the-bottom-bar.md)                            |
| 0020 | [Phone: 44px tap targets in the theme, rows that never squeeze, room for the keyboard](0020-phone-touch-targets-and-keyboard.md)              |

## Decisions that must have a record

- A new app, or a new framework for an app.
- Hosting or rendering strategy of an app.
- A state management or data transport approach.
- A rule that every change must follow.

New record: copy the newest file, take the next number, and keep the sections Context, Decision, Consequences. A record
that is replaced stays, with its status changed to `superseded by NNNN`.
