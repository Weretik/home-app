# 0030. The Todos module and its model

Status: accepted, 2026-11-26

## Context

The family keeps its to-dos in the Google Sheet «✅ Список дел» (sheet «Все дела», 13 rows on 2026-11-26), fed by a
Telegram bot. It moves to the site as the second module, with more than the sheet had: steps inside a task, a status
that can always be changed back, priorities, categories the family edits, and a home page summary. The specification
is [specs/todos/001-todos-module](../specs/todos/001-todos-module/README.md).

## Decision

- One module, `Todos`, with its own schema `todos`. It knows nothing of `Expenses` and shares only the platform.
- `TodoTask` is the aggregate root. Its checklist (`TaskStep`, at most 50) lives inside the aggregate, in its own
  table `task_steps` with a cascading foreign key, and changes only through the task's methods. Every method,
  step changes included, sets `UpdatedAtUtc`, so the task row is written and its `xmin` version moves
  ([0017](0017-row-version-via-xmin-for-edits.md)).
- Edits of the task itself, status changes and deletes send the version in `If-Match` and answer `409` when it is
  stale. Step changes take no version: ticking steps quickly from two phones must never conflict. Because every step
  change writes the task row, two changes sent together race on its `xmin`; the loser reads the task again and
  applies its change once more (`StepChanges`, up to 10 attempts, each one `SaveChanges` of which only one commits).
  A change fails only when another change of the same task committed after its read, so up to 10 changes of one task
  sent together all land; past that it answers `409`.
- `TodoCategory` is its own aggregate, a table and not an enum, seeded with eight categories with fixed ids. Tasks
  reference it with a real foreign key (same schema, [0005](0005-schema-per-module.md)) `ON DELETE SET NULL`, so
  deleting a category keeps its tasks. Names are unique without regard to case (a unique index on `lower(name)`).
- Task creates take an `Idempotency-Key` ([0007](0007-idempotency-key-for-creates.md)) in `todos.create_requests`.
  Category creates do not: a repeat hits the unique name and answers `409`, which the web treats as done after a
  retry. Step adds do not either: a duplicated step is visible at once and is removed with one tap.
- Enums are stored as text. Priority is ranked only in queries (Urgent 0 … Low 3), never stored as a number.
- "Today" for overdue and due-today counts is the date in Europe/Kyiv, computed from `TimeProvider`.
- The title search is `LIKE` on `lower(title)` with a `pg_trgm` GIN index. The migration creates the extension if the
  database allows it and skips the index otherwise; the search works either way.
- The sheet is imported once by a plain idempotent SQL script (`todos-import.sql`, run by the owner in DBeaver,
  `INSERT … ON CONFLICT (import_key) DO NOTHING`), not by an endpoint as in [0014](0014-one-time-sheet-import.md):
  13 rows do not justify a keyed endpoint and a parser.
- The module is open without sign-in like Expenses ([0015](0015-expenses-open-until-identity.md)): every write uses
  the `PublicWrite` rate limit.

## Consequences

- Lists carry steps and notes, so the web opens a task without a second request; a page is at most 100 tasks.
- A category delete changes the `xmin` of its tasks (the foreign key action writes them), so an open task form may
  answer `409` once after a category it used was deleted.
- Repeating tasks, reminders and notifications are not modelled; adding them later adds fields or a module without
  changing tasks and steps.
- The import key is a plain unique index (NULLs never collide), so the script needs no `WHERE` in `ON CONFLICT`.
