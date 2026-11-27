# 0017. «Про нас»: a section of its own with person tabs, «about me» first

Status: accepted, 2026-11-27

## Context

Vitaliy asked for notes about each other («Маша любит …, Виталий не любит томатный сок») as a separate page and
backend module, where each of them can look «про себя или про партнёра» and also see everything. The site knows who
is signed in and, since the identity work, which person of the household that is (`person` in `/api/identity/me`,
read through `platform/member-api`). The main menu keeps five sections for the phone's bottom bar.

## Decision

- A new feature `src/features/notes` and section `/notes` «Про нас», in the side menu under the main five and linked
  on a phone from the home page and the profile page; the bottom bar stays at five.
- Tabs in the URL ([0006](0006-section-pages-as-tabs-with-state-in-the-url.md)) are relative to the signed-in member:
  «Про мене» (default), «Про <партнера>» by name, «Усі». A member without a person sees «Віталій», «Маша», «Усі».
- The list is cards grouped by category, each card one statement with a coloured kind mark; filters (kind, category,
  words) live in the URL and stay across tabs. A quick add on top creates a note about the tab's person in one step;
  the dialog edits everything ([0007](0007-editing-in-dialogs-with-idempotency-and-versions.md)).

## Consequences

- The same link («?about=me») shows each member their own notes, so it can be shared between the two.
- Adding a note takes one line; categories and descriptions are added when needed, not forced.
