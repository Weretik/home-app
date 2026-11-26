# Web architecture

```mermaid
flowchart LR
    contract["api/docs/contracts/openapi.json"] -->|openapi-typescript| client["src/api-client<br/>generated types + openapi-fetch"]
    client --> platform["src/platform/api.ts<br/>the one client instance"]
    platform --> features["src/features/*/*-api.ts<br/>React Query hooks"]
    features --> screens["Pages and components"]
    platform -->|"HTTPS, CORS"| api["API"]
```

One React single-page app built with Vite. It reaches the API only through the typed client generated from the
backend contract. Every feature is specified before it is built: the [feature specifications (SDD)](specs/README.md)
hold the scenarios, the design and the tasks of each one.

## Folders

```
src/
  main.tsx        entry: router and providers
  app/
    root/         routes and providers (theme, React Query)
    theme/        palette, theme, light/dark/system switch (docs/design.md)
    shell/        header, side menu, bottom bar, the list of sections
  api-client/     generated/ types (never edited), client/ createApiClient, apiCall, ApiError, newIdempotencyKey
  platform/       the API client instance (VITE_API_URL), the query client and each backend module's query key
                  prefix (query-keys.ts)
  features/
    home/         Главная: the section summaries and their switcher
    status/       API and database status page
    expenses/     the «Бюджет» section, the Expenses module (below)
    shopping/     the «Покупки» section, the Shopping module (below)
    gifts/        the «Подарки» section, the Gifts module (below)
    todos/        the «Дела» section, the Todos module (below)
```

