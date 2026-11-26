# 001 — «Дела» section (family to-do list)

- **App type:** React Web (Vite, MUI, React Query, React Router)
- **App:** `web/`, feature `src/features/todos`, home summary `src/features/home/todos-summary`
- **Architecture source:** [architecture](../../../architecture.md), [rules](../../../rules.md), [design rules](../../../design.md)
- **Status:** accepted for delivery
- **Owner:** Vitaliy · **Created:** 2026-11-26 · **Updated:** 2026-11-26
- **Backend SDD:** [api/docs/specs/todos](../../../../../api/docs/specs/todos/001-todos-module/README.md) (rules R-_, scenarios SC-_,
  contract)

The «Дела» section replaces the «Coming soon» placeholder at `/todos`: a phone-first list of the family's tasks with
quick add, one-tap done, a task dialog with steps and notes, an archive, the section's settings (categories), and a
«Дела» summary on the home page. Written from the frontend SDD template (Weretik/obsidian `Templates/frontend-sdd`),
React variant, condensed.

## State

- Specification: accepted · Implementation: done · Verification: local checks green, final audit done · Blockers:
  none

## Requirements and behaviour

The product rules are the backend SDD's (R-001…R-016). The screens add these:

| ID     | Behaviour                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| ------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| FR-001 | `/todos` «Задачи»: active tasks grouped «Просрочено», «Сегодня», «Скоро» (next 7 days), «Позже», «Без срока»; empty groups hidden. Inside a group the API order (priority, date, creation).                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| FR-002 | Filters on one row, kept in the URL: «Кому» chips (Все · Маша · Виталий · Общее; `?who=maria`), category select (`?category=<id>` or `none`), search field (`?q=`, 300 ms debounce). «Маша» and «Виталий» also show «Общее» tasks, as the old sheet's per-person tabs did; «Общее» shows only shared ones.                                                                                                                                                                                                                                                                                                                         |
| FR-003 | Quick add on top of the list: a text field «Новая задача…», Enter or the button adds a task with the chosen «Кому» filter as assignee («Общее» when «Все»), `Normal` priority, no date; the field clears and keeps focus.                                                                                                                                                                                                                                                                                                                                                                                                          |
| FR-004 | A row: a round checkbox (tick = `Done`, with a «Готово · Отменить» snackbar that restores the previous status), the title (struck through while saving), then on one wrapping line: a priority mark (🔥 Срочно, ⭐ Важно, 💭 Когда-нибудь; none for Обычно), the due date («сегодня», «завтра», «пн 30.11»; red when overdue), category chip with emoji, «2/5» steps, a notes icon when there are notes, the assignee avatar (М / В / two people) and status chip when not `Todo`. Tap opens the task.                                                                                                                             |
| FR-005 | The task dialog (full screen on a phone, 600 px on a computer) for new and existing tasks: title, «Кому» (segmented: Маша · Виталий · Общее), «Поставил(а)» (Маша · Виталий · не указано), status select, priority (segmented with icons), due date with quick chips «Сегодня», «Завтра», «На выходных», «Без срока», category select, notes (multiline), and «Шаги»: a checklist with tick, inline rename, delete and an «Добавить шаг» field. Steps of an existing task save at once through the step endpoints; steps of a new task are sent with the create. «Сохранить» saves the rest with `If-Match`; «Удалить» asks first. |
| FR-006 | `/todos/archive` «Архив»: done and cancelled tasks, newest completion first, «Показать ещё» paging, the same filters; a row shows the completion date and a «Вернуть» action (status `Todo`).                                                                                                                                                                                                                                                                                                                                                                                                                                      |
| FR-007 | `/todos/settings` «Настройки» (gear next to the tabs, like Expenses): the category list with emoji, name and active task count; add, rename, change emoji and delete in dialogs; delete warns how many tasks lose the category.                                                                                                                                                                                                                                                                                                                                                                                                    |
| FR-008 | Home summary «Дела» in the switcher (`?s=todos`): three figure cards (Маша, Виталий, Общее: active count, with «просрочено N» in red and «сегодня N» when non-zero), a «Сделано за неделю» line, the «В фокусе» list (up to 8 rows from the summary's `focus`, each ticks done in place) and «Открыть дела».                                                                                                                                                                                                                                                                                                                       |
| FR-009 | States: loading skeleton rows; an empty list «Задач нет. Добавьте первую выше»; a filtered empty list offers «Сбросить фильтр»; an error shows the message and «Повторить». A 409 `todos.changed_meanwhile` or a 404 reloads the list and says so.                                                                                                                                                                                                                                                                                                                                                                                 |
| FR-010 | Everything works at 360 px without horizontal scroll, in both colour schemes, with design tokens only.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |

## Design

### Structure (`src/features/todos`, at most 6 production files a folder)

```
todos/
  data/           todos-api.ts (task list infinite query, one task), categories-api.ts, labels.ts (Russian labels of
                  enums), due-date.ts (buckets, «сегодня/завтра/пн 30.11», Kyiv today)
  writes/         task-writes-api.ts (create / edit / delete, Idempotency-Key), status-api.ts (optimistic move, undo),
                  steps-api.ts, category-writes-api.ts, failures-api.ts (Russian text of a failed write)
  section/        TodosSection (title, tabs «Задачи» / «Архив», gear), todos-pages.ts
  url-state/      use-todo-filters.ts (?who=&category=&q=, debounced search), filters.ts (mapping to the API query)
  task-filters/   TaskFilters, WhoChips, SearchField
  task-list/      ActiveTasksPage, TaskGroups, QuickAdd, use-done-tick.ts
  task-row/       TaskRow, TaskMeta (the wrapping meta line), DoneCheckbox, AssigneeAvatar
  task-dialog/    TaskDialog (frame, save/delete), TaskFields, DueDateField, SegmentedField, task-form.ts (values ↔
                  request, checks), use-task-form.ts
  task-steps/     StepsEditor, StepItem, AddStepField, use-steps.ts (step mutations for an existing task, local list
                  for a new one)
  feedback/       ListStatus, LoadMoreButton, ResultSnackbar, ConfirmDeleteDialog
  archive/        ArchivePage, ArchiveRow
  settings/       CategoriesSettingsPage, CategoryRow, CategoryDialog, DeleteCategoryDialog, category-form.ts
home/todos-summary/
  data/           todos-summary-api.ts (GET /api/todos/summary, the focus tick), labels
  view/           TodosSummary, AssigneeCards, FocusList
```

The ≤6 rule is checked by `npm run architecture:structure`; the final audit checked the result.

### State and data

- Server state only in React Query. Keys: `['todos', 'tasks', view, filters]`, `['todos', 'task', id]` (the open dialog), `['todos', 'categories']`,
  `['todos', 'summary']` (home feature). Every task or step write invalidates `['todos']`; category writes also.
  The home summary invalidates on its own done tick.
- Create sends `Idempotency-Key` (one per opened form or quick-add text, renewed after success), edits send the
  `version` in `If-Match` (as [ADR 0007](../../../decisions/0007-editing-in-dialogs-with-idempotency-and-versions.md)).
- Done tick is optimistic: the row moves out at once, rolls back with the error message on failure.
- URL state with `useSearchParams`, as Expenses (ADR 0006).

### Routes and shell

- `App.tsx`: `/todos` → `TodosSection` with index `ActiveTasksPage`, `archive`, `settings`; the placeholder route is
  removed. `sections.ts` already lists «Дела».
- `home/page/summary-sections.ts`: the `todos` entry gets `Summary: TodosSummary`.

### Size limits

Component file ≤ 150 lines, hook or module ≤ 200 lines, split by responsibility. `tools/check-structure.mjs` does
not measure lines (it checks folders, imports and the API boundary), so the sizes were checked by hand in the
[audit](audit.md) (AF-007).

## Tasks and traceability

| ID     | Responsibility                                                                      | Test                                    | Covers                        |
| ------ | ----------------------------------------------------------------------------------- | --------------------------------------- | ----------------------------- |
| TS-101 | Regenerate the client from the contract                                             | `npm run contracts:generate`, typecheck | —                             |
| TS-102 | `due-date.ts` buckets and labels, `task-form.ts` checks and mapping, filter mapping | Vitest unit                             | FR-001, FR-002, FR-005        |
| TS-103 | Data hooks (reads, writes, steps, categories)                                       | through page tests with mocked fetch    | FR-009                        |
| TS-104 | Active page: groups, filters, quick add, done with undo                             | Testing Library                         | FR-001…FR-004, SC-001, SC-002 |
| TS-105 | Task dialog with steps                                                              | Testing Library                         | FR-005, SC-004, SC-005        |
| TS-106 | Archive with «Вернуть»                                                              | Testing Library                         | FR-006, SC-003                |
| TS-107 | Category settings                                                                   | Testing Library                         | FR-007, SC-006                |
| TS-108 | Home summary                                                                        | Testing Library                         | FR-008, SC-007                |
| TS-109 | Routes, docs (architecture, ADR), structure check                                   | `npm run architecture:structure`, lint  | FR-010                        |

## Readiness

- [x] Behaviour, states and URLs defined; contract taken from the backend SDD.
- [x] Delivery: typecheck, lint, tests, structure check, format check and build green locally (CI pending).
- [x] Final frontend code audit with a runtime smoke run: [audit.md](audit.md), findings fixed or accepted.
