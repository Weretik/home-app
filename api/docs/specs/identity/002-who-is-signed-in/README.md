# 002 — Who is signed in (Vitaliy or Masha)

- **Module:** `Identity` (`api/src/Modules/Identity`), no schema change
- **Architecture source:** [architecture](../../../architecture/README.md), [rules](../../../rules.md),
  [identity 001](../001-google-sign-in/README.md)
- **Status:** accepted for delivery
- **Owner:** Vitaliy · **Created:** 2026-11-27 · **Updated:** 2026-11-27
- **Decision:** [ADR 0038](../../../decisions/0038-allowlist-position-says-who-the-member-is.md)
- **Frontend SDD:** [web/docs/specs/identity/002-signed-in-person-and-profile](../../../../../web/docs/specs/identity/002-signed-in-person-and-profile/README.md)

Vitaliy (2026-11-27): «на каждой странице автоматом выбирай пользователя который вошел» (Shopping «Я», the to-do and
gift filters), and a profile page that shows the signed-in member. The web app needs to know whether the member is
Vitaliy or Masha. Written from the backend SDD template (Weretik/obsidian `Templates/backend-sdd`), condensed to one
page.

## State

- Specification: accepted · Implementation: done · Verification: [audit](audit.md) · Blockers: none (uses the
  existing `Identity__AllowedEmails__0/1`).

## Scope

In: `person` on the signed-in member. Out: per-person settings, a stored profile, notes about each other (a separate
module in its own thread), changes to the other modules.

## Rules

| ID    | Rule                                                                                                                                                                                   |
| ----- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| R-001 | The allowlist entry `Identity:AllowedEmails:0` is Vitaliy (`Vitaliy`), entry `:1` is Masha (`Maria`). An empty or missing entry keeps its index. Any later entry signs in with `person` null. |
| R-002 | `person` is computed from the settings on every request, never stored: changing the settings changes it at once, with no migration and no new sign-in.                                |
| R-003 | Everything else of identity 001 is unchanged: the allowlist still decides who may sign in, the session handler still puts e-mail, name and picture in the claims.                       |

## Scenarios

| ID     | Given / when / then                                                                                                     | Test                                                                 |
| ------ | ----------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------- |
| SC-001 | Masha (entry 1) signs in → the sign-in response's `member.person` and `GET /me` `person` are `Maria`.                   | `IdentityApiTests.A_Member_SignsIn_AndTheTokenOpensTheApi`           |
| SC-002 | Vitaliy (entry 0) calls `GET /me` → `person` is `Vitaliy`.                                                              | `IdentityApiTests.Me_SaysWhoTheMemberIsInTheHome`                    |
| SC-003 | Entry 0 is empty and entry 1 is Masha → Masha is still `Maria`; a third e-mail signs in with `person` null.             | `FamilyAllowlistTests` (`An_EmptyEntry_KeepsItsPlace`, `A_ThirdEntry_…`) |

## Design

- `Identity.Application/Features/Members/FamilyPerson` (enum `Vitaliy`, `Maria`; the same names as the other
  modules' people). `FamilyAllowlist(IEnumerable<string?>)` keeps the index; `PersonOf(email)`.
- `FamilyMember(Email, Name, PictureUrl, FamilyPerson? Person)`, built by `FamilyMember.From(profile, allowlist)` in
  the sign-in handler and in `GET /me` (`MemberClaims.Member(principal, allowlist)`).
- `IdentityInfrastructureRegistration` reads `AllowedEmails:0…N` by index, a missing index read as empty.
- Contract: `FamilyMember.person: FamilyPerson | null` (additive; `openapi.json` regenerated, web client regenerated
  in the same change). No new endpoint, no schema change, no idempotency concern (reads only).

## Change history

- 2026-11-27: first version.
