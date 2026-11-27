# Final frontend architecture audit — identity 002

- **Date:** 2026-11-27 · **Scope:** `src/platform/member-api.ts`, `src/platform/query-client.ts` (keys merged in),
  `src/features/identity/{data,member,profile}`, `src/features/shopping/section/use-me.ts`,
  `src/features/todos/url-state`, `src/features/gifts/url-state`, `src/app/root/App.tsx`, the generated client.
- **Checklists:** obsidian `Architecture/Checklists` (frontend), [web rules](../../../rules.md), [design rules](../../../design.md).

| Area                   | Evidence                                                                                                                                                                                                         |
| ---------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Imports and boundaries | Features reach the member only through `platform/member-api.ts`; no feature imports another (`featuresAreIsolated` green). The client instance is used only from `*-api.ts` (`member-api.ts`, `profile-api.ts`). |
| Folders and files      | `platform/` back to 6 production files after merging `query-keys.ts` into `query-client.ts`; `identity/profile/` 3 files; components ≤150 lines (`ProfilePage` 87). `architecture:structure` green (16 rules).   |
| State                  | Server state in React Query (`['identity','me']`, `['notifications','status']` shared with the «Телеграм» page); the URL keeps the filters; only the per-tab «Я» pick in `sessionStorage`.                       |
| Contract               | `FamilyMember.person` from the regenerated client; no hand-written types.                                                                                                                                        |
| UI and text            | Ukrainian texts; skeletons while loading; MUI components per the design rules; profile reached from the avatar menu.                                                                                             |
| Tests                  | `npm test` 98 files, 451 tests green; typecheck, lint, Prettier, build green.                                                                                                                                    |

## Findings

| ID    | Severity | Status   | Finding                                                                               | Decision                                                                                      |
| ----- | -------- | -------- | ------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------- |
| AF-01 | low      | accepted | The identity feature reads the notifications endpoint for the profile's Telegram row. | Web ADR 0016: a read under the notifications key, so the «Телеграм» page's writes refresh it. |
| AF-02 | low      | accepted | The old `localStorage` key `home.shopping.me` stays on devices that used it.          | One short word per device; not worth a migration.                                             |
| AF-03 | info     | fixed    | `platform/` would hold seven files with `member-api.ts`.                              | `query-keys.ts` merged into `query-client.ts`; docs and ADR 0009 updated.                     |

No open findings.
