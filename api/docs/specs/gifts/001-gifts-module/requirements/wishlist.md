# Gifts — wishlist, archive, categories, details and summary

## Behaviour

- A person adds a gift idea for Маша, Виталий or both («Вместе»), with as much as they know: a name and a category
  always, a price, a link, variants, an occasion and notes when they have them.
- The idea stays in the active list until it is gifted (or bought: the sheet's «Куплено») or dropped; then it moves to
  the archive. It can be moved back.
- Categories and details are kept in the settings of the section; the home page only shows details.

## Business rules

### R-001 — A gift has a recipient, a name and a category

The recipient is one of `Maria`, `Vitaliy`, `Together`. The name is trimmed and 1–200 characters. The category must
exist. The priority is one of `High` («🔥 Очень хочу»), `Normal` («⭐ Хочу»), `Someday` («💭 Когда-нибудь»).

### R-002 — A price is optional and follows the money rules

No price means unknown. A price is greater than 0, at most 10 000 000, with at most two digits after the point
(hryvnias with kopecks). The same holds for a variant's price.

### R-003 — Links are absolute web addresses

A link (the gift's or a variant's) is an absolute `http` or `https` address of at most 2000 characters. The gift's
link is optional; a variant always has one.

### R-004 — Variants belong to the gift

A gift has 0–20 variants, each a link with an optional title (trimmed, ≤ 200, empty means none) and an optional
price. They are kept in the order sent. An edit of the gift sends the whole list, which replaces the stored one.

### R-005 — Occasion and notes

The occasion is trimmed text of at most 100 characters; empty means none. Its date is optional and lies in
2000-01-01…2100-12-31. Notes are trimmed text of at most 2000 characters; empty means none.

### R-006 — Status

A new gift is `Wanted`. An edit keeps the status. A status change sets `Wanted`, `Gifted` or `Dropped`. With
`Gifted` the gift gets the day it was given: the day sent, else the day it already has (a repeated «Подарено»
changes nothing), else today in `Europe/Kyiv`; the day lies in 2000–2100. Any other status clears the day.

### R-007 — Active list and archive

The active list shows `Wanted` gifts, ordered by priority (`High` first), then the nearest occasion date (no date
last), then creation time. The archive shows `Gifted` and `Dropped` gifts, the latest gifted day first (no day last),
then the newest created. Both are complete, without pages.

### R-008 — Edits do not overwrite each other

Every edit of a gift or a detail, and every status change, carries the version it started from. If someone changed
the row meanwhile, the edit is refused and nothing changes.

### R-009 — A repeated create makes one row

Creating a gift, a category or a detail takes a request key. The same key with the same data returns the first
result; the same key with other data is refused.

### R-010 — Categories are editable data

A category has a unique name (trimmed, 1–100 characters). The 11 starting categories (Техника … Другое) exist from
the start with fixed ids. A category can be added (at the end), renamed, and deleted only while no gift uses it.

### R-011 — Details are per-person reference data

A detail has a recipient, a label (trimmed, 1–100, e.g. «Кольцо») and a value (trimmed, 1–200, e.g. «18,5–19,0»).
New details go to the end of their recipient's list. Details are edited and deleted only in settings; the summary
shows all of them, by recipient (Маша, Виталий, Вместе) and position.

### R-012 — The summary is computed on every read

Per recipient and in total: the number of wanted gifts, the sum of their known prices, how many have no price, how
many are `High`; the number of gifted gifts and the sum of their known prices. Per recipient: up to five `High`
wanted gifts (name, price, link). Overall: up to five wanted gifts whose occasion date is today (Kyiv) or later,
nearest first. All details. Nothing is stored.

### R-013 — Open, rate-limited writes

Reads and writes need no sign-in (as Expenses, ADR 0015). Every write counts against the `PublicWrite` rate limit.

## Acceptance scenarios

### SC-001 — Add a gift with variants

**Covers:** R-001, R-002, R-003, R-004, R-005, R-006, R-009

Given the category «Украшения» exists
When Vitaliy adds «Кольцо с камнем» for Маша, priority `High`, without a price, with two variants (one with a title
and a price, one with only a link) and the occasion «День рождения» on 2040-03-14
Then the gift is created as `Wanted` with no price
And the active list shows it with both variants in the order sent.

| Input | Result |
| --- | --- |
| name `"  Кольцо  "` | stored `Кольцо` |
| name empty or 201 characters | 400 `gifts.name_required` / `gifts.name_too_long` |
| price `0`, `-1`, `10000000.01`, `1.234` | 400 `gifts.price_invalid` |
| link `ftp://x`, `example.com`, 2001 characters | 400 `gifts.link_invalid` |
| 21 variants | 400 `gifts.too_many_variants` |
| occasion date 1999-12-31 | 400 `gifts.occasion_date_out_of_range` |
| unknown category | 404 `gifts.category_not_found` |

### SC-002 — A repeated create returns the first gift

**Covers:** R-009

Given a gift was created with key K
When the same request with key K arrives again
Then the answer is the first gift's id and there is still one gift
And the same key K with another name is refused with 409 `gifts.idempotency_key_reused`.

### SC-003 — Edit a gift and replace its variants

**Covers:** R-004, R-006, R-008

Given a `Gifted` gift with three variants and version V
When Masha saves it with version V, a new name and one variant
Then the gift has the new name, exactly one variant, and is still `Gifted` with the same day.

### SC-004 — A stale edit is refused

**Covers:** R-008

Given a gift edited by Vitaliy after Masha opened it
When Masha saves with the version she started from
Then the answer is 409 `gifts.changed_meanwhile` and the gift keeps Vitaliy's values
And an edit without a version is 400.

### SC-005 — Mark gifted, drop, and return

**Covers:** R-006, R-007

Given a wanted gift
When it is marked `Gifted` without a day
Then it leaves the active list, appears in the archive with today's Kyiv date
When it is moved back to `Wanted`
Then it is in the active list again without a gifted day
When it is marked `Dropped` with a day
Then it is in the archive without a gifted day.

### SC-006 — List order

**Covers:** R-007

Given wanted gifts with priorities `Someday`, `High`, `Normal`, two `High` ones with occasion dates 2040-05-01 and
2040-02-01 and one without
When the active list is read
Then `High` gifts come first, the one on 2040-02-01 before 2040-05-01 before the undated one, then `Normal`, then
`Someday`.

### SC-007 — Delete a gift

**Covers:** R-004

Given a gift with variants
When it is deleted
Then it and its variants are gone, and a second delete answers 404 `gifts.gift_not_found`.

### SC-008 — Manage categories

**Covers:** R-010

Given the starting categories
When a category «Книги» is added, renamed to «Книги и комиксы», and deleted
Then each step answers success and the list reflects it
And adding or renaming to an existing name answers 409 `gifts.category_name_taken`
And deleting a category a gift uses answers 409 `gifts.category_in_use`.

### SC-009 — Manage details

**Covers:** R-008, R-011

Given no details for Маша
When «Кольцо» = «18,5–19,0» and «Обувь» = «38» are added for Маша
Then the details list shows them in that order
When «Обувь» is edited with its version to «38,5»
Then the list shows the new value; a stale version answers 409; a delete removes it.

### SC-010 — Home summary

**Covers:** R-012

Given for Маша two wanted gifts (one `High` for 1 000, one without a price), one gifted for 500 and one dropped, and a
wanted gift for both with an occasion tomorrow
When the summary is read
Then Маша has 2 wanted, sum 1 000, 1 without a price, 1 `High`, 1 gifted, gifted sum 500, and the `High` gift in her
top list
And the total adds up all recipients
And the gift for both is among the upcoming occasions
And all details are listed by recipient and position.

### SC-011 — Writes are rate limited and documented

**Covers:** R-013

Given the generated OpenAPI document
When it is read
Then every Gifts write lists 429, every operation has a unique camelCase name and a summary.

## Deferred scenarios

- Reordering by drag and drop (sort orders exist in the data, but there is no use case to change them yet).
- Moving a variant into the gift's own link ("this is the one").
- Reminders before an occasion (would need the outbox and a notification channel).
