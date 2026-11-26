# 0011. Todos: a grouped list with quick add, an optimistic done tick, steps saved at once

Status: accepted, 2026-11-26

## Context

The «Дела» section replaces the to-do tabs of the old Google Sheet and the Telegram bot. Tasks are short, added and
ticked many times a day from a phone, often on a weak connection. A task has optional steps (a checklist) and notes;
the backend changes a task's version on every change, steps included, but step writes need no `If-Match`
(api/docs/specs/todos/001-todos-module, R-010), so ticking steps quickly never conflicts. The Expenses pages are month lists edited in
small dialogs ([ADR 0007](0007-editing-in-dialogs-with-idempotency-and-versions.md)); a task list has no month and is
read by due date instead.

## Decision

- `/todos` («Задачи») shows the active tasks grouped by due date: «Просрочено», «Сегодня», «Скоро» (the next 7 days),
  «Позже», «Без срока»; empty groups are hidden and the API's order (priority, date, creation) is kept inside a group.
  Today is the date in Europe/Kyiv, as on the backend. `/todos/archive` («Архив») lists done and cancelled tasks,
  newest completion first, with «Вернуть». `/todos/settings` (the gear) holds the categories. Tabs and gear as in
  [ADR 0006](0006-section-pages-as-tabs-with-state-in-the-url.md).
- The filters (`?who=&category=&q=`) live in the URL and are kept across the two tabs. «Маша» and «Виталий» also show
  the shared tasks, as the sheet's per-person tabs did; the search writes the URL 300 ms after typing stops.
- A quick-add field sits on top of the list: Enter adds a task for the chosen «Кому» («Общее» for «Все»), Обычно,
  without a date, with an `Idempotency-Key` that is renewed after each success, so the same title added twice on
  purpose is two tasks. The full form opens from a pencil next to it or by tapping a task.
- The done tick is optimistic: the row leaves every cached list at once (`setQueriesData`), a «Готово · Отменить»
  snackbar shows, and a failure puts the rows back and says why. «Отменить» waits for the tick to be saved, reads the task
  again (the tick changed its version) and restores the previous status. «Вернуть» in the archive works the same way. Every write invalidates
  `['todos']`, which also refreshes the home summary.
- The task dialog (full screen on a phone, 600 px on a computer) edits every field. Steps of an existing task are
  saved at once through the step endpoints; the dialog follows the task's query (`['todos', 'task', id]`), so after a
  step write it holds the new version, and «Сохранить» waits while a step is being saved. Steps of a new task stay in
  the form and go with the create. A 409 `todos.changed_meanwhile` reloads the task into the form and says so.
- The home page gets the «Дела» summary ([ADR 0008](0008-home-page-as-a-registry-of-section-summaries.md)): per
  assignee counts, done in the last week, and up to eight tasks in focus that tick done in place.

## Consequences

- Adding and ticking tasks takes one tap and no dialog; a lost answer never creates a second task.
- A tick shows at once even on a slow connection, at the cost of a short rollback when the write fails.
- An edit made elsewhere while the dialog is open is picked up after the user's own step change (the task is read
  again), so a later «Сохранить» does not report a conflict for it; edits of the fields themselves still conflict.
- The todos feature keeps its own small list states, snackbar and delete confirmation instead of the Expenses ones,
  because features never import each other.

## Amendment (2026-11-26): day headings and a date filter

The coarse groups «Скоро» and «Позже» hid which day a task is due, so the active list now has a heading per day:
«Просрочено», «Сегодня», «Завтра», then one heading per later due day, «Без срока» last. The archive is grouped the
same way by the Kyiv day of completion («Сегодня», «Вчера», dates). Both lists get a date filter in the URL
(`?when=&from=&to=`): «Срок» on the active list reads the due date, «Когда сделано» in the archive reads the completion
day (API `completedFrom` / `completedTo`, R-017). The date choice is not kept across the tabs, since its meaning differs
per list. On the home page the focus list is called «На сегодня» and is shown only when it has tasks.
