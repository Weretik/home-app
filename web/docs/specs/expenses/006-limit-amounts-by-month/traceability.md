# Traceability — 006 Limit amounts by month (web)

Status: **implemented, audited** (2026-11-27, [code audit](code-audit/audit.md)).

Test files are relative to `web/src/features/expenses/`. Page tests run the real page against a mocked `fetch` with
«now» fixed at 10 December 2026 in Kyiv.

## Scenarios

| Scenario   | Rules                | Tasks          | Proving tests                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |
| ---------- | -------------------- | -------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| SC-WLA-001 | R-WLA-001, R-WLA-002 | TS-001, TS-004 | `settings-page/SettingsPage.test.tsx` "SC-WLA-001: a changed amount applies from the current month by default", "changes a limit with its version in If-Match", "a quarterly limit says the amount starts with the quarter; a cleared month blocks saving"; `limit-editing/limit-form.test.ts` "starts an existing limit from its values and the current Kyiv month", "a changed amount is sent from the default month", "sends an edit with the first day of its month"            |
| SC-WLA-002 | R-WLA-003, R-WLA-005 | TS-002, TS-004 | `SettingsPage.test.tsx` "SC-WLA-002: the monthly limit is saved from the chosen month", "saves the limit and the goal with the version in If-Match"; `settings-page/budget-settings-form.test.ts` "an empty field is sent as 0, the month as its first day", "the same limit from another month is sent", "the month is needed"                                                                                                                                                     |
| SC-WLA-003 | R-WLA-004            | TS-003, TS-004 | `SettingsPage.test.tsx` "SC-WLA-003: a changed limit shows since when and the amount before; one amount shows none"; `settings-page/amount-steps.test.ts` "picks the step in force in a month and the one before it", "the caption takes today in Kyiv", "the monthly limit hint joins the earlier and the next amount"                                                                                                                                                             |
| SC-WLA-004 | R-WLA-001            | TS-001, TS-004 | `SettingsPage.test.tsx` "SC-WLA-004: a new limit has no «Діє з» month and sends none", "adds a limit with an Idempotency-Key and its categories"; `limit-form.test.ts` "starts a new limit monthly and empty, with no «Діє з» month", "a new limit never sends «Діє з»"                                                                                                                                                                                                             |
| SC-WLA-005 | R-WLA-001, R-WLA-003 | TS-006         | `SettingsPage.test.tsx` "SC-WLA-005: a rename sends no «Діє з», so a change saved ahead stays", "SC-WLA-005: a goal-only save sends no monthlyLimitFrom"; `limit-form.test.ts` "a rename or new categories with the amount and month untouched send no «Діє з»", "the same amount from another month is sent: it replaces the later steps"; `budget-settings-form.test.ts` "a goal-only save sends no month, so later steps stay", "a changed limit is sent from the default month" |
| SC-WLA-006 | R-WLA-007            | TS-006         | `SettingsPage.test.tsx` "SC-WLA-006: a change saved ahead shows its month and amount"; `amount-steps.test.ts` "the next step is the first one after the month", "a change saved ahead is captioned with its month and amount"                                                                                                                                                                                                                                                       |

Rules without a scenario of their own: R-WLA-005 (month validation) — `entry-editing/entry-fields.test.ts` "needs a
month between 2000 and 2100", `limit-form.test.ts` "an edited limit needs its month", the page tests "a quarterly
limit … a cleared month blocks saving" and "the API rejecting the month shows under the month field", and the
placeholder check in SC-WLA-002; R-WLA-006 (Ukrainian texts, no arithmetic) — review, and the Kyiv month helpers in
`data/month.test.ts` "takes the current month in Kyiv, not in the browser zone", "turns a month field into its first
day".

## Task evidence

| Task   | Evidence                                                                                                                                                                                                     |
| ------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| EN-001 | `src/api-client/generated/api.ts` regenerated; the audit's scratch regeneration is identical (`npx openapi-typescript ../api/docs/contracts/openapi.json`).                                                  |
| TS-001 | `limit-form.test.ts`, page tests SC-WLA-001 / SC-WLA-004 green.                                                                                                                                              |
| TS-002 | `budget-settings-form.test.ts`, page test SC-WLA-002 green.                                                                                                                                                  |
| TS-003 | `amount-steps.test.ts`, page test SC-WLA-003 green.                                                                                                                                                          |
| TS-004 | `SettingsPage.test.tsx` green (all SC-WLA-* page tests).                                                                                                                                                     |
| TS-005 | `docs/architecture.md`, web ADR 0018 and its index, this folder.                                                                                                                                             |
| TS-006 | Red: with the new logic switched off (the mappers always sending the month, `nextAmountCaption` finding nothing) the three page tests SC-WLA-005 (×2) and SC-WLA-006 fail; green with it on. Full run below. |

Full check run in `web/` on 2026-11-27 after the remediation: `npm run typecheck`, `npm run lint`, `npm test`,
`npm run architecture:structure`, `npm run format:check`, `npm run build` — all passed (numbers in the
[audit's remediation section](code-audit/audit.md#remediation)).
