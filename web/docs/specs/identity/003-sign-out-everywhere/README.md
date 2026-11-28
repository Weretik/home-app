# 003 — «Вийти на всіх пристроях» (web)

- **Application type:** React Web (Vite SPA, MUI, React Query)
- **Application:** `web/`
- **Domain:** identity, page «Профіль»
- **Status:** implemented, audited
- **Owner:** Vitaliy
- **Created:** 2026-11-28
- **Backend counterpart:** [api identity 003](../../../../../api/docs/specs/identity/003-session-limit-and-sign-out-everywhere/README.md)
- **Decision:** [api ADR 0046](../../../../../api/docs/decisions/0046-session-max-age-and-sign-out-everywhere.md)

## Requirements and scenarios

| Id       | Requirement / scenario                                                                                                                                        | Test                                                                        |
| -------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------- |
| R-SOE-01 | The profile shows «Вийти на всіх пристроях» beside «Вийти». It asks first («Вийти на всіх пристроях?», «Скасувати» / «Вийти всюди») and sends nothing before. | `ProfilePage.test.tsx` «… asks first, then ends every session and this one» |
| R-SOE-02 | After the server confirms, this device forgets its token and clears the cache, like «Вийти».                                                                  | the same test                                                               |
| R-SOE-03 | When the server does not confirm, the dialog says «Не вдалося вийти на всіх пристроях: …» and this device stays signed in. A `401` signs this device out.     | «… keeps this device signed in when the server did not confirm»             |

## File map (as built)

Paths are relative to `web/src/features/identity/`.

| Path                              | State     | One responsibility                                      |
| --------------------------------- | --------- | ------------------------------------------------------- |
| `data/identity-api.ts`            | existing  | `useSignOutEverywhere`                                  |
| `profile/SignOutButtons.tsx`      | new       | «Вийти», «Вийти на всіх пристроях» and its confirmation |
| `profile/ProfilePage.tsx`         | existing  | uses `SignOutButtons` instead of its own «Вийти» button |
| `profile/ProfilePage.test.tsx`    | existing  | R-SOE-01 … 03                                           |
| `src/api-client/generated/api.ts` | generated | `signOutEverywhere` (`npm run contracts:generate`)      |

## Audit

✅ Typecheck, lint, the structure check and the whole web suite (612 tests) pass. `profile/` holds 4 code files.
`SignOutButtons.tsx` is 80 lines, `ProfilePage.tsx` 107. The texts are Ukrainian. The API is reached only through
`identity-api.ts`.
