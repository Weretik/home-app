# 006 — Limit amounts by month (web)

- **Application type:** React Web (Vite SPA, MUI, React Query)
- **Application:** `web/`
- **Project architecture source:** [web architecture](../../../architecture.md), [web rules](../../../rules.md), [design rules](../../../design.md)
- **Domain:** expenses, settings page «Налаштування» of the «Бюджет» section
- **Status:** implemented, audited
- **Owner:** Vitaliy
- **Created:** 2026-11-27
- **Backend counterpart:** [api 006](../../../../../api/docs/specs/expenses/006-limit-amounts-by-month/README.md)
- **Decision:** [web ADR 0018](../../../decisions/0018-limit-change-applies-from-a-month.md)

Changing a limit's amount now says from which month it applies, and the settings page shows what the amount was
before. Past months keep their own limit; all the arithmetic stays on the server, the page only sends the month and
shows the steps it gets back.

## Requirements

- [Scope, rules and scenarios](requirements/overview.md)

## Technical context

- [Frontend design](design/frontend.md)
- [API contract use](contracts/api-contract.md)
- [Traceability and task evidence](traceability.md)
- [Code audit](code-audit/audit.md)

## File map (as built)

Paths are relative to `web/`.

| Path                                                                                        | State                | One responsibility                                                                                                                                             | Task           |
| ------------------------------------------------------------------------------------------- | -------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------- |
| `src/api-client/generated/api.ts` (written by `npm run contracts:generate`)                 | existing (generated) | the new `amounts`, `monthlyLimits`, `amountFrom`, `monthlyLimitFrom`                                                                                           | EN-001         |
| `src/features/expenses/budget-data/budget-api.ts`                                           | existing             | the `AmountStep` type over the generated schema                                                                                                                | EN-001         |
| `src/features/expenses/data/month.ts` (+ test)                                              | existing             | `kyivMonthText` (current month in Kyiv as `YYYY-MM`), `monthStartDay` (`YYYY-MM` → `YYYY-MM-01`)                                                               | TS-001, TS-002 |
| `src/features/expenses/entry-editing/entry-fields.ts` (+ test)                              | existing             | `monthError` («Виберіть місяць» for an empty or malformed month)                                                                                               | TS-001, TS-002 |
| `src/features/expenses/limit-editing/limit-form.ts` (+ test)                                | existing             | `amountFrom` in the form values (edit only); `sendsAmountFrom` / `toLimitRequest` send it only when the amount or the month changed                            | TS-001         |
| `src/features/expenses/limit-editing/MonthField.tsx`                                        | new                  | the native month input: label («Діє з» by default), month or quarter hint, placeholder «РРРР-ММ», field errors                                                 | TS-001, TS-002 |
| `src/features/expenses/limit-editing/LimitDialog.tsx`                                       | existing             | the «Діє з» field when editing; keeps the starting values for the request mapper                                                                               | TS-001         |
| `src/features/expenses/settings-page/budget-settings-form.ts` (+ test)                      | existing             | `monthlyLimitFrom` in the values; `toBudgetSettingsRequest` sends it only when the monthly limit or the month changed                                          | TS-002         |
| `src/features/expenses/settings-page/BudgetSettingsCard.tsx`                                | existing             | the «Ліміт діє з» field beside the monthly limit; the earlier and the scheduled amount in its helper text                                                      | TS-002         |
| `src/features/expenses/settings-page/amount-steps.ts` (+ test)                              | new                  | `stepsAround` (step in force and the one before), `nextStep`, `formatSince` («з грудня 2026»), `earlierAmountCaption`, `nextAmountCaption`, `monthlyLimitHint` | TS-003         |
| `src/features/expenses/settings-page/LimitRow.tsx`                                          | existing             | the earlier and the scheduled amount under the current one                                                                                                     | TS-003         |
| `src/features/expenses/settings-page/SettingsPage.test.tsx`                                 | existing             | SC-WLA-* through the page with the mocked API                                                                                                                  | TS-004         |
| `docs/architecture.md`, `docs/decisions/0018-*.md`, `docs/decisions/README.md`, this folder | existing / new       | docs, ADR, traceability, audit                                                                                                                                 | TS-005         |

Folders stay at ≤ 6 code files (`settings-page/` is at 6, `limit-editing/` at 4), components ≤ 150 lines, modules
≤ 200 (`npm run architecture:structure`). The next file in `settings-page/` splits it by meaning (a `limits/`
subfolder with `LimitsCard`, `LimitRow` and `amount-steps`); the next field in `LimitDialog.tsx` moves the categories
select into its own component.

## Planning

| Task   | Work                                                                                                              | Depends on         |
| ------ | ----------------------------------------------------------------------------------------------------------------- | ------------------ |
| EN-001 | `npm run contracts:generate` after the API build                                                                  | api TS-006         |
| TS-001 | limit dialog «Діє з»                                                                                              | EN-001             |
| TS-002 | settings card «Діє з» and earlier amount                                                                          | EN-001             |
| TS-003 | limit row earlier amount                                                                                          | EN-001             |
| TS-004 | page tests for SC-WLA-001 … 004                                                                                   | TS-001 … TS-003    |
| TS-005 | docs, ADR                                                                                                         | TS-004             |
| TS-006 | audit remediation: month sent only when changed (SC-WLA-005), change saved ahead shown (SC-WLA-006), labels, docs | TS-005, api AF-002 |

Traceability per scenario, with the proving tests and the task evidence: [traceability.md](traceability.md).

## Change history

- 2026-11-27 — created.
- 2026-11-27 — implemented EN-001, TS-001 … TS-005: the month helpers went into `data/month.ts` (`kyivMonthText`,
  `monthStartDay`) and `entry-editing/entry-fields.ts` (`monthError`); the step helpers and the caption into a new
  `settings-page/amount-steps.ts` (+ test); the field into `limit-editing/MonthField.tsx` (`entry-dialogs/` already
  holds six files). The caption uses the page's amount format («7 000,00 ₴»); on the settings card it is part of the
  monthly limit's helper text.
- 2026-11-27 — audit remediation ([code audit](code-audit/audit.md#remediation)): the month is sent only when the
  amount or the month changed (backend AF-002: an explicit month replaces every later step; R-WLA-001, R-WLA-003,
  SC-WLA-005); a change saved ahead is captioned (R-WLA-007, SC-WLA-006); «Ліміт діє з» and the «РРРР-ММ» placeholder;
  the as-built file map and [traceability](traceability.md). Status: implemented, audited.
