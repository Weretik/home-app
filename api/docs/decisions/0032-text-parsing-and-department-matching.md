# 0032. Text parsing and department matching are pure domain services with learned keywords

Status: accepted, 2026-11-26

## Context

The bot read «молоко, хлеб, яйца 10, сыр 300 г» with two regular expressions (`parseItems_`) and put every item into a
department with a keyword dictionary (`deptOf_`): exact name first, then the longest keyword whose words start words
of the name, «Другое» otherwise. When someone moved an item to another department, the bot appended «exact name →
department» to the dictionary. The family is used to these results; the site must give the same ones.

## Decision

- `ItemTextParser`, `NameNormalizer` and `DepartmentMatcher` are pure static/immutable classes in `Shopping.Domain`,
  ported line by line from the bot (including its `UNITS` list and its quirks: a decimal comma splits the text, only
  ASCII digits are numbers) and tested with the bot's cases. No language model, no fuzzy matching.
- The matcher takes the keywords in a defined order (department store order, then keyword) so ties are
  deterministic; the Application layer builds it from the tables per request (a few hundred rows).
- Keywords are stored normalised and unique. Moving an item to a non-fallback department upserts its normalised name
  as a `Learned` keyword, in the same save as the edit; the settings page shows the «выучено» marker and lets the
  family edit or delete it.
- `GET /departments/match` exposes the same matcher for the preview while typing.

## Consequences

- Parsing and matching are fast, predictable and fully unit-tested; their results equal the bot's for the same
  dictionary.
- A wrong learned keyword is fixed like any keyword, in the settings.
- Better matching (stems, typos, a model) would replace the matcher behind the same calls, with its own decision.
