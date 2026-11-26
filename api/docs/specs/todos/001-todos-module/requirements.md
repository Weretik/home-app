# Todos — scope, rules and scenarios

## Goal

As Masha or Vitaliy, I want to give a task to myself, to the other one or to both of us, break it into steps, see what
is overdue or due soon, mark it done and find it later in an archive, so that the household's to-dos live in one
place that is easier than the Google Sheet.

## In scope

- Tasks with a title, notes, an assignee (Маша, Виталий, Общее), who set it (optional), a status, a priority, an
  optional due date and an optional category.
- Steps (a checklist) inside a task: add, rename, tick, delete.
- A status that can be changed at any time, including back from the archive.
- An active list and an archive (done or no longer relevant tasks), with filters by assignee and category and a
  search by words in the title.
- Categories the family adds, renames and deletes; a starting set is seeded.
- A summary for the home page: per assignee the active, overdue and due-today counts, and the tasks that need
  attention first.
- A one-time, repeatable import of every task of the sheet (done ones go to the archive) as an SQL script.

## Out of scope

- Sign-in and per-person permissions: the site is open until Vitaliy starts sign-in
  ([ADR 0015](../../../decisions/0015-expenses-open-until-identity.md) applies to this module too).
- Notifications and the morning digest (the Telegram bot keeps doing that until it is retired).
- Repeating tasks, reminders at a time of day, attachments, comments by several people, a board view.

## Actors

- Masha, Vitaliy: use the site from a phone or a computer; either can change any task.
- The old sheet: source of the import only.

## Business rules

| ID | Rule |
| --- | --- |
| R-001 | A title is trimmed, 1–200 characters. |
| R-002 | Notes are trimmed, at most 4000 characters; empty notes are stored as `null`. |
| R-003 | The assignee is `Vitaliy`, `Maria` or `Shared`. «Кто поставил» (`Author`) is `Vitaliy`, `Maria` or none. |
| R-004 | The status is one of `Todo` (Ждёт), `InProgress` (В работе), `Paused` (Отложено), `Done` (Сделано), `Cancelled` (Не актуально). The first three are active, the last two are archived. A new task starts as `Todo` unless the request says otherwise. |
| R-005 | Moving into `Done` or `Cancelled` sets `CompletedAtUtc` to now; moving back to an active status clears it. Changing between the two archived statuses keeps the first completion time. |
| R-006 | The priority is `Urgent` (Срочно), `High` (Важно), `Normal` (Обычно, the default) or `Low` (Когда-нибудь). |
| R-007 | A due date is optional, a calendar day between 2000-01-01 and 2100-12-31. |
| R-008 | A category is optional; if given it must exist (`404` `todos.category_not_found`). Deleting a category leaves its tasks without one. |
| R-009 | A task has at most 50 steps; a step title follows R-001. Steps keep the order they were added in. |
| R-010 | Every change of a task, including a change of its steps, changes the task's version; edits of the task itself and status changes send the version in `If-Match` and get `409` `todos.changed_meanwhile` when it is stale. Step changes do not need the version, so ticking steps quickly never conflicts. |
| R-011 | A category name is trimmed, 1–50 characters, unique without regard to case (`409` `todos.category_name_taken`). An optional emoji (at most 8 characters). A new category goes to the end of the list. |
| R-012 | Active list order: priority (Urgent first), then due date (earliest first, none last), then creation (oldest first). Archive order: completion time, newest first. |
| R-013 | "Overdue" is an active task with a due date before today; "today" is a due date equal to today. Today is the date in Europe/Kyiv. |
| R-014 | A create takes an `Idempotency-Key`: a repeat with the same body returns the first result, the same key with another body answers `409`. |
| R-015 | Every write is anonymous and rate limited (`PublicWrite`), like Expenses. |
| R-016 | The import keeps the sheet's `ID` as `ImportKey` under a unique index, so running it twice adds nothing twice. |
| R-017 | Lists narrow by date (added 2026-11-26): `dueFrom` / `dueTo` (days, both included, either open) or `withoutDueDate` on the due date; in the archive `completedFrom` / `completedTo` on the Europe/Kyiv day of completion. A period and `withoutDueDate` together, a period ending before it starts, or a completion period outside `view=archive` answer `400`. |

## Acceptance scenarios

| ID | Scenario |
| --- | --- |
| SC-001 | Vitaliy adds «Приклеить деревяшки» for Маша, priority Важно, due 30.11, category Дом: it appears in the active list for Маша and in «Общее» only when the assignee is Shared. |
| SC-002 | Masha ticks it done: it leaves the active list, appears at the top of the archive with the completion time. |
| SC-003 | She reopens it from the archive (status Ждёт): it is active again, completion time cleared. |
| SC-004 | A task gets three steps, two are ticked: the list shows 2/3; deleting a step leaves 2/2. |
| SC-005 | Two people edit the same task: the second save answers 409 and the form reloads. |
| SC-006 | A category is renamed and its tasks show the new name; it is deleted and its tasks stay, without a category. |
| SC-007 | The home summary shows for each of Маша, Виталий, Общее how many tasks are active, overdue and due today, and up to 8 tasks to look at first (overdue, then today, then urgent ones). |
| SC-008 | The import script runs twice: the 13 sheet rows exist once; the four done ones are in the archive. |
| SC-009 | Filters: `assignee` may repeat, so «Маша + Общее» is one request; a category filter and `withoutCategory` narrow it; search «балкон» finds «Разобрать балкон…». |

## Non-functional requirements

| Requirement | Value | Check |
| --- | --- | --- |
| Volume | hundreds of active tasks, thousands archived | lists page in SQL, at most 100 a page |
| Phone first | every list and dialog works at 360 px | frontend SDD |
| Open writes | rate limit 60 writes a minute per address | integration test of the policy on the group |
