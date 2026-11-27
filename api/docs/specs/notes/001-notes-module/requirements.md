# Notes — scope, rules and scenarios

## Goal

As Masha or Vitaliy, I want to write down what my partner (or I) likes, dislikes or simply needs the other one to
know, sorted by categories and with a description and a place or brand, so that we understand each other better and
can look it up before a purchase, a gift or a dinner.

## In scope

- Notes about one person (Vitaliy or Masha): a kind (likes, dislikes, a fact), a title, an optional description, an
  optional place or brand («Агромол», «Rozetka»), an optional category, an «important» mark.
- Who wrote a note and who changed it last, taken from the signed-in member, never from the request.
- Lists filtered by person, kind, category and words, for the «about me», «about my partner» and «everyone» views.
- Categories the family adds, renames and deletes; a starting set is seeded.

## Out of scope

- Private notes visible to one person only: everything is shared between the two, as Vitaliy asked.
- Photos and links, a home page summary, Telegram messages, links to gifts or the shopping list (later, if asked).
- An import: there is no sheet with this data.

## Actors

- Masha, Vitaliy: signed in with Google ([Identity](../../identity/001-google-sign-in/README.md)); either may change
  any note, about themselves or about the other.

## Business rules

| ID | Rule |
| --- | --- |
| R-001 | A note is about exactly one person: `Vitaliy` or `Maria`. |
| R-002 | The kind is `Likes` (Любить), `Dislikes` (Не любить) or `Fact` (Факт). |
| R-003 | A title is trimmed, 1–200 characters (`notes.title_invalid`). |
| R-004 | A description is trimmed, at most 2000 characters; empty is stored as `null` (`notes.details_too_long`). |
| R-005 | A place or brand is trimmed, at most 200 characters; empty is stored as `null` (`notes.place_too_long`). |
| R-006 | A category is optional; if given it must exist (`404` `notes.category_not_found`). Deleting a category keeps its notes, without a category. |
| R-007 | «Important» is a flag, `false` by default; important notes come first inside their category. |
| R-008 | The author (`CreatedBy`) and the last editor (`UpdatedBy`) are the person of the signed-in member (Identity contract `ICurrentMember`), `null` when the member has no person in the household. A request cannot set them. |
| R-009 | Edits and deletes send the version in `If-Match`; a stale one answers `409` `notes.changed_meanwhile`; a missing note `404` `notes.note_not_found`. |
| R-010 | A create takes an `Idempotency-Key`: a repeat with the same body returns the first result, the same key with another body answers `409`. |
| R-011 | A category name is trimmed, 1–50 characters, unique without regard to case (`409` `notes.category_name_taken`); an optional emoji of at most 8 characters. A new category goes to the end. |
| R-012 | List order: by category order (notes without a category last), inside a category important first, then the kind (Likes, Dislikes, Fact), then the title without regard to case. |
| R-013 | List filters: `about` (one person), `kind` (repeatable), `categoryId` or `withoutCategory` (not both: `400`), `search` (every word must occur in the title, the description or the place, without regard to case). At most 1000 notes are returned, no paging. |
| R-014 | Reads need a signed-in member or the scheduler key (the deploy smoke check); writes need a signed-in member and use the `PublicWrite` rate limit like every module. |

## Acceptance scenarios

| ID | Scenario |
| --- | --- |
| SC-001 | Vitaliy adds about Маша: Не любить «Молоко», place «АТБ», category Їжа; and Любить «Молоко», place «Агромол». Both appear in the list about Маша, in Їжа, the «Любить» one first; the author is Vitaliy. |
| SC-002 | Masha adds about herself: Любить «Півонії», important, category Квіти. Filtering `about=Maria&kind=Likes` shows it; `about=Vitaliy` does not. |
| SC-003 | Masha edits Vitaliy's note «Томатний сік» (Не любить): the note keeps Vitaliy as the author and gets Masha as the last editor. |
| SC-004 | Two people edit the same note: the second save answers `409`; a delete with a stale version answers `409` too. |
| SC-005 | A category is renamed: its notes show the new name; it is deleted: its notes stay, without a category, at the end of the list. |
| SC-006 | Search «агромол» finds the note by its place; «окрошка літо» finds a note whose title is «Окрошка» and description mentions «літо». |
| SC-007 | Without a session every read and write answers `401`; the scheduler key reads the list but cannot create a note. |

## Non-functional requirements

| Requirement | Value | Check |
| --- | --- | --- |
| Volume | hundreds of notes | one query, at most 1000 rows |
| Phone first | every list and dialog works at 360 px | frontend SDD |
| Privacy | notes are personal family data, visible only to the two members | Identity fallback policy, integration test |
