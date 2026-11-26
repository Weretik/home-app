# Entries and sheet import — domain design (as implemented)

## Responsibility

- `Expense` and `Income` (`Domain/Entries/`) are aggregates with private setters. `Create` and `Update` run
  `EntryRules.Check` (date range, amount > 0, ≤ 10 000 000, two decimals, comment ≤ 500, import key ≤ 100) and
  normalise the comment. `Update` keeps `ImportKey` and `CreatedAtUtc`. Ids are `Guid.CreateVersion7(now)`.
- `Version` maps PostgreSQL `xmin` ([ADR 0017](../../../../decisions/0017-row-version-via-xmin-for-edits.md)).
- `Category` (`Domain/Categories/`) is data, not an enum; `Payer`, `Person`, `PaymentMethod` are enums stored as
  names ([ADR 0013](../../../../decisions/0013-expenses-module-model.md)).
- Errors are typed `Result` errors with stable codes (`EntryErrors`, `CategoryErrors`,
  [ADR 0003](../../../../decisions/0003-result-and-typed-errors.md)).
- The import's cell parsing and key making live in Application (`Features/Imports/SheetFormat/`), not in Domain: the
  sheet format is an adapter concern; the domain only sees ordinary `Create` calls with an import key.

## Cross-module interaction

None; `Expenses.Contracts` is empty.

## Risks

- Lost update between two editors: handled by the version check plus `DbUpdateConcurrencyException`.
- Delete without version (GAP-001 in [overview](../requirements/overview.md)).
- Duplicate creates from retries: handled by `Idempotency-Key` ([ADR 0007](../../../../decisions/0007-idempotency-key-for-creates.md)).
- Import repeated: handled by unique import keys; a parallel import losing the race answers `ImportErrors.AlreadyRunning`.
