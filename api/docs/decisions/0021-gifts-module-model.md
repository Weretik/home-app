# 0021. The Gifts module and its model

Status: accepted, 2026-11-26

## Context

The family keeps its gift ideas in the Google Sheet «🎁 Список подарков» (and fed it from a Telegram bot): for whom,
what, category, priority, price, link, more links in the notes, an occasion, and a «Куплено» mark. It is the next
sheet to move to the site. No existing module owns this data: Expenses records money that was spent, while a gift
idea is a wish with its own life cycle, its own categories and its own reference data (sizes).

Options considered: growing Expenses (unrelated data, different categories, two areas that change independently
would share one schema); one generic "lists" module for shopping, to-dos and gifts (the three share almost no rules);
a module of its own (chosen).

## Decision

- A new module `Gifts`, five projects like every module ([0002](0002-module-projects-and-contracts.md)), with its own
  schema `gifts` and migration history `gifts.__ef_migrations_history` ([0005](0005-schema-per-module.md)).
  `Gifts.Contracts` stays empty: no module needs gifts, and Gifts needs nothing from Expenses.
- `Gift` is the aggregate root: `Recipient` (`Maria`, `Vitaliy`, `Together`), name, category, `Priority` (`High`,
  `Normal`, `Someday`), optional price (`numeric(12,2)`, `null` = unknown, otherwise the money rules of Expenses),
  optional link (absolute `http`/`https`), occasion and its date, notes, `GiftStatus` (`Wanted`, `Gifted`, `Dropped`),
  `gifted_on`, creation time. Its variants are part of it ([0022](0022-gift-variants-owned-by-gift.md)).
- Enums are stored as their names (`varchar(16)`), as in Expenses, so the data reads without the code and the import
  SQL can write them.
- Table and column names are fixed (`gifts.categories`, `gifts.gifts`, `gifts.gift_variants`, `gifts.details`,
  `gifts.create_requests`), because the one-time import writes into them ([0026](0026-gifts-import-as-sql-script.md)).
- Writes follow the project's patterns: `Idempotency-Key` on every create ([0007](0007-idempotency-key-for-creates.md)),
  the `xmin` row version in `If-Match` on edits of gifts and details and on the status change
  ([0017](0017-row-version-via-xmin-for-edits.md)), the `PublicWrite` rate limit on every write. Like Expenses, the
  module is open without sign-in until the Identity module ([0015](0015-expenses-open-until-identity.md) applies to
  Gifts too).
- The home summary is a read model computed on every request, as the month summary is
  ([0020](0020-month-summary-computed-on-read.md)): per-recipient counts and sums, top `High` wanted gifts, upcoming
  occasions and the details.
- Lists are not paged: a few hundred gifts at most.

## Consequences

- The wishlist can grow (reminders, sharing, reordering) without touching Expenses.
- "Together" gifts (things for the home) are a recipient like the two people, so every list, filter and summary
  handles three recipients without special cases.
- Prices are wishes, not money movements; the summary's sums are estimates and never feed the budget.
- Unpaged lists are a deliberate limit; if the list ever reaches thousands of rows, `listGifts` gets paging.
