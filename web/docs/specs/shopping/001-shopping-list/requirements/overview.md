# Scope and outcome

## Goal

Two people (Masha and Vitaliy) keep one shopping list on their phones, at the same time, in the shop. It must be
faster than the sheet and the bot: type «молоко, хлеб, яйца 10», tap a checkbox, see what the other person added.

## Users and surfaces

- Both owners, mostly on a phone (360–400 px) in a shop, sometimes on a computer. No sign-in (API ADR 0015).
- Surfaces: `/shopping` (Список), `/shopping/history` (История), `/shopping/settings` (Настройки, the gear), and the
  «Покупки» summary on `/` (`?s=shopping`).

## In scope

- Quick add from text with a department preview, an «urgent» flag, ⭐ Частые chips, undo of the last add.
- The list grouped by department in store order; optimistic check / uncheck; edit dialog (name, quantity, note,
  department, Срочно, Удалить); «В корзине» with «Убрать купленные».
- Polling every 20 s while the page is visible and on window focus; «Я» per device.
- History by day with «Вернуть в список» and delete; «Показать ещё».
- Settings: departments (add, rename, icon, delete with confirm, reorder up/down), keywords (search, department
  filter, add, edit, delete, «выучено»), cart hours and frequent window.
- Home summary: KPI cards, departments with counts, next items, top frequent, «Открыть список».

## Out of scope

- Sign-in, push notifications, real-time sockets, offline queue of writes, drag-and-drop reorder, the sheet import
  (backend task), multiple lists.

## Non-functional targets

- List of up to ~100 rows renders without virtualisation; polling interval 20 s; department preview debounced
  400 ms. Other targets: system default (web/docs/design.md, phone first, both colour schemes).
- Accessibility: every icon button has `aria-label` and a tooltip; checkboxes are labelled by the item name; 48 px
  touch targets for checkboxes.

## Architecture questions

- Business goal as Given/When/Then: see [behavior](behavior.md).
- Access rights: ➖ no identity yet (API ADR 0015); writes are open and rate limited by the backend.
- End-to-end test: ➖ the project has no e2e runner; the main path (add → check → clear) is covered by page tests.
- Double submit: every mutation disables its trigger while pending; the add sends an `Idempotency-Key`.
- Personal data in logs: ➖ the app has no analytics or logging.
