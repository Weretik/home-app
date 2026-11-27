# Scope, rules and scenarios

## Outcome

On the settings page a person changes a limit and picks from which month the new amount applies (the current month
unless they choose otherwise); earlier months keep their own limit. The page shows the amount that was in force before.

## Rules

- **R-WLA-001** The limit dialog, when it edits an existing limit, has a month field «Діє з» (native month input),
  filled with the current month in Europe/Kyiv. It is sent as `amountFrom` = the first day of that month, but only
  when the amount differs from the limit's current `amount` or the person moved the month off the default: the API
  treats an explicit `amountFrom` as "replace every step from that period on with this amount" (api R-LA-002, R-LA-005), so a
  rename or a category change with the amount and month untouched sends no `amountFrom` and keeps a change saved
  ahead. A new limit has no such field and sends no `amountFrom`.
- **R-WLA-002** For a quarterly limit the field's hint says the amount applies from the start of the quarter that holds
  the chosen month (the server moves it); for a monthly limit the hint says earlier months keep their limit.
- **R-WLA-003** The settings card has the same month field, labelled «Ліміт діє з», beside «Загальний ліміт витрат на
  місяць», sent as `monthlyLimitFrom` under the same condition: only when the monthly limit differs from the budget's
  `monthlyLimit` or the month was moved off the default; a save that changes only «План скарбнички» sends no
  `monthlyLimitFrom`. The decision lives in the pure request mappers (`toLimitRequest`, `toBudgetSettingsRequest`).
- **R-WLA-004** A limit row shows the current amount (`amount`) as before; when the limit has more than one step, a
  caption under it names the step in force today and the amount before it in the page's amount format, for example
  «з грудня 2026 · раніше 7 000,00 ₴». The settings card shows the same line as the monthly limit field's helper text
  when `monthlyLimits` has a step before today's, merged with what an empty field means: «з грудня 2026 · раніше
  45 000,00 ₴; порожньо — без ліміту» (without earlier or scheduled steps: «Порожньо — без ліміту»). While the field
  shows a validation error, the error replaces this helper text.
- **R-WLA-005** An empty or malformed month blocks saving with «Виберіть місяць». The field carries the placeholder
  «РРРР-ММ», shown only by browsers without a native month picker (desktop Firefox and Safari), so the format is
  visible where the month is typed as text.
- **R-WLA-006** Texts are Ukrainian; no carry or limit arithmetic in the browser.
- **R-WLA-007** A change saved ahead is shown: when a step starts after the current month in Kyiv, a caption names the
  next one, «з квітня 2027 — 6 000,00 ₴», under the limit row's amount (after the earlier-amount caption, if any) and in
  the monthly limit's helper text before «порожньо — без ліміту».

## Scenarios

- **SC-WLA-001** Editing «Їжа» (7 000) to 5 000 with the default month sends `amount: 5000, amountFrom: "<current
month>-01"` with `If-Match`.
- **SC-WLA-002** Saving the settings with the month changed to 2027-01 sends `monthlyLimitFrom: "2027-01-01"`.
- **SC-WLA-003** A limit with steps 7 000 (2000-01-01) and 5 000 (2026-12-01) and `amount` 5 000 shows «5 000,00 ₴»
  and «з грудня 2026 · раніше 7 000,00 ₴»; a limit with one step shows no caption.
- **SC-WLA-004** The «Новий ліміт» dialog has no «Діє з» field and its request has no `amountFrom`.
- **SC-WLA-005** Editing «Їжа» and changing only its name sends no `amountFrom`; saving the settings with only «План
  скарбнички» changed sends no `monthlyLimitFrom`. Changing the amount, or keeping it and moving the month, still sends
  the month.
- **SC-WLA-006** A limit with steps 3 000 (2026-10-01) and 6 000 (2027-04-01), viewed in December 2026, shows
  «3 000,00 ₴» and «з квітня 2027 — 6 000,00 ₴» and no «раніше»; a monthly limit with a step from 2027-04 shows the
  same caption in its helper text.
