# 002 — Pages start on the signed-in person; profile page

- **App type:** React Web (Vite, MUI, React Query, React Router)
- **App:** `web/`, features `identity` (profile), `shopping` («Я»), `todos` («Кому»), `gifts` («Для кого»); the member
  in `src/platform/member-api.ts`
- **Architecture source:** [architecture](../../../architecture.md), [rules](../../../rules.md), [design rules](../../../design.md)
- **Status:** accepted for delivery · **Owner:** Vitaliy · **Created:** 2026-11-27 · **Updated:** 2026-11-27
- **Backend SDD:** [api/docs/specs/identity/002-who-is-signed-in](../../../../../api/docs/specs/identity/002-who-is-signed-in/README.md)
  (`FamilyMember.person`)
- **Decision:** [web ADR 0016](../../../decisions/0016-pages-start-on-the-signed-in-person.md)

Vitaliy (2026-11-27, with screenshots of the three controls): «на каждой странице автоматом выбирай пользователя
который вошел»; switching by hand stays. And: a profile page where the signed-in member and their details are seen.
Notes about each other are a separate module (its own SDD) and only get a link on this page later. Written from the
frontend SDD template (Weretik/obsidian `Templates/frontend-sdd`), React variant, condensed.

## State

- Specification: accepted · Implementation: done (2026-11-27) · Verification: [audit.md](audit.md) · Blockers: none.

## Requirements and behaviour

| ID     | Behaviour                                                                                                                                                                                                                                                                                                   |
| ------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| FR-001 | Покупки «Я: Віталій / Маша» starts on the signed-in person. A tap switches it for this tab (sessionStorage `home.shopping.me:<email>`); a new tab or visit starts on the signed-in person again. While the member loads, or for a member who is neither, «Я» is Віталій. Quick add and «Повернути» use «Я». |
| FR-002 | Справи «Кому» (active list and archive): without `?who=` the list shows the signed-in person (with the shared tasks, as before). «Усі» writes `?who=all`; «Скинути фільтр» clears the other filters and writes `who=all`. A link with `who` keeps working.                                                  |
| FR-003 | Подарунки «Для кого» (wishlist and «Подаровані»): without `?recipient=` the list shows the signed-in person. «Усі» and «Скинути фільтри» write `recipient=all`. `?gift=<id>` links open the gift whoever it is for (web ADR 0013).                                                                          |
| FR-004 | `/profile` «Профіль», from «Профіль» in the avatar menu: the Google picture (72 px) or first letter, name, e-mail; «У домі»: Віталій / Маша, or «Гість» for an allowlisted e-mail that is neither; the Telegram row; «Вийти». Skeletons while the member loads.                                             |
| FR-005 | Telegram row: «Підключено: <chat title>» with «Налаштування», or «Не підключено» with «Підключити»; both go to `/notifications`. For a guest: «Бот пише лише Віталію та Маші», no request. A failed read says «Не вдалося дізнатися».                                                                       |

## Design

- `src/platform/member-api.ts`: `useCurrentMember` (moved from the identity feature, key `['identity','me']`,
  `staleTime: Infinity`), `useSignedInPerson`, types `FamilyMember`, `FamilyPerson`. Features never import each
  other, so the shared member sits in `platform/`; `query-keys.ts` merged into `query-client.ts` to stay within six
  files.
- Pure, tested resolution: todos `resolveWho(raw, person)` and `everyoneParam` in `url-state/filters.ts`; gifts
  `parseFilters(params, categoryIds, person)` and `everyoneParam` in `url-state/gift-filters.ts`.
- `features/identity/profile/`: `ProfilePage.tsx`, `TelegramRow.tsx`, `profile-api.ts` (`useTelegramChat(person)` reads
  `getNotificationsStatus` under `['notifications','status']` and selects the person's chat). Route `/profile` in
  `App.tsx`; not in the side menu (reached from the avatar).
- No contract change beyond the backend's `person`; the generated client is regenerated.

## Tests

| Scenario       | Test                                                                                                                                 |
| -------------- | ------------------------------------------------------------------------------------------------------------------------------------ |
| FR-001         | `shopping/section/use-me.test.tsx` (signed-in default, per-tab pick, fallback, blocked storage); `ListPage.test.tsx` unchanged       |
| FR-002         | `todos/url-state/filters.test.ts` `resolveWho`                                                                                       |
| FR-003         | `gifts/url-state/gift-filters.test.ts` (default, link wins, `all`); `GiftsPage.test.tsx` «Скинути фільтри» writes `recipient=all`    |
| FR-004, FR-005 | `identity/profile/ProfilePage.test.tsx` (linked, not linked, guest without a request, «Вийти»); `MemberMenu.test.tsx` «Профіль» link |

## Change history

- 2026-11-27: first version.
