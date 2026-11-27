# Code audit — Notifications module and the gift ↔ to-do link

- **Features:** [notifications/001-telegram-bot](README.md) and [gifts/002-gift-purchase-todo](../../gifts/002-gift-purchase-todo/README.md)
- **Scope:** the new module `src/Modules/Notifications` (five projects, `Initial` migration); the building-block
  changes (`IOutboxPublisher`, `ModuleDbContext.Publish`/`HasOutbox`, `SchedulerKey`, `OutboxTickEndpoint`); the
  Gifts changes (giver, buy-by, `Purchases/`, outbox, `GiftPurchasePlan` migration); the Todos changes (source,
  `Integrations/`, outbox, `TaskSourceAndOutbox` migration); the readers in Shopping and Expenses; the host
  registration, `appsettings*.json`, `api.yml`, the new `scheduler.yml`; tests and docs this change touched.
- **Stage:** final audit after implementation (skill `backend-code-audit`).
- **Date:** 2026-11-27
- **Verdict:** **pass with open findings.** No blocker. All findings are Low or Medium and accepted with a reason;
  none needs a code change before merge.

## Step 0: quality targets

[quality-targets.md](../../../architecture/quality-targets.md) exists; every value is still *proposed, to confirm*
and *not measured* (recorded there, not repeated as a finding of this feature). This change adds scheduler load
only: 192 keyed calls a day, each a few indexed reads and a handful of Telegram calls; a note with these numbers was
added to the file. No speed verdict is given beyond that.

## Boundaries and layers

- [x] `Notifications.Application` references only its own Domain/Contracts, `BuildingBlocks.Application` and the
  `Contracts` of Expenses, Gifts, Shopping, Todos. Each reader (`ITodosDigest`, `ITodoCompletion`, `IGiftsDigest`,
  `IShoppingDigest`, `IExpensesDigest`) is implemented in its owner's Application over the owner's own DbContext.
  `ModuleBoundaryTests`, `LayerDependencyTests` pass.
- [x] Domain has no EF Core, ASP.NET Core or HTTP; Telegram lives only in `Notifications.Infrastructure/Telegram`.
- [x] Schema `notifications` with its own history table; no foreign key across schemas; Todos keeps the gift id as a
  plain `source_id`.
- [x] Use cases are `Features/<Area>/Commands|Queries/<UseCase>/` with message, handler, validator; endpoints only
  bind and map through `ToHttpResult`.
- [x] Host only composes (`AddNotificationsModule`, `MapNotificationsModule`, `MediatorSetup`).
- [x] Folder axiom: no folder over six code files, no empty, wrapper or one-file folder (architecture test passes;
  the `TodoTaskSource` record was dropped to keep `Tasks/Reading` at six files).
- [x] No singleton holds a scoped service: `NotificationsSettings`, `GiftLinks` are plain values; `ITelegramBot` is a
  typed `HttpClient` (transient) with the token's request logging removed (`RemoveAllLoggers`).

## Data and idempotency

| Write | Commits | Repeat or race |
| --- | --- | --- |
| Notifications tick | one save: deliveries + outbox messages | unique `deliveries.key`; keys already present are filtered, a racing tick hits the unique index → caught, plans 0 |
| Outbox `TelegramMessage` | send, then one `ExecuteUpdate` of `sent_at_utc` | a crash between the two resends once (AF-002) |
| Webhook link / unlink | one insert or one `ExecuteDelete` | unique audience and chat id; a repeat answers «уже підключено» |
| Webhook «✅» | `ITodoCompletion`: one save, 3 attempts on a version race | an already closed task answers `AlreadyClosed` |
| Gift create / update / status / delete | one save with the purchase event | Gifts create keeps its `Idempotency-Key` |
| `GiftPurchasePlanned` / `Closed` in Todos | one save | upsert by `(source_kind, source_id)` unique; status checks |
| `TodoTaskCompleted` in Gifts | one save | only a `Wanted` gift moves |
| Unlink chat, test message (site) | one `ExecuteDelete` / no write | repeat: `404` / a second test message (AF-003) |

## Findings

| ID | Severity | Status | Path | Evidence and impact | Decision |
| --- | --- | --- | --- | --- | --- |
| AF-001 | Medium | accepted | `Gifts.Application.csproj`, `Todos.Application.csproj` | Gifts and Todos each reference the other's `Contracts` (two-way event flow). The project graph is acyclic (`Contracts` reference nothing), but the modules now know each other. | Recorded as a consequence in [ADR 0035](../../../decisions/0035-integration-events-through-the-outbox.md); limited to three message types. |
| AF-002 | Low | accepted | `Notifications.Application/Sending/TelegramMessageHandler.cs` | Telegram has no idempotency key: if the process stops after `sendMessage` and before the outbox marks the message, the next tick sends it again (a second identical digest). | At-least-once by design (ADR 0033); the window is milliseconds and the harm is one repeated message. |
| AF-003 | Low | accepted | `SendNotificationsTestEndpoint.cs` | `POST /api/notifications/test` sends outside without a request key; a double click sends two test messages. | Harmless test message, rate limited by `PublicWrite`; no state to protect (ADR 0033). |
| AF-004 | Low | accepted | `TelegramWebhookEndpoint.cs`, `NotificationsTickEndpoint.cs` | Anonymous machine endpoints without a rate limit policy. | Guarded by a secret compared in constant time (404 without a token or key); allow-listed in `EndpointRulesTests` with ADRs 0033/0034. |
| AF-005 | Low | accepted | `HandleTelegramUpdateHandler.cs` | Interactive replies are sent at once, not through the outbox; a Telegram failure loses the reply. | Best effort on purpose (ADR 0033): the person asks again; the webhook always answers 200 so Telegram does not redeliver. |
| AF-006 | Low | open | `notifications.deliveries` | No retention: about 5 rows a day, under 2 000 a year. Stores chat ids and keys, no message text. | Revisit if it passes 10 000 rows; a delete older than a year is a one-line job on the tick. |
| AF-007 | Low | accepted | `Notifications.Domain/Schedule/DigestSchedule.cs` | Russian display titles of the schedule live in Domain next to the windows. | Kept together on purpose: one place for the times and how they read on the site; plain strings, no framework dependency. |
| AF-008 | Low | open | integration tests | Ordering of the personal morning and the common message is not guaranteed (both go through one outbox tick). | Acceptable for two people; noted in the module page. Not covered by a test. |

## Architecture records

- [x] ADRs 0033 (module, webhook), 0034 (GitHub Actions cron), 0035 (integration events), 0036 (chat linking) are
  accepted and match the code.
- [x] C4 context (Telegram, cron), containers (webhook, ticks) and API components (Notifications, events between Gifts
  and Todos) updated.
- [x] `docs/deploy.md` lists `Telegram__BotToken`, `OutboxTick__Key`, `SCHEDULER_KEY` and the BotFather steps;
  `AGENTS.md` lists the module and the secrets. Nothing secret is committed (`Telegram:BotToken` is empty in
  `appsettings.json`).

## Evidence

- `dotnet build HomeApp.slnx`: success, 0 warnings; `openapi.json` regenerated.
- Unit 350 passed; architecture 77 passed; integration 92 passed (includes `NotificationsApiTests`,
  `NotificationsBotApiTests` with a recording Telegram and a fixed clock at 08:30 Kyiv, `GiftTodoFlowApiTests`).
