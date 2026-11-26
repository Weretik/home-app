# Rules and scenarios

## Rules

- **R-1 Quick add.** Text with several items (`, ; newline`) is sent as one add; «Я» is the author. A single item
  shows its department before it is added. An «urgent» flag applies to everything added.
- **R-2 Undo.** After an add a snackbar names what was added and offers «Отменить», which deletes the newly added
  rows (with their versions). Restored or updated rows are not undone.
- **R-3 Grouping.** Needed items are grouped by department in the department order; inside a group urgent first,
  then by added time. Bought items still in the cart are a separate «В корзине (N)» block, newest first.
- **R-4 Toggle.** A checkbox changes the status at once (optimistic) with «Я» as the buyer; on failure the row goes
  back and an error says why.
- **R-5 Live list.** The list reloads every 20 s while the page is visible, on window focus and after each write; a
  pending toggle is not overwritten by a reload.
- **R-6 Edit.** Tapping an item opens a dialog: name (1..80), quantity (≤30), note (≤200), department, Срочно,
  Удалить (asks first). Saving sends the version in `If-Match`; a conflict reloads the list.
- **R-7 Cart.** «Убрать купленные» moves the whole cart to history.
- **R-8 Frequent.** ⭐ Частые chips (from the API) add the item with its department in one tap.
- **R-9 «Я».** The device remembers Виталий or Маша (default Виталий); storage failures fall back to the default.
- **R-10 History.** Bought items grouped by day (newest first) with who and when; «Вернуть в список» adds the item
  again; delete asks first; 50 rows a page with «Показать ещё».
- **R-11 Departments.** Listed in store order with keyword and needed counts; add, rename, icon, delete (not the
  fallback; its items and words move to «Другое»), move up / down.
- **R-12 Keywords.** Search and department filter; add, edit, delete; learned ones carry «выучено».
- **R-13 List settings.** Cart hours 1..72, frequent days 30..730, saved with the version.
- **R-14 Home summary.** «Покупки» on the home page: Нужно купить, Срочно, Куплено за 7 дней; departments with
  counts; next items; top frequent; «Открыть список».
- **R-15 States.** Loading, empty («Список пуст 🎉»), error with «Повторить», offline (last data kept with a
  warning) are designed on every page.

## Scenarios

- **SC-1** Given an empty field, When I type «молоко, хлеб» and press Enter, Then one POST with `Idempotency-Key`
  and `addedBy` = «Я» is sent, the field clears and a snackbar «Добавлено: Молоко, Хлеб» with «Отменить» appears.
- **SC-2** Given the snackbar after an add, When I tap «Отменить», Then each added row is deleted with its version
  in `If-Match` and the list reloads.
- **SC-3** Given I type one item, When I stop typing, Then the matched department is shown under the field.
- **SC-4** Given items of several departments, When the list loads, Then groups follow the department order with a
  count, urgent items first and marked «Срочно».
- **SC-5** Given a needed item, When I tick it, Then it moves to «В корзине» at once and PUT status `Bought` by «Я»
  is sent; When the API fails, Then it moves back and an error is shown.
- **SC-6** Given items in the cart, When I tap «Убрать купленные», Then the clear-cart request is sent and the cart
  disappears after reload.
- **SC-7** Given a list item, When I tap its name and save a new quantity, Then PUT with `If-Match` = its version is
  sent.
- **SC-8** Given frequent items, When I tap a chip, Then it is added with that department.
- **SC-9** Given the list is empty, When it loads, Then «Список пуст 🎉» and a hint are shown.
- **SC-10** Given history rows of two days, When История opens, Then they are grouped under day headings with who
  bought them; «Вернуть в список» posts the name; delete asks and then sends DELETE with `If-Match`.
- **SC-11** Given departments, When I tap «Вверх» on the second one, Then the order request lists it first.
- **SC-12** Given keywords, When I type in the search, Then the list reloads with `search`; learned rows show
  «выучено».
- **SC-13** Given list settings, When I enter 100 hours, Then «От 1 до 72» is shown and nothing is sent; a valid
  value is sent with `If-Match`.
- **SC-14** Given «Я: Маша» chosen, When I reload, Then «Маша» stays chosen and is the author of new items.
- **SC-15** Given the home page with `?s=shopping`, When the summary loads, Then the KPI cards, departments, next
  items and «Открыть список» → `/shopping` are shown.
- **SC-16** Given a load failure, When a page loads, Then the reason and «Повторить» are shown; with data already
  shown, a warning keeps the last data.
