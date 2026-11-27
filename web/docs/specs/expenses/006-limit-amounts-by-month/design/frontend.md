# Frontend design

- Data: no new queries. `useBudget()` returns the new fields; the existing writes (`useSaveSpendingLimit`,
  `useSaveBudgetSettings`) send them and still refresh the module prefix (web ADR 0009), so the home summary and the
  budget page pick up the new amounts.
- Forms: the month is kept as `YYYY-MM` text in the form values (what `<input type="month">` gives) and turned into
  `YYYY-MM-01` in the `to*Request` functions. Those mappers also decide whether the month is sent at all: only when the
  amount differs from the one in force or the month differs from the form's default (R-WLA-001, R-WLA-003); the dialog
  and the card keep the form's starting values for that comparison. The default comes from a tested helper that takes "now" and answers the
  Kyiv month, so tests are deterministic.
- «The step in force today and the one before» is picked by a pure tested helper over `amounts` / `monthlyLimits`
  (the last step whose `from` is not after the current month, and the one before it); `nextStep` picks the first step
  after the current month for the «з квітня 2027 — 6 000,00 ₴» caption (R-WLA-007); the label uses the existing month
  formatting.
- Layout follows the design rules: the month field sits in the same grid as the amount (one column on phones, two
  from `sm`), the caption is `caption` / `text.secondary` under the amount, right-aligned in the row like the period
  caption.
- Accessibility: the month field has a visible label («Діє з» in the dialog, «Ліміт діє з» on the settings card so a
  screen reader names whose month it is), a helper text and the placeholder «РРРР-ММ» where the browser shows a text
  box; the caption is plain text.