A feature folder holds its page components, its `<area>-api.ts` with React Query hooks, and tests next to the code.
When a feature grows past six files it splits by meaning (`list/`, `editor/`, `data/`). A component stays within 150
lines and any other module within 200 ([rules](rules.md#structure)); `npm run architecture:structure` checks the
folders, the file rules and the import boundaries in CI ([rules and their checks](rules.md#rules-and-their-checks)).

## Routes

| Path                 | Page                                                                                                    | Feature                  |
| -------------------- | ------------------------------------------------------------------------------------------------------- | ------------------------ |
| `/`                  | Главная, the dashboard                                                                                  | `home`                   |
| `/expenses`          | Бюджет › Расходы: the month's (or a period's) expenses, category and «Кто» filters, add / edit / delete | `expenses/expense-list`  |
| `/expenses/incomes`  | Доходы: the month's incomes, «Кто» filter, add / edit / delete                                          | `expenses/income-list`   |
| `/expenses/savings`  | Копилка: the balance now and the goal ring, the month's movements with add / edit / delete, the months  | `expenses/savings-page`  |
| `/expenses/forecast` | Прогноз: the month's planned expenses and «Запланировано», add / edit / delete, «Записать как расход»   | `expenses/forecast-page` |
| `/expenses/settings` | Настройки (the gear next to the tabs): monthly limit, savings goal, spending limits                     | `expenses/settings-page` |
| `/shopping`          | Покупки, Список: quick add, ⭐ Частые, groups by department, ticks, «В корзине»                         | `shopping/list-page`     |
| `/shopping/history`  | История: bought items by day, «Вернуть в список», delete                                                | `shopping/history-page`  |
| `/shopping/settings` | Настройки (the gear): departments and their order, keywords, cart hours and the frequent window         | `shopping/settings-page` |
| `/gifts`             | Список: wanted gifts grouped by person, recipient and category filters, add / edit / delete, «Подарено» | `gifts/pages`            |
| `/gifts/archive`     | Подаренные: gifted and dropped gifts, «Вернуть в список»                                                | `gifts/pages`            |
| `/gifts/settings`    | Настройки (the gear next to the tabs): «Нюансы» and categories                                          | `gifts/settings-page`    |
| `/todos`             | Задачи: active tasks in due groups, «Кому» / category / search filters, quick add, done with undo       | `todos/task-list`        |
| `/todos/archive`     | Архив: done and cancelled tasks, «Вернуть», «Показать ещё»                                              | `todos/archive`          |
| `/todos/settings`    | Настройки (the gear next to the tabs): categories with emoji and active count                           | `todos/settings`         |
| `/status`            | Состояние: API and database status                                                                      | `status`                 |

Unknown paths redirect to `/`. Routes are declared in `src/app/root/App.tsx`.

## Home (`src/features/home`)

The home page shows one section's summary at a time, picked with a switcher
([ADR 0008](decisions/0008-home-page-as-a-registry-of-section-summaries.md)).

```
home/
  page/              HomePage (title, switcher, the open summary, the «Состояние» link for phones), the summary
                     registry (summary-sections.ts), ?s= in the URL
  shopping-summary/
    data/            shopping-summary-api.ts (GET /api/shopping/summary, key ['shopping', 'summary']), its labels
    overview/        ShoppingSummary («Открыть список», states), SummaryContent (which cards show), «Список пуст 🎉»
    cards/           Нужно купить / Срочно / Куплено за 7 дней (SummaryKpis, FigureCard), «По отделам», «Купить в
                     первую очередь», «Покупаем чаще всего»
  expenses-summary/
    data/            summary-api.ts (GET /api/expenses/summary), the month and ?year=&month=, amounts and the goal
                     percent (money.ts), labels, figure-lines.ts (the empty month, the «Вне лимитов» line, the
                     «Прогноз» lines)
    overview/        ExpensesSummary (month picker, «Открыть расходы», loading and error), SummaryContent (which
                     cards show), the empty month, the month picker
    figures/         the four figure cards (KpiCards, one KpiCard), «Прогноз», «Кто сколько потратил» and «Топ-5
                     категорий» (SpendingCards, AmountsCard), and card-lines.ts (their captions and lines)
    limits/          «Лимиты»: the card, LimitTable (computer), LimitList (phone), the carry-over and plan notes
                     under a row, the fill bar, the rows with «ИТОГО»
  gifts-summary/
    data/            summary-api.ts (GET /api/gifts/summary, key ['gifts', 'summary']), recipient labels, sums and dates
    overview/        GiftsSummary (states, «Открыть подарки»), SummaryContent (which cards show), «Желаний пока нет»
    cards/           a figure card per person and «Всего», «🔥 Очень хочу», «Ближайшие поводы», «Нюансы» read-only
                     with «Изменить» → /gifts/settings, and the titled card they share
  todos-summary/
    data/            todos-summary-api.ts (GET /api/todos/summary, the in-place done tick), labels and due dates
    view/            TodosSummary (states, «Сделано за неделю», «Открыть дела»), the three assignee cards, «В фокусе»
```

- `summary-sections.ts` lists every section with its summary component, or `null` while it has none: the switcher
  shows those disabled, marked «скоро». A new summary is its own folder here plus one line in the list. A missing or
  unknown `?s=` opens the first built summary.
- «Бюджет» is the old sheet's «Сводка»: Доходы (by person), Расходы (against the monthly limit, and «Вне лимитов: N
  трат на S» only when the month has expenses marked «Не в лимиты»), Итог месяца (after the jar), Копилка (against
  the goal); the limits, quarterly ones marked «(квартал)», with «ИТОГО» = the expenses that count against the limits
  plus the overspend carried in, against the monthly limit, and what may still be spent per day; under a row
  «перенос N» (carried in), «→ след. мес. N» (moves on) and «план N» (planned), each only when it is not zero;
  «Прогноз» (expected expenses, planned, the expected result, what stays of the monthly limit, «Открыть план»); who
  spent what; the top five categories. Its query key is `['expenses', 'summary', month]`, under the Expenses prefix
  (`moduleKeys.expenses` in `src/platform/query-keys.ts`), so every write on the Бюджет pages refreshes it
  ([ADR 0009](decisions/0009-writes-refresh-their-backend-module.md)). «Открыть расходы» opens the same month. A month
  with no entries and no plans shows the empty state.
- The summary keeps its own labels and formatters: features never import each other. Rules both features show keep
  one definition per side with the same test cases: the savings goal percent is rounded down on both «Копилка» cards
  (`goalPercentOf` here, `goalPercent` in `expenses/savings-goal/goal-progress.ts`), so 100 % means the goal is reached;
  the month helpers and the month picker are copies of the `expenses` ones.
- «Покупки» shows the list at a glance: three figure cards, the departments with needed items, up to eight next items
  (urgent first), the five most bought, and «Открыть список». An empty list says «Список пуст 🎉».
- «Дела» (`?s=todos`): a card per Маша, Виталий, Общее with the active count, «просрочено N» in red and «сегодня N»;
  «Сделано за неделю»; «В фокусе», up to eight tasks from the summary's `focus`, each ticked done in place (struck
  through while saving). Its query key is `['todos', 'summary']`, so every Todos write refreshes it.

## Expenses (`src/features/expenses`)

The «Бюджет» section (named «Расходы» before; the URLs stay `/expenses`): pages of the old expenses Google Sheet, as
tabs of one section ([ADR 0006](decisions/0006-section-pages-as-tabs-with-state-in-the-url.md)).

```
expenses/
  data/             expenses-api.ts and savings-api.ts (React Query reads), writes-api.ts (save / delete hooks of
                    expenses, incomes and savings movements), formatting (signed amounts too), Russian enum labels,
                    months
  write-data/       what every write shares: write-hooks-api.ts (the Writer shape, useSave / useChange / useRemove,
                    If-Match, the Idempotency-Key hook, the module refresh) and write-errors-api.ts (Russian text of
                    a failed write)
  section/          ExpensesSection (title, month picker, tabs and the «Настройки» gear, <Outlet />), the list of pages
  url-state/        the month (?year=&month=), the Расходы period (?from=&to=, its checks) and filters
                    (?category=&payer=, ?person=) in the URL
  month-view/       parts every month page uses: the toolbar row with the add button, the paged list card
                    (loading/empty/error, the rows, «Показать ещё»), the filter select
  entry-editing/    form logic shared by every entry: field checks, default date, the open-dialog state, useEntryForm
  entry-dialogs/    the add / edit dialog frame, the delete confirmation, the row «⋮» menu (with an optional step of
                    its own), amount, person and category fields
  expense-list/     «Расходы» page: which expenses it shows (use-shown-expenses.ts: month or period, filters, from the
                    URL), phone list, computer table, the «вне лимитов» chip
  expense-filters/  the row above the expenses: category and «Кто» filters, «Период», «Добавить расход»
  expense-editing/  the expense form (values ↔ request, «Не в лимиты») and its dialogs
  income-list/      «Доходы» page: filter, «Добавить доход», phone list, computer table
  income-editing/   the income form and its dialogs
  savings-page/     «Копилка» page: balance card, «По месяцам» and its rows, signed amounts
  savings-movements/ «Движения за месяц»: the open month's movements (phone list, computer table) with add / edit /
                    delete
  savings-editing/  the savings form (direction + positive amount → signed request) and its dialogs
  savings-goal/     «План копилки» card on Копилка: percent ring, now / goal / left, or a link to set a goal
  budget-data/      budget-api.ts (the budget read) and budget-writes-api.ts (settings and spending limit writes)
  settings-page/    «Настройки» page: the monthly limit and savings goal form, the spending limits list
  limit-editing/    the spending limit form (name, month / quarter, amount, categories) and its dialogs
  planned-data/     planned-api.ts (the month's plans, «Запланировано») and planned-writes-api.ts (plan writes and
                    «Записать как расход»)
  forecast-page/    «Прогноз» page: «Запланировано» (PlannedTotal), «Добавить план», phone list, computer table
  planned-editing/  the plan form (name, category, who, amount), the record form (date, payment, comment) and their
                    dialogs
```

- The month defaults to the current one; the arrows change `?year=&month=`, so a link opens the same month.
- «Расходы» can show any days instead of the month: «Период» adds `?from=&to=` (the open month's first and last day to
  start), hides the month picker and sends `from`/`to` instead of `year`/`month`; the filters work with it and «К
  месяцу» drops it. Both dates set, «с» not after «по» and at most 366 days are checked before anything loads.
  Switching tabs keeps only the month, so Доходы and Копилка stay month-based.
- Lists load 50 rows a page with `useInfiniteQuery` and «Показать ещё». The Расходы and Доходы pages show no month
  total; an overall summary will be a page of its own.
- Expenses, incomes and savings movements are added, changed and deleted in dialogs
  ([ADR 0007](decisions/0007-editing-in-dialogs-with-idempotency-and-versions.md)): a create sends an
  `Idempotency-Key` (one per opened form, kept while the same data is retried), an edit sends the row's `version` in
  `If-Match`, and a delete asks first. A success invalidates every query of the Expenses module
  (`moduleKeys.expenses`, `['expenses']`): the lists, the budget, the plans and the home summary
  ([ADR 0009](decisions/0009-writes-refresh-their-backend-module.md)); a 404 or `expenses.changed_meanwhile` reloads them
  too. Only the queries on screen refetch; the rest refetch when they are shown again.
- «Настройки» (`/expenses/settings`, no month picker, not a tab) reads `/api/expenses/budget`. The monthly limit and
  the savings goal are one form saved with the budget's `version` in `If-Match` (empty = 0 = none); spending limits
  are added (`Idempotency-Key`), changed (`If-Match`) and deleted in dialogs like entries. Like every write, a budget
  write invalidates the module's queries, so the home summary's limits and goal follow. The settings are changed
  through `useChange` (a write with no create or delete), bound to the budget's version.
- An expense marked «Не в лимиты» counts in the month's totals but in no limit and no carry-over; the lists mark it
  with a small «вне лимитов» chip.
- «Прогноз» (`/expenses/forecast`) lists the open month's planned expenses (`/api/expenses/planned?year=&month=`) with
  their total «Запланировано». Plans are added (`Idempotency-Key`), changed (`If-Match`) and deleted in dialogs like
  entries and refresh the module's queries. «Записать как расход» in a plan's «⋮» menu asks for the date
  (today), the payment and a comment and posts `/api/expenses/planned/{id}/record` with the plan's version in
  `If-Match`: the plan becomes an expense and is gone; like every write it refreshes the module's queries.
- «Копилка» also shows «План копилки»: the balance today against the budget's savings goal.
- «Копилка» shows the jar's balance today (`/api/expenses/savings/months`), the open month's movements with its
  start / in / out / end figures (`/api/expenses/savings`), and every month; a month in that list opens it.
- Below `md` a list is a compact MUI `List`; from `md` it is a `Table` (`useMediaQuery`).
- A new page (Обзор, Лимиты, Платежи, По месяцам, Категории) is an entry in
  `section/expenses-pages.ts`, a nested route in `App.tsx` and its own folder.

## Shopping (`src/features/shopping`)

The shared shopping list of the old «🛒 Список покупок» sheet and its bot, as tabs Список and История plus the
«Настройки» gear ([ADR 0006](decisions/0006-section-pages-as-tabs-with-state-in-the-url.md)); live for two people
([ADR 0012](decisions/0012-live-shared-list-with-optimistic-ticks-and-polling.md)). Specification:
[docs/specs/shopping/001-shopping-list](specs/shopping/001-shopping-list/README.md).

```
shopping/
  data/                 shopping-api.ts (the list with 20 s polling, ⭐ Частые, department match), item-writes-api.ts
                        (add with Idempotency-Key, edit / delete with If-Match, clear cart, undo, the shared write hook),
                        status-api.ts (the optimistic tick), item-status.ts, write-failure-api.ts (Russian text), labels
  section/              ShoppingSection (title with counts, «Я», tabs, gear, <Outlet />), the page list, use-me.ts
  quick-add/            the sticky field with «Срочно», department preview, ⭐ chips, the undo snackbar, use-quick-add
  list-page/            «Список»: department cards in store order, item rows, «В корзине», the grouping
  item-editing/         the item dialog (name, quantity, note, department, Срочно, Удалить) and its form
  page-parts/           form and confirm dialogs, the «⋮» menu, loading / error / offline / empty states
  history-page/         «История»: days, «Вернуть в список», delete, «Показать ещё»
  settings-page/        «Настройки»: the page and the «Список» card (cart hours, frequent days)
  department-settings/  «Отделы»: order with up / down, add / rename / delete
  keyword-settings/     «Слова для отделов»: search, department filter, add / edit / delete, «выучено»
```

- Query keys start with the Shopping prefix (`moduleKeys.shopping`, `['shopping']`); every write invalidates that
  prefix (list, frequent, history, departments, the home summary), as [ADR 0009](decisions/0009-writes-refresh-their-backend-module.md) asks.
- Needed items are grouped by department in its sort order, urgent first, then oldest added; the cart is newest
  bought first. Moving an item to another department in the dialog teaches the dictionary on the server.
- «Я» (Виталий / Маша) is per device and is the author of adds and ticks.

## Gifts (`src/features/gifts`)

The old «🎁 Список подарков» sheet as a section with tabs ([ADR 0006](decisions/0006-section-pages-as-tabs-with-state-in-the-url.md)):
«Список», «Подаренные», and «Настройки» behind the gear. Specification:
[specs/gifts/001-gifts-section](specs/gifts/001-gifts-section/README.md).

```mermaid
flowchart LR
    pages["pages/ GiftsPage, ArchivePage"] --> list["list-view/ useGiftList, filters, groups, states"]
    list --> url["url-state/ ?recipient=&category="]
    list --> data["data/ gifts-api (reads), *-writes-api, write-errors-api"]
    pages --> card["gift-card/"] & giftEdit["gift-editing/ + variant-editing/"]
    settings["settings-page/"] --> detail["detail-editing/"] & category["category-editing/"]
    giftEdit & detail & category --> editing["editing/ dialogs, useEditForm"] --> data
    data --> platform["src/platform/api.ts"]
```

```
gifts/
  data/              gifts-api.ts (types from the contract, reads, query keys), writes-api.ts (Writer, useSave/useRemove,
                     useIdempotencyKey), gift-writes-api.ts (gift and status), settings-writes-api.ts (categories,
                     «нюансы»), write-errors-api.ts (Russian text of a failed write, isStale)
  display/           Russian labels of recipients, priorities and statuses; price, date, variant and plural formatting
  url-state/         ?recipient=&category= (unknown values ignored), filtering and grouping in memory
  section/           GiftsSection (title, tabs, gear, <Outlet />), the list of pages
  editing/           EditDialog, DeleteDialog, RowActions («⋮» with extra items), useEditForm, useEditorState, field checks
  gift-card/         GiftCard: name and price, chips, notes, variants (VariantLinks), CardActions (link, status, «⋮»)
  list-view/         useGiftList (the controller of a tab), GiftFilters, GiftGroups, ListStatus, StatusSnackbar
  pages/             GiftsPage («Список»), ArchivePage («Подаренные»)
  gift-editing/      GiftEditor, GiftDialog (GiftFields, OccasionFields), gift-form.ts (values ↔ GiftRequest, checks)
  variant-editing/   VariantsEditor (up to 20 rows), VariantRow, PriceField
  settings-page/     GiftSettingsPage, SettingsCard, DetailsCard («Нюансы» by person), CategoriesCard
  detail-editing/    the «нюанс» form and its dialogs
  category-editing/  the category form and its dialogs
```

- The API filters only by `archived`; the person and category filters run in memory over the tab's one list (a few
  hundred gifts at most), so a filter change sends no request. Without a person filter «Список» groups by person.
