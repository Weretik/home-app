# Budget forecast and carry-over (web) — rules and scenarios

## Business rules

### R-W5-001 — The section is called «Бюджет»

The section's name in the side menu, the bottom bar, the section title and the home summary switcher is «Бюджет». The
first tab stays «Расходы».

### R-W5-002 — Plans are kept per month

The «Прогноз» tab shows the plans of the open month, sorted as the API returns them (by name), and their total
«Запланировано». The month follows the section's `?year=&month=` like the other tabs.

### R-W5-003 — Plans are edited like entries

A plan is added with one `Idempotency-Key` per opened form (kept while the same data is retried, renewed after a
change or a "key reused" answer), changed with its version in `If-Match`, deleted after a confirmation. Name 1–100
characters, a category, a payer and a positive amount are checked before sending.

### R-W5-004 — A plan becomes an expense

«Записать как расход» asks for the date (today by default), the payment method (card by default) and an optional
comment, sends the plan's version, and on success the plan leaves «Прогноз» and the expense appears in «Расходы» and in
the home summary. A 404 or «changed meanwhile» answer reloads the lists and says why.

### R-W5-005 — «Не в лимиты»

The expense dialog has the checkbox with the helper text; it starts unchecked for a new expense and shows the stored
value for an edit. A row whose expense is outside limits shows the chip «вне лимитов».

### R-W5-006 — Carry-over, plans and outside limits on the home summary

The summary shows the API's figures: «перенос N» when a limit's `carriedIn > 0`, «→ след. мес. N» when
`carriesOut > 0`, «план N» when `planned > 0`; the same for «ИТОГО» with the month's `carriedIn`/`carriesOut`. «Вне
лимитов: N трат на S» appears only when `outsideLimitsCount > 0`, with Russian plural forms.

### R-W5-007 — The forecast card

The «Прогноз» card shows «Ожидаемые расходы» (`expectedExpenses`), «Запланировано» (`planned`), «Ожидаемый итог
месяца» (`expectedResult`, success colour when ≥ 0, error when < 0) and, when `monthlyLimit > 0`, «Останется от лимита»
(`expectedTotalLeft`, error colour when < 0).

## Acceptance scenarios

### SC-W5-001 — The section is «Бюджет»

**Covers:** R-W5-001

Given the app on a phone and on a computer
When the user opens any page
Then the menu shows «Бюджет», the section page title is «Бюджет», the home switcher shows «Бюджет», and the first tab
is «Расходы»

### SC-W5-002 — Plans of a month

**Covers:** R-W5-002

Given two plans in December 2026, «Абонемент» 2 000 and «Подарки маме» 3 000
When the user opens «Прогноз» for December
Then both rows show name, category, who and amount, and «Запланировано» shows 5 000 ₴
When the month has no plans
Then an empty state offers to add one

### SC-W5-003 — Add a plan once

**Covers:** R-W5-003

Given the add dialog
When the user fills название, категория, кто, сумма and saves, and the network fails, and saves again with the same data
Then both requests carry the same `Idempotency-Key`, the submit is disabled while pending, and on success the list
shows the plan
When required fields are empty or the amount has three decimals
Then the dialog shows the field messages and sends nothing

### SC-W5-004 — Edit and delete a plan

**Covers:** R-W5-003

Given a listed plan
When the user changes its amount
Then the request carries `If-Match` with the plan's version and the list shows the new amount
When someone else changed it meanwhile
Then the dialog says the entry changed elsewhere and the list reloads
When the user deletes it
Then a confirmation is asked first, and after it the plan disappears

### SC-W5-005 — Record a plan as an expense

**Covers:** R-W5-004

Given the plan «Подарки маме» 3 000
When the user picks «Записать как расход», keeps today and «Карта», and confirms
Then the request is `POST /api/expenses/planned/{id}/record` with `If-Match`, the plan disappears from «Прогноз», and
«Расходы» of that date's month shows a 3 000 expense in «Подарки»
When the plan was already recorded or deleted elsewhere
Then the dialog says so and the plans reload

### SC-W5-006 — Mark an expense outside limits

**Covers:** R-W5-005

Given the add expense dialog
When the user ticks «Не в лимиты» and saves
Then the request has `outsideLimits: true`, and the row shows «вне лимитов»
When the user edits that expense
Then the checkbox is ticked

### SC-W5-007 — Carry-over on the home summary

**Covers:** R-W5-006

Given a summary where «Личные Виталий» has `carriedIn` 1 000, `carriesOut` 0, `planned` 0 and «ИТОГО» has `carriesOut` 2 000
When the home page shows the month
Then «Личные Виталий» shows «перенос 1 000» and no «→ след. мес.», and «ИТОГО» shows «→ след. мес. 2 000»
Given a limit with all three at 0
Then it shows none of these notes

### SC-W5-008 — Outside-limits line

**Covers:** R-W5-006

Given `outsideLimitsCount` 2 and `outsideLimitsAmount` 3 500
Then the KPI area shows «Вне лимитов: 2 траты на 3 500 ₴»
Given the count is 0
Then the line is absent

### SC-W5-009 — Forecast card

**Covers:** R-W5-007

Given `forecast` 35 000 / 5 000 / 35 000 / 15 000 and `monthlyLimit` 50 000
Then the card shows «Ожидаемые расходы 35 000 ₴», «Запланировано 5 000 ₴», «Ожидаемый итог месяца 35 000 ₴» in the success
colour and «Останется от лимита 15 000 ₴»
Given `monthlyLimit` 0
Then «Останется от лимита» is absent

### SC-W5-010 — Phone layout

**Covers:** R-W5-002, R-W5-006, R-W5-007

Given a 360 px wide screen
When «Прогноз», the expense dialog and the home summary are shown with long names and the notes above
Then nothing scrolls horizontally and the stat lines wrap

## Examples

| State                                              | Observable result                 |
| -------------------------------------------------- | --------------------------------- |
| `carriedIn` 1 000, `carriesOut` 0, `planned` 3 000 | «перенос 1 000 · план 3 000»      |
| `carriedIn` 0, `carriesOut` 500                    | «→ след. мес. 500»                |
| `outsideLimitsCount` 1 / 1 000                     | «Вне лимитов: 1 трата на 1 000 ₴» |
| `outsideLimitsCount` 5                             | «… 5 трат …»                      |

## Deferred scenarios

- Editing `limitsFrom` on «Настройки» (open question).
