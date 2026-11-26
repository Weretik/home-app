# Name suggestions — scope, rules and scenarios

## Goal

As Masha or Vitaliy, when I start typing «мол» in the quick add, I want to see «Молоко», «Молоко овсяное» and
«Мороженое молочное» with their departments and how often we took them, so that I pick the usual spelling in one tap
instead of typing it, and see at once when it is already in the list.

## In scope

- One read operation: names that start with, or have a word that starts with, what is typed, from every row of the
  list ever (needed, in the cart, history).
- For each name: the latest spelling and department, how many times it was bought, when it was last added, and
  whether it is in the list now.

## Out of scope

- Fuzzy or typo-tolerant search, trigram indexes or any database extension: the production server refuses
  `CREATE EXTENSION pg_trgm` for this module ([ADR 0031](../../../decisions/0031-shopping-module-model.md), the
  module page's Database section).
- Suggestions from the department keywords: they are word starts («зубн паст»), not names.
- Remembering what was suggested or chosen; a new table or column.
- Deleting a name from the suggestions (deleting its rows does that already).

## Business rules

| ID | Rule |
| --- | --- |
| R-201 | The query is normalised as names are (`NameNormalizer`: lower case, «ё» → «е», runs of spaces as one, trimmed). After that it has 1–80 characters, else `400` with a field error on `query`. |
| R-202 | `limit` is 1–20, default 8, else `400` with a field error on `limit`. |
| R-203 | A name matches when its normalised form starts with the query, or one of its words (after a space) does. `%`, `_` and `\` in the query are literal characters, never wildcards. |
| R-204 | Rows with the same normalised name are one suggestion, whatever their status: needed, cart and history rows all count. |
| R-205 | The suggestion shows the name and department of the row added most recently (ties: the larger id, the later UUID v7). |
| R-206 | `timesBought` is the number of its rows with status `Bought` (cart and history); `lastAddedAtUtc` is the latest `added_at_utc`. |
| R-207 | `inList` is true when one of its rows is in the list now (needed, or in the cart, by `CartWindow` and the settings' `CartKeepHours`). |
| R-208 | Order: names that start with the query first, then the most bought, then the most recently added, then alphabetically; at most `limit` suggestions. |
| R-209 | A read without side effects: no `Idempotency-Key`, no rate limit of writes, open like the rest of the module ([ADR 0015](../../../decisions/0015-expenses-open-until-identity.md)). |

## Scenarios

| ID | Given | When | Then | Rules |
| --- | --- | --- | --- | --- |
| SC-201 | «Молоко» bought twice, «Йогурт молочный» once, «Мука» once | `?query=мол` | «Молоко» (`timesBought` 2) first, then «Йогурт молочный»; «Мука» absent | R-203, R-206, R-208 |
| SC-202 | «Зелёный чай» in the history | `?query=Зеле` | «Зелёный чай» in its spelling, from the latest row | R-201, R-205 |
| SC-203 | «Сыр» added as «сыр» to a department, later re-added in another spelling and department | `?query=сыр` | one suggestion with the newer spelling and department | R-204, R-205 |
| SC-204 | «Хлеб» needed now | `?query=хле` | `inList` true; a name only in the history has `inList` false | R-207 |
| SC-205 | A name with `%` or `_` in it, and others | `?query=50%` | only names that start with «50%» literally | R-203 |
| SC-206 | — | `?query=%20%20` or 81 characters, `?limit=0` or `21` | `400` with the field error | R-201, R-202 |
| SC-207 | 12 matching names | `?query=…` without `limit` | 8 suggestions | R-202, R-208 |