- Writes follow [ADR 0007](decisions/0007-editing-in-dialogs-with-idempotency-and-versions.md): creates send an
  `Idempotency-Key`, gift, status and «нюанс» edits send the `version` in `If-Match` (categories have none), a 409
  `gifts.changed_meanwhile`, a 412 or a 404 reload the list. Every Gifts write (gift, status, category, «нюанс»)
  invalidates the whole Gifts prefix (`moduleKeys.gifts`, `['gifts']`): both tabs, the settings and the home summary
  ([ADR 0009](decisions/0009-writes-refresh-their-backend-module.md)).
- «Подарено» sends `{ status: 'Gifted', giftedOn: null }` (the server takes today in Kyiv); «Не актуально» and
  «Вернуть в список» send `Dropped` and `Wanted`. The edit dialog never changes the status.
- The editing frame is a copy of the Expenses one adapted to the `gifts.*` codes, because features never import each
  other.

### Tests

`cd web && npx vitest run src/features/gifts src/features/home/gifts-summary`: unit tests of `display/formatting`,
`url-state/gift-filters`, `editing/field-checks`, `data/write-errors-api`, `gift-editing/gift-form`,
`detail-editing/detail-form`, `category-editing/category-form`; component tests of `gift-card/GiftCard`,
`editing/EditDialog` (and `DeleteDialog`), `section/GiftsSection`; page tests of `pages/GiftsPage`,
`pages/ArchivePage`, `settings-page/GiftSettingsPage` and `home/gifts-summary/overview/GiftsSummary` with a stubbed
`fetch`.

