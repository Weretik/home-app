# Expenses and incomes pages — rules and scenarios

## Business rules

### R-W1-001 — The month lives in the URL

The open month is `?year=&month=`; without them, or with invalid values, the current month. The arrows change it.
Switching tabs keeps only the month.

### R-W1-002 — The expense period

«Период» shows `?from=&to=` (starting as the open month's first and last day), hides the month picker and loads by
`from`/`to`. Both dates must be set, real days in 2000–2100, «с» not after «по», at most 366 days; otherwise the page
says why and loads nothing. «К месяцу» drops the period. Filters work with it.

### R-W1-003 — Filters live in the URL

«Категория» and «Кто» for expenses, «Кто» for incomes; a filter that leaves nothing offers to reset it.

### R-W1-004 — Paged lists

Rows load 50 at a time, newest first; «Показать ещё» appears while more exist.

### R-W1-005 — Add once

An add dialog makes one `Idempotency-Key` per opened form; a retry with the same data reuses it; changed data or a
"key reused" answer gets a new one. New expense defaults: date today (or the first day of the open month/period when
today is outside it), «Общее», card. New income: today or the 1st, the form's person.

### R-W1-006 — Change with the version, delete after asking

An edit sends the row's `version` in `If-Match`. A delete asks for confirmation first. A 404 or
`expenses.changed_meanwhile` answer shows why and reloads the list.

### R-W1-007 — Field checks before sending

Amount: a number with at most two decimals, greater than 0, at most 10 000 000 (typed as «1 234,5» or «1234.50»).
Date: set, 2000–2100. Comment ≤ 500. Expense needs a category. API field errors show under the fields.

### R-W1-008 — Write errors in Russian

No connection, changed elsewhere, already deleted, key reused, too many changes (429), invalid fields (400) each have
their own message.

## Acceptance scenarios

### SC-W1-001 — The month's expenses with Russian labels

**Covers:** R-W1-001, R-W1-004
Given expenses in the current month
When «Расходы» opens without `?year=&month=`
Then the current month's rows show with Russian labels and no month total card

### SC-W1-002 — Previous month kept across tabs

**Covers:** R-W1-001
When the user moves to the previous month and opens «Доходы»
Then «Доходы» shows that month

### SC-W1-003 — Filters

**Covers:** R-W1-003
When the user filters by who paid and by category
Then the request carries them and the URL keeps them
When the filter leaves nothing
Then the page offers to reset it

### SC-W1-004 — Empty, error, more

**Covers:** R-W1-004
Given no expenses → the page says so; given an API error → it shows it with «Повторить»; given more than 50 → «Показать ещё» loads the next page

### SC-W1-005 — Add with a key kept on retry

**Covers:** R-W1-005
When the user adds an expense, the request fails and the user saves again with the same data
Then both requests carry the same `Idempotency-Key`

### SC-W1-006 — Edit with If-Match, stale edit

**Covers:** R-W1-006
When the user changes an expense
Then `If-Match` carries its version
When the API answers `expenses.changed_meanwhile`
Then the dialog says it was changed elsewhere and the list reloads

### SC-W1-007 — Delete after confirmation

**Covers:** R-W1-006
When the user picks «Удалить»
Then nothing is sent until the confirmation

### SC-W1-008 — Period

**Covers:** R-W1-002
When the user switches to «Период» and back
Then the URL gains and loses `?from=&to=`
Given a URL with a period and filters → both apply
Given «с» after «по» or more than 366 days → the reason shows and nothing loads
When adding an expense in a period → the date starts inside the period

### SC-W1-009 — Incomes

**Covers:** R-W1-001, R-W1-003, R-W1-004
Given incomes → each shows without a total card; the «Кто» filter works; empty and error states show

## Examples

| Typed amount | Result                                                       |
| ------------ | ------------------------------------------------------------ |
| «1 234,5»    | 1234.5                                                       |
| «1234.505»   | message «Сумма — число, не больше двух знаков после запятой» |
| «0»          | «Сумма больше нуля»                                          |
