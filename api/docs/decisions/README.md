# Decisions

Architecture decision records: why the backend is built the way it is. Records 0001–0012 come from
[fullstack-modular-template](https://github.com/Weretik/fullstack-modular-template) and keep its examples
(`Catalog`, `Orders`); the rules apply to this project's modules the same way. Repository-wide decisions (hosting,
database server) live in [../../docs/decisions](../../../docs/decisions/README.md).

| # | Decision |
| --- | --- |
| 0001 | [A modular monolith, not microservices](0001-modular-monolith.md) |
| 0002 | [Five projects per module; other modules only through Contracts](0002-module-projects-and-contracts.md) |
| 0003 | [Business rules return Result with a typed Error](0003-result-and-typed-errors.md) |
| 0004 | [A DbContext port per module instead of repositories](0004-dbcontext-port-instead-of-repositories.md) |
| 0005 | [One PostgreSQL database with a schema per module](0005-schema-per-module.md) |
| 0006 | [Outbox delivered by a scheduled HTTP tick](0006-outbox-with-scheduled-tick.md) |
| 0007 | [Idempotency-Key for creates that must not happen twice](0007-idempotency-key-for-creates.md) |
| 0008 | [The OpenAPI document is generated from the code](0008-openapi-generated-from-code.md) |
| 0009 | Authentication for the two users: decided in [0037](0037-google-sign-in-with-server-sessions.md) |
| 0010 | [Source-generated Mediator with a validation pipeline](0010-mediator-pipeline.md) |
| 0011 | [Folder rules enforced by tests](0011-folder-rules-enforced-by-tests.md) |
| 0012 | [Rate limits on anonymous writes and sign-in](0012-rate-limits-on-anonymous-writes.md) |
| 0013 | [The Expenses module and its model](0013-expenses-module-model.md) |
| 0014 | [One-time import from the Google Sheet through a keyed endpoint](0014-one-time-sheet-import.md) |
| 0015 | [Expenses are readable and editable without sign-in until the Identity module](0015-expenses-open-until-identity.md) (superseded by 0037) |
| 0016 | [Production migrations run as EF bundles in the deploy job](0016-migrations-as-bundles-in-deploy.md) |
| 0017 | [Edits are guarded by a row version from PostgreSQL xmin](0017-row-version-via-xmin-for-edits.md) |
| 0018 | [The savings jar is a log of signed movements](0018-savings-jar-as-signed-movements.md) |
| 0019 | [Spending limits are named, overlapping category sets stored in the module](0019-spending-limits-as-category-sets.md) (carry-over part superseded by 0027) |
| 0020 | [The month summary is computed on read, with no stored totals](0020-month-summary-computed-on-read.md) |
| 0021 | [The Gifts module and its model](0021-gifts-module-model.md) |
| 0022 | [Gift variants belong to the gift and are replaced as a whole](0022-gift-variants-owned-by-gift.md) |
| 0023 | [A gifted or dropped gift moves to an archive by its status](0023-gifted-gifts-move-to-archive.md) |
| 0024 | [Gift categories are editable data with a seeded start](0024-gift-categories-as-editable-data.md) |
| 0025 | [Personal details are reference data of the Gifts module, shown read-only on the home page](0025-gift-details-as-reference-data.md) |
| 0026 | [The gifts sheet is imported once by a hand-run SQL script](0026-gifts-import-as-sql-script.md) |
| 0027 | [Overspend carries over like the sheet, computed on read from a start day](0027-limit-carry-over-like-the-sheet.md) |
| 0028 | [Planned expenses are their own table and are recorded into real expenses](0028-planned-expenses-for-the-forecast.md) |
| 0029 | [Every backend rule is checked by a test, or marked review-only with a reason](0029-every-rule-checked-by-a-test.md) |
| 0030 | [The Todos module and its model](0030-todos-module-model.md) |
| 0031 | [The Shopping module, with the list, cart and history computed on read](0031-shopping-module-model.md) |
| 0032 | [Text parsing and department matching are pure domain services with learned keywords](0032-text-parsing-and-department-matching.md) |
| 0033 | [The Notifications module: one Telegram bot with a webhook](0033-notifications-module-and-telegram-webhook.md) |
| 0034 | [The scheduler is a GitHub Actions cron](0034-scheduler-github-actions-cron.md) |
| 0035 | [Modules talk through integration events in the outbox](0035-integration-events-through-the-outbox.md) |
| 0036 | [Telegram chats are linked from Telegram, not configured](0036-telegram-chats-linked-from-telegram.md) |
| 0037 | [Texts the API and the bot write are Ukrainian, in place](0037-ukrainian-user-facing-texts.md) |
| 0037 | [Google sign-in for the family, server-side sessions, everything closed by default](0037-google-sign-in-with-server-sessions.md) |
| 0039 | [The Notes module and its model](0039-notes-module-model.md) |
| 0040 | [Other modules learn who is signed in through Identity.Contracts](0040-current-member-through-identity-contracts.md) |

## Decisions that must have a record

- Adding, merging or extracting a module.
- A new external system (payment provider, e-mail, storage, another API).
- A new way of running work (scheduled job, queue, broker, background process).
- A change of database, hosting or authentication.
- A rule that every change must follow.

New record: copy the newest file, take the next number, and keep the sections Context, Decision, Consequences. A record
that is replaced stays, with its status changed to `superseded by NNNN`.