## Todos (`src/features/todos`)

The family's to-do list «Дела» ([ADR 0011](decisions/0011-todos-grouped-list-quick-add-optimistic-done.md),
[SDD](specs/todos/001-todos-section/README.md)).

```
todos/
  data/          todos-api.ts (task list infinite query, one task), categories-api.ts, labels.ts (Russian enums,
                 priority marks), due-date.ts (Kyiv today, due groups, «сегодня / завтра / пн 30.11»)
  writes/        task-writes-api.ts (create / edit / delete, Idempotency-Key), status-api.ts (optimistic move between
                 lists, undo), steps-api.ts, category-writes-api.ts, failures-api.ts (Russian text of a failed write)
  section/       TodosSection (title, tabs «Задачи» / «Архив», the gear), the list of pages
  url-state/     the filters (?who=&category=&q=) and their mapping to the API query, the debounced search
  task-filters/  «Кому» chips, the category select, the search field
  task-list/     «Задачи» page: quick add, the due groups, the done tick with «Отменить»
  task-row/      a task row: round tick, title, the wrapping meta line (priority, date, category, steps, notes,
                 assignee, status)
  task-dialog/   the task dialog (full screen on a phone), its fields, the due date with quick chips, task-form.ts
  task-steps/    «Шаги»: the checklist editor, saved at once for an existing task, kept in the form for a new one
  feedback/      loading / empty / error states, «Показать ещё», the result snackbar, the delete confirmation
  archive/       «Архив» page and its row with «Вернуть»
  settings/      «Настройки»: categories list, add / change dialog, delete with the count of tasks that lose it
```

