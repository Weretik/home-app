# 0016. Pages start on the signed-in person

Status: accepted, 2026-11-27. Supersedes the «кто я» part of [0012](0012-live-shared-list-with-optimistic-ticks-and-polling.md).

## Context

Since sign-in ([0015](0015-sign-in-gate-with-google-button-and-bearer-token.md)) the app knows who is using it, and
the API now says whether the member is Vitaliy or Masha (`person` on `GET /api/identity/me`, API ADR 0038). Three
places still asked by hand: Shopping «Я» (kept per device in `localStorage`, ADR 0012), the to-do filter «Кому»
(`?who=`) and the gift filter «Для кого» (`?recipient=`). Vitaliy asked that each page start on whoever signed in and
that switching by hand stays. Every feature needs the member, and features never import each other.

## Decision

- The signed-in member lives in `src/platform/member-api.ts`: `useCurrentMember()` (query key `['identity', 'me']`,
  never stale during a session) and `useSignedInPerson()` (`'Vitaliy' | 'Maria' | undefined`). The identity feature
  re-exports it; other features read only `useSignedInPerson`.
- Shopping «Я»: a hand-picked person is kept in `sessionStorage` per signed-in e-mail
  (`home.shopping.me:<email>`), so it lasts for the tab; otherwise «Я» is the signed-in person, else Віталій.
- Todos «Кому» and Gifts «Для кого»: the URL still wins. Without the parameter the list shows the signed-in person;
  «Усі» and «Скинути фільтр(и)» write `who=all` / `recipient=all`, so the choice survives a reload and a shared link.
  Deep links (`?gift=<id>`, ADR 0013) resolve against the whole list and are not affected.
- A profile page (`/profile`, from the avatar menu) shows the member, who they are in the home, their Telegram chat
  (read from `getNotificationsStatus` under the notifications key, so the «Телеграм» page's changes refresh it) and
  «Вийти».

## Consequences

- Each person opens Покупки, Справи and Подарунки on their own items; quick-added to-dos and a new gift start on that
  person too, as they did with a manual filter.
- The old `localStorage` key `home.shopping.me` is no longer read; it is left as it is (one word per device).
- A link without `who`/`recipient` opens differently for Masha and for Vitaliy; a link that must show everyone carries
  `all`.
- `src/platform/query-keys.ts` merged into `query-client.ts`, so `platform/` stays within six files.
