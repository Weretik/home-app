# Budget forecast and carry-over (web) — scope and outcome

## Goal

As Masha or Vitaliy, I want to plan the month's expenses, mark one-off expenses as outside the limits, and see on the
home page how overspending carries over and what the month is expected to end with, so that the site works like the
«Бюджет» and «Сводка» tabs of the sheet.

## In scope

- Section rename «Расходы» → «Бюджет» (side menu, bottom bar, page titles, home switcher). The «Расходы» list tab keeps
  its name.
- Tab «Прогноз» (`/expenses/forecast`): month picker, list of plans (name, category, payer, amount), total
  «Запланировано», add / edit / delete dialog (название, категория, кто, сумма), «Записать как расход» (date default
  today, payment method, optional comment).
- Expense dialog: checkbox «Не в лимиты» with helper text «Считается в итогах, но не в лимитах и не переносится».
  Expense list: chip «вне лимитов» on such rows.
- Home expenses summary: limit rows show «перенос N» when `carriedIn > 0`, «→ след. мес. N» when `carriesOut > 0`,
  «план N» when `planned > 0`; the KPI area shows «Вне лимитов: N трат на S» only when the count is above 0; a
  «Прогноз» card with ожидаемые расходы, запланировано, ожидаемый итог месяца (and what stays of the monthly limit
  when there is one).
- «Настройки»: the date limits count from (`limitsFrom`) — only if the backend exposes it on the settings form;
  see open question.

## Out of scope

- Computing carry-over, forecast or outside-limits sums in the browser (the API returns them).
- Recurring payments, copying plans between months.

## Participants and external systems

- Masha, Vitaliy on phones first, also on a computer.
- The Expenses API (contract `api/docs/contracts/openapi.json`).

## Constraints

- UI text Russian, code and docs English. Phone first: no horizontal overflow at 360 px; stat lines wrap.
- Every screen follows [design rules](../../../../../design.md): theme tokens, both colour schemes, card patterns.

## Non-functional requirements

| Requirement    | Value                                                                                | How to check                                      |
| -------------- | ------------------------------------------------------------------------------------ | ------------------------------------------------- |
| Data on screen | up to ~30 plans a month, one request, no paging                                      | component test with a list                        |
| Layout         | no horizontal scroll at 360 px on «Прогноз», the expense dialog and the home summary | component test at phone width plus a manual check |
| Double submit  | every mutation's submit disabled while pending                                       | component tests                                   |

## Open product questions

- [NEEDS CLARIFICATION: should «Настройки» get a field for `limitsFrom` («Считать лимиты с»)? The note adds it to the
  API request only. Owner: Vitaliy. Not blocking: the API keeps the stored value when the field is left out.]
- [NEEDS CLARIFICATION: the note says «If-Match on PUT/DELETE» for plans. The current web and backend send no version
  on plan delete (like every other delete in the module). Decide and align both halves. Owner: Vitaliy.]