- Query keys: `['todos', 'tasks', view, query]`, `['todos', 'task', id]`, `['todos', 'categories']`; the home
  summary's `['todos', 'summary']`. Every write invalidates `['todos']`.
- Groups (FR-001): «Просрочено», «Сегодня», «Скоро» (next 7 days), «Позже», «Без срока»; empty ones hidden; today is
  the Kyiv date. Lists load 50 tasks a page with «Показать ещё».
- Filters: «Маша» and «Виталий» send `assignee` twice (the person and `Shared`), «Общее» only `Shared`; the category is
  an id or `none` (`withoutCategory=true`); the search reaches the URL 300 ms after typing stops. Switching tabs keeps
  them; the gear drops them.
- Quick add creates a task for the chosen «Кому» («Общее» for «Все»), Обычно, no date, with an `Idempotency-Key`
  renewed after each success; the field clears and keeps focus. The pencil opens the full form with the typed title.
- The done tick and «Вернуть» are optimistic: the row leaves its list at once and comes back with the reason if the
  write fails. «Отменить» waits for the tick to be saved, reads the task again (the tick changed its version) and
  restores its previous status.
- The task dialog edits title, «Кому», «Поставил(а)», status, priority, due date («Сегодня», «Завтра», «На выходных»,
  «Без срока»), category, notes and steps. Edits send the version of the task as the dialog's query holds it now, so
  steps saved meanwhile do not cause a conflict; a 409 reloads the task into the form, a 404 says it is gone.

## API client (`src/api-client`)

- `generated/api.ts`: types generated from the backend contract. Never edited by hand.
- `createApiClient({ baseUrl, fetch })`: an `openapi-fetch` client that sends cookies.
- `apiCall(request)`: unwraps a response or throws `ApiError` with the HTTP status, the problem `code` and field
  errors keyed by camelCase field name, ready for a form.
- `newIdempotencyKey()`: a key for requests that must not run twice.
- `Schema<'SystemStatus'>`: a named schema type, so components can type their props from the contract.

## App

- Server state lives in React Query. Every read of a backend module starts its query key with the module's prefix
  (`src/platform/query-keys.ts`), and every write invalidates that whole prefix, so a page or summary of another
  feature that shows the same module's data refreshes too ([ADR 0009](decisions/0009-writes-refresh-their-backend-module.md)).
  Failures with a 4xx status are not retried.
- `index.html` has `noindex` and `robots.txt` disallows everything.
- `public/staticwebapp.config.json` sends unknown paths to `index.html` on Azure Static Web Apps; other static hosts
  ignore it.
- UI text is Russian; screens are designed for a phone first. Look, theme and layout: [design rules](design.md).
