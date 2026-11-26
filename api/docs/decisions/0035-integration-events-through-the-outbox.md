# 0035. Modules talk through integration events in the outbox

Status: accepted, 2026-11-26

## Context

A gift with a giver and a buy-by date must become a to-do of the giver, and ticking that to-do done must move the gift
to «Подаренные» ([specs/gifts/002](../specs/gifts/002-gift-purchase-todo/README.md)). Gifts and Todos have separate
schemas ([0005](0005-schema-per-module.md)) and may use each other only through `Contracts`
([0002](0002-module-projects-and-contracts.md)). A direct call from one handler into the other module would write two
schemas in two transactions: a crash between them leaves the gift and the task out of step.

## Decision

- A module that must tell another one about a change publishes a message, a plain record in **its own**
  `Contracts` (`GiftPurchasePlanned`, `GiftPurchaseClosed` in `Gifts.Contracts`; `TodoTaskCompleted` in
  `Todos.Contracts`). The publishing handler calls `Publish(message)` on its module's DbContext port; the message is
  written to the module's own `outbox_messages` table in the same transaction as the data.
- `ModuleDbContext.Publish` serializes the message with the registered type name; a module that publishes maps the
  shared `OutboxMessage` table into its schema (`HasOutbox`). The outbox table is the one type a module context maps
  from outside its own namespace; the data ownership test allows exactly that type.
- The **consuming** module registers the handler (`AddOutboxMessage<TMessage, THandler>(name)`) in its own
  Application; the handler uses only its own DbContext. Handlers are safe to repeat: they upsert by a source key and
  check the current state; a lost race throws and the outbox retries.
- To break loops, a handler that applies a change it was told about publishes nothing back (a task completed because
  its gift was gifted does not publish `TodoTaskCompleted`).

## Consequences

- The gift and its task agree within one or two scheduler ticks (up to about 15 minutes, [0034](0034-scheduler-github-actions-cron.md)).
- Each module still owns its schema; Todos stores the gift id only as a plain value (`source_id`).
- Gifts and Todos now know each other's `Contracts` (`Gifts.Application` → `Todos.Contracts` for `TodoTaskCompleted`,
  `Todos.Application` → `Gifts.Contracts` for the purchase events). The project graph stays acyclic because the
  `Contracts` projects reference nothing; the two-way knowledge is limited to these three message types.
- Messages are contracts: renaming a property breaks stored messages, so a change adds a new message type.
