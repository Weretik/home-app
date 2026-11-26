# Home expenses summary — rules and scenarios

## Business rules

### R-W4-001 — One summary at a time

The switcher lists Расходы (built), Дела, Покупки, Подарки (скоро). `?s=` keeps the choice; switching keeps the month.

### R-W4-002 — The month

`?year=&month=`, current month by default; arrows change it and keep `?s=`.

### R-W4-003 — Figure cards

Доходы (total, by person), Расходы (total; «лимит N · P%» or «лимит не задан»), Итог месяца (result, after the jar),
Копилка (balance; «P% от цели N» with a bar capped at 100 %, or «цель не задана»).

### R-W4-004 — Limits

Every limit from the API, quarterly ones marked «(квартал)», with spent, limit, left and «в день» while the period
runs; then «ИТОГО»: all expenses against the monthly limit, left, percent, and per day = max(0, round(left / daysLeft,
2)) in the current month; without a monthly limit «ИТОГО» shows only the spent sum. Fill colour: < 80 % success,
80–100 % warning, > 100 % error. Phone: a list; computer: a table.

### R-W4-005 — Who and top categories

«Кто сколько потратил» by payer; «Топ-5 категорий» from the API.

## Acceptance scenarios

### SC-W4-001 — Default summary and coming sections

**Covers:** R-W4-001
When the home page opens → the expenses summary is open and other sections show «скоро»

### SC-W4-002 — Month figures, limits, who spent

**Covers:** R-W4-003, R-W4-004, R-W4-005
Given a summary → the cards, the limits with «ИТОГО» and «Кто сколько потратил» show its figures

### SC-W4-003 — Next month

**Covers:** R-W4-002
When the user presses the next arrow → the next month loads

### SC-W4-004 — Error

**Covers:** R-W4-002
Given an API error → the reason and «Повторить» show

### SC-W4-005 — ИТОГО row

**Covers:** R-W4-004
Given monthly limit 50 000, expenses 30 000, 10 days left → «ИТОГО» left 20 000, 60 %, 2 000 a day
Given no monthly limit, or a past month → no comparison / no per day

### SC-W4-006 — Quarter label and fill

**Covers:** R-W4-004
Given a quarterly limit at 85 % → «(квартал)» and the warning colour
