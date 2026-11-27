# 0039. The Notes module and its model

Status: accepted, 2026-11-27

## Context

Vitaliy asked for a place where he and Masha keep what each of them likes, dislikes or should know about the other
(«Маша любит такие-то цветы, не любит молоко из АТБ, но любит из Агромола»), editable by either of them about either
of them, and asked explicitly for its own backend module with its own schema and its own section of the site, not a
part of the profile. The specification is [specs/notes/001-notes-module](../specs/notes/001-notes-module/README.md).

## Decision

- One module, `Notes`, with its own schema `notes`. It knows no other module except `Identity.Contracts`, for who is
  signed in ([0040](0040-current-member-through-identity-contracts.md)).
- `Note` is an aggregate about one person (`Vitaliy` or `Maria`), with a kind (`Likes`, `Dislikes`, `Fact`), a title,
  an optional description, an optional place or brand, an «important» flag and an optional category. A like and a
  dislike of the same thing in different shops are two notes («Молоко» likes with place «Агромол», dislikes with place
  «АТБ»), so a note stays one statement and the list reads like a sentence.
- Nothing is private: both members read and change every note. The author and the last editor are the person of the
  signed-in member, set by the server, so the list can say who wrote what.
- `NoteCategory` is editable data seeded with ten categories, like Todos ([0030](0030-todos-module-model.md)): a real
  foreign key in the same schema, `ON DELETE SET NULL`, unique names without regard to case.
- Note creates take an `Idempotency-Key` ([0007](0007-idempotency-key-for-creates.md)); edits and deletes take the
  `xmin` version in `If-Match` ([0017](0017-row-version-via-xmin-for-edits.md)). Category creates rely on the unique
  name, as in Todos.
- The list is one query without paging (at most 1000 rows), filtered by person, kind, category and words; the search
  is `ILIKE` on title, description and place without an index (the hosting forbids `pg_trgm`, hundreds of rows).

## Consequences

- The web can switch between «about me», «about my partner» and «everyone» with one cheap request each.
- A third allowlisted account (no person) can read and write notes; its notes carry no author.
- Photos, links to gifts or purchases and a home summary are not modelled; adding them adds fields or endpoints
  without changing notes.
