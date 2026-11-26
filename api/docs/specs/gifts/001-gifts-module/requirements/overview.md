# Gifts — overview and scope

## Goal

As Masha or Vitaliy, I want to keep our gift ideas on the site — for whom, what, how much it matters, where to buy it
and in which variants, for which occasion — and see what was already given, so that we always know what to give each
other and our home, and never lose an idea in a spreadsheet.

Given the wishlist on the site
When one of us adds a gift with a few variant links and later marks it «Подарено»
Then it leaves the active list, appears under «Подаренные» with the day it was given, and the home summary counts it.

## In scope

- Gifts: add, edit, delete; recipient (Маша, Виталий, Вместе), name, category, priority, optional price, optional
  link, any number of variants (each a link with an optional title and price), an occasion with an optional date,
  notes.
- The active list (wanted) and the archive (gifted or no longer wanted) as two reads of one list; the quick status
  change «Подарено» / «Не актуально» / «Вернуть в список».
- Gift categories: list, add, rename, delete (not while used); the 11 starting categories with fixed ids.
- Personal details («нюансы»): list, add, edit, delete, per recipient.
- The home summary: per-recipient counts and sums, top wanted «🔥» gifts, upcoming occasions, all details.

## Out of scope

- The web pages and the home card (frontend SDD of `web/src/features/gifts`).
- Importing the sheet: a hand-run SQL script owned by the coordinator, not an endpoint
  ([ADR 0026](../../../../decisions/0026-gifts-import-as-sql-script.md)).
- Sign-in and access rules: the site is open until the Identity module
  ([ADR 0015](../../../../decisions/0015-expenses-open-until-identity.md)); Gifts follows the same rule.
- Reordering gifts, details or categories by hand; price history; currencies other than hryvnia; reminders before an
  occasion; sharing a list outside the household.

## Actors and external systems

- Masha and Vitaliy: read and edit everything, on phones, sometimes at the same time.
- Operator (coordinator): runs the one-time import SQL against the database after the `Initial` migration.
- The web app: the only client, through the generated OpenAPI contract.

## Non-functional requirements

| Requirement | Value | How it is checked |
| --- | --- | --- |
| Data volume | ~100 gifts today (91 from the sheet), at most a few hundred; ≤ 20 variants per gift; tens of details; ~10 categories | variant limit in the domain test; lists are unpaged by design |
| Latency | system default; there is no quality targets file in the repository (see audit AF-001). Each read is one or two indexed SQL queries over a few hundred rows | integration tests; query shape reviewed in the audit |
| Concurrency | two people editing the same gift: the later save with an old version is refused, not lost | `xmin` row version, integration test |
| Repeats | a create repeated by a flaky phone connection makes one row | `Idempotency-Key`, integration test |
| Access | open, writes rate limited (`PublicWrite`, 60 a minute per address) | endpoint metadata, OpenAPI `429` responses |
| Personal data | details hold body sizes of the two people (ring, shoes, clothes); no names beyond the two fixed recipients; nothing is logged beyond the request path | code review; no logging of bodies |

## Assumptions decided without asking

The owner assigned specification and implementation together and asked not to stop for questions. These points were
decided and are recorded here and in the ADRs:

- **A-1** Variant ids are new on every gift update (the list is replaced as a whole, target values); clients must not
  keep variant ids across an edit.
- **A-2** `giftedOn` sent with a status other than `Gifted` is ignored (the date is cleared), not rejected.
- **A-3** The status change carries the version in `If-Match` like every other edit; a stale version answers `409`.
- **A-4** Category rename and delete carry no version: a rename is a target value and categories have no other
  fields. Category creates still take an `Idempotency-Key`, like every create of the project.
- **A-5** Active list order: priority (`High`, `Normal`, `Someday`), then occasion date ascending (past dates first,
  no date last), then creation time. Archive order: `giftedOn` newest first, no date last, then creation time newest
  first.
- **A-6** "Gifted sum" counts known prices of `Gifted` gifts only; `Dropped` gifts count nowhere in the summary.
- **A-7** A new category goes to the end (last `sort_order` + 10); a new detail goes to the end of its recipient's
  details. Edits keep the position.
- **A-8** Prices, variant prices and details are plain text/amounts in hryvnias; the price of a variant does not change
  the gift's price.
- **A-9** The summary's top list holds up to five `High` wanted gifts per recipient, ordered like the active list;
  upcoming occasions are up to five wanted gifts overall with `occasionDate` ≥ today in Kyiv.

## Open questions

- None blocking. The missing quality-targets file is a repository-wide gap, recorded in the audit, not a product
  question of this feature.
