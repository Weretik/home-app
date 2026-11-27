# 0038. The allowlist position says who the member is

Status: accepted, 2026-11-27.

## Context

After sign-in ([0037](0037-google-sign-in-with-server-sessions.md)) the web app knew the member's e-mail, name and
picture, but not whether they were Vitaliy or Masha. The sections still asked by hand: Shopping «Я», the to-do filter
«Кому» and the gift filter «Для кого». Vitaliy asked that every page start on whoever signed in
(spec [identity/002](../specs/identity/002-who-is-signed-in/README.md)). The modules already name the two people
`Vitaliy` and `Maria` (Shopping `Person`, Todos `Assignee`, Gifts `Recipient`, Notifications `Audience`).

Options:

1. A setting per person (`Identity:Members:Vitaliy = <e-mail>`): explicit, but Vitaliy would have to rename the App
   Service variables he set an hour earlier.
2. A choice stored in the database the first time each member signs in: one more screen and table for two people.
3. The position on the existing allowlist: `Identity:AllowedEmails:0` is Vitaliy, `:1` is Maria. That is already how
   the deployment guide told him to fill it in.

## Decision

Option 3. `FamilyAllowlist` keeps each entry's index (an empty or missing index keeps its place) and
`PersonOf(email)` answers `Vitaliy` for entry 0, `Maria` for entry 1, and null for any later entry or an unknown
e-mail. The value travels as `person` (`FamilyPerson`, nullable) on `GET /api/identity/me` and on the sign-in
response; it is computed from the settings on each request, not stored, so swapping the two entries fixes a mix-up
at once. Other modules that need the current person read it through the Identity contracts, never from claims.

## Consequences

- No new configuration and no migration; the deployment guide states which entry is whom.
- Swapping the two variables swaps the people everywhere on the next request; a third e-mail may sign in as a guest
  that no section picks.
- Revisit if the family grows past two named people: then option 1 or 2.
