# Frontend code audit — 006 Limit amounts by month (web), 2026-11-27

- **Scope:** the uncommitted `web/` changes of this SDD: `src/api-client/generated/api.ts`,
  `features/expenses/{budget-data/budget-api.ts, data/month.ts, entry-editing/entry-fields.ts, limit-editing/*,
settings-page/*}` with their tests, `docs/architecture.md`, `docs/decisions/{0018-*,README.md}`,
  `docs/specs/README.md` and this SDD. `api/` is out of scope (changed in parallel).
- **Sources:** [this SDD](../README.md) (R-WLA-001 … 006, SC-WLA-001 … 004), [web ADR 0018](../../../../decisions/0018-limit-change-applies-from-a-month.md),
  `AGENTS.md`, [web rules](../../../../rules.md), [architecture](../../../../architecture.md), [design](../../../../design.md),
  the backend counterpart [api 006](../../../../../../api/docs/specs/expenses/006-limit-amounts-by-month/README.md) (read
  only), the owner's checklist `Architecture/Checklists/frontend/checks/feature.md`.
- **Mode:** audit only; no source file was changed.

## Step 0 — quality targets

| Target                                                                       | Value                                                                                             | Current      |
| ---------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------- | ------------ |
| Main screen load (Бюджет › Налаштування), phone 4G, P75                      | **proposed, to confirm:** ≤ 2.5 s to first content, ≤ 3.5 s to data                               | not measured |
| API P95 / P99 of the reads and writes this page uses (`/budget`, its writes) | proposed in `api/docs/architecture/quality-targets.md`: 300 / 800 ms reads, 400 / 1 000 ms writes | not measured |

The web still has no quality targets of its own (005 audit AF-12, open; here AF-09). The feature adds no request
and only two small pure helpers per render, so it does not move any of these numbers. No performance verdict.

## Findings

Severity: **blocker** — must be fixed before merge; **major** — a written rule broken or a wrong behaviour users hit;
**minor** — docs, naming, UX polish, accepted deviations.

| ID    | Sev.  | Status   | Where                                                                                                | Rule / source                                                        | Evidence                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      | Fix                                                                                                                                                                                                                                                                                                           |
| ----- | ----- | -------- | ---------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| AF-01 | minor | open     | this SDD `README.md:32`, `:38`, `:43` (planned file map)                                             | AGENTS.md "docs in sync"; SDD template (file map = real paths)       | row 32 names `src/api/generated/*`, the real file is `src/api-client/generated/api.ts`; row 38 is still "`data/month.ts` or a new small module"; no rows for `limit-editing/MonthField.tsx`, `settings-page/amount-steps.ts` (+ test), `entry-editing/entry-fields.ts` (`monthError`), `budget-data/budget-api.ts` (`AmountStep`); line 43 still suggests `MonthField` in `entry-dialogs/`. The as-built paths are only in the change history                                                 | turn the map into the as-built map: fix the generated path, replace row 38 with `data/month.ts` (`kyivMonthText`, `monthStartDay`) and `settings-page/amount-steps.ts` (step in force, «з грудня 2026», caption), add `MonthField.tsx`, `entry-fields.ts`, `budget-api.ts`; drop the `entry-dialogs/` example |
| AF-02 | minor | open     | `settings-page/amount-steps.ts:50`; SDD `requirements/overview.md:19`, `:30`                         | R-WLA-004, SC-WLA-003                                                | the rule and scenario write «раніше 7 000 ₴»; the code uses `formatAmount` → «7 000,00 ₴»                                                                                                                                                                                                                                                                                                                                                                                                     | **accept the code**: it is the format of the amount right above the caption (`LimitRow` uses `formatAmount(limit.amount)`), so the row stays consistent and no second amount format appears. Update R-WLA-004 and SC-WLA-003 to «7 000,00 ₴» (or "in the page's amount format")                               |
| AF-03 | minor | open     | `settings-page/amount-steps.ts:16-27`, `:43-52`                                                      | R-WLA-004 (silent on future steps); ADR 0018                         | when the only change is in a later month (steps 7 000 from 2000-01, 6 000 from 2027-04, today 2026-12) `stepsAround` finds no step before today's, so no caption; the row still shows 7 000 and reopening the dialog shows 7 000. A person who saved «from April» sees nothing saying the change is scheduled and may re-save                                                                                                                                                                 | literally spec-compliant, but a UX gap. Owner decides: either add a pure `nextStep(steps, month)` and a caption «з квітня 2027 — 6 000,00 ₴» (rule R-WLA-007, unit + page test), or record "a future change is not shown until its month" as a consequence in ADR 0018 and in R-WLA-004                       |
| AF-04 | minor | accepted | `settings-page/BudgetSettingsCard.tsx:72-76`; `entry-dialogs/AmountField.tsx` (`helperText ?? hint`) | R-WLA-004 ("the same line under the monthly limit field")            | the earlier-amount line is merged into the monthly limit's helper text («з грудня 2026 · раніше 45 000,00 ₴; порожньо — без ліміту»). It is under the field, as the rule asks; it is hidden while the field shows a validation error (the error replaces the hint)                                                                                                                                                                                                                            | accepted: it avoids a second caption element and a new `AmountField` prop, and the error case is transient. Write the merge into R-WLA-004 so the SDD matches                                                                                                                                                 |
| AF-05 | minor | accepted | `settings-page/BudgetSettingsCard.tsx:11` → `limit-editing/MonthField.tsx`                           | rules: features never import each other; structure `importDirection` | a sibling-folder import **inside** the `expenses` feature, not across features; `settings-page` already imports `limit-editing/LimitEditor`; `MonthField` carries the limit period hints, so `limit-editing/` is its natural home; `entry-dialogs/` already holds six files. `architecture.md:170-171` documents the shared use. Structure check passes                                                                                                                                       | accepted, no change. If a third, non-limit user appears, move it with `AmountField` into a split `entry-dialogs/fields/`                                                                                                                                                                                      |
| AF-06 | minor | open     | `limit-editing/MonthField.tsx:32-44`; `BudgetSettingsCard.tsx:81-86`                                 | design rules (labels say what they are), a11y                        | (a) on the settings card the field is labelled only «Діє з» next to two amount fields; a screen reader hears "Діє з, обовʼязкове, Попередні місяці зберігають свій ліміт", which does not name the monthly limit; (b) desktop Firefox and desktop Safari render `type="month"` as a plain text box with no format cue, and the error «Виберіть місяць» does not say the format. Label/helper association, `required` and `aria-invalid` are correct (tested, `SettingsPage.test.tsx:688-711`) | (a) give `MonthField` an optional `label` and pass «Ліміт діє з» on the card; (b) add `placeholder: 'РРРР-ММ'` to `htmlInput` (shown only where the native picker is missing) or say the format in the error                                                                                                  |
| AF-07 | minor | open     | `docs/architecture.md:168` (touched line), also `:56`                                                | glossary (Настройки → Налаштування), docs match code                 | the edited folder line still says «Настройки» page; the UI title is «Налаштування» (`SettingsPage.tsx:19`)                                                                                                                                                                                                                                                                                                                                                                                    | write «Налаштування» on the touched line (the route table rows are older drift; fix together if cheap)                                                                                                                                                                                                        |
| AF-08 | minor | open     | this SDD (no `traceability.md`, no task evidence)                                                    | SDD template, AGENTS.md delivery order                               | traceability is one line in the README; no Red / Green evidence of TS-001 … TS-005; status «implemented (audit pending)»                                                                                                                                                                                                                                                                                                                                                                      | record the test names per scenario (SC-WLA-001 → `SettingsPage.test.tsx` "SC-WLA-001…", etc.) and set the status after this audit                                                                                                                                                                             |
| AF-09 | minor | open     | —                                                                                                    | skill step 0                                                         | no web quality targets (inherited 005 AF-12)                                                                                                                                                                                                                                                                                                                                                                                                                                                  | owner confirms the proposed numbers; not caused by this feature                                                                                                                                                                                                                                               |

No blocker and no major finding.

## Checks with no finding

- **Generated client:** `npx openapi-typescript ../api/docs/contracts/openapi.json` into a scratch file is identical to
  `src/api-client/generated/api.ts` (no hand edits). The contract snapshot itself is still uncommitted on the API
  side; merge order API → client → web applies.
- **Boundaries:** no cross-feature import; the API is reached only through `budget-data/budget-api.ts` /
  `budget-writes-api.ts` (types `AmountStep`, `SpendingLimitRequest`, `BudgetSettingsRequest`); no new query or
  write hook.
- **Folders and sizes:** `settings-page/` has 6 code files (at the limit — the next one splits it, e.g.
  `limits/` = `LimitsCard`, `LimitRow`, `amount-steps.ts`), `limit-editing/` 4, `data/` 6, `entry-editing/` 4.
  Largest touched files: `LimitDialog.tsx` 139 lines (close to 150: the next field should move the categories select
  into its own component), `BudgetSettingsCard.tsx` 109, `limit-form.ts` 75, `amount-steps.ts` 52. Structure check:
  16 rules, 508 files, fine.
- **One responsibility:** `MonthField` is one input; the default month, the conversion, validation and the step
  choice are pure functions (`kyivMonthText`, `monthStartDay`, `monthError`, `stepsAround`, `formatSince`,
  `earlierAmountCaption`) taking `now` for determinism; components only compose. No new `useState`/`useEffect`.
- **Logic has tests:** `month.test.ts` (Kyiv month across the UTC boundary, summer time), `entry-fields.test.ts`
  (`monthError`), `amount-steps.test.ts`, `limit-form.test.ts`, `budget-settings-form.test.ts`.
- **Rules and scenarios:** R-WLA-001 (`limit-form.test.ts`, SC-WLA-001/004 page tests), R-WLA-002 (quarter hint page
  test, month hint in SC-WLA-001), R-WLA-003 (SC-WLA-002), R-WLA-004 (SC-WLA-003 + `amount-steps.test.ts`; see
  AF-02/03/04), R-WLA-005 (empty month blocks saving, page and unit tests; a 400 on `AmountFrom` shows under the field),
  R-WLA-006 (Ukrainian texts from the glossary; the browser only selects a step, no limit or carry arithmetic).
  SC-WLA-001 uses the fixture «Еда и быт» 15 000 instead of «Їжа» 7 000 — same behaviour.
- **Idempotency (3c):** creates keep their `Idempotency-Key` and send no `amountFrom`; edits keep `If-Match`;
  submits stay locked while pending (`EntryDialog`, `BudgetSettingsCard`). A rename or a goal-only save re-sends the
  same amount from the current month, which the API stores as a no-op (api R-LA-002/R-LA-005).
- **ADRs (3b):** web ADR 0018 records the decision and is indexed in `decisions/README.md`; no new app, framework,
  hosting or state approach.
- **Feature review (3d):** access rights — the existing session gate; double-submit lock ✅; end-to-end test ❌ (no E2E
  in the project, inherited open question); architecture.md and specs index updated ✅ (AF-01, AF-07 for the rest);
  scenarios proved by page tests ✅.
- **Design:** month field in the card's grid next to the monthly limit (one column on phones, two from `sm`); caption
  `caption` / `text.secondary` under the amount in the row, like the period caption.

## Verification

Run in `web/` on 2026-11-27, no files changed:

```
npm run typecheck                # tsc -b, no errors
npm run lint                     # no problems
npm test                         # 109 files, 513 tests passed
npm run architecture:structure   # Structure is fine: 16 rules, 508 files
npm run format:check             # All matched files use Prettier code style!
npx openapi-typescript ../api/docs/contracts/openapi.json -o <scratch>  # identical to src/api-client/generated/api.ts
```

## Remediation

Done on 2026-11-27 in the same uncommitted change; `api/` untouched (fixed in parallel).

**Backend AF-002 (follow-up, not a finding of this audit).** The API now treats an explicit `amountFrom` /
`monthlyLimitFrom` as "replace every step from that period on with this amount", even when the amount is unchanged;
only an omitted field keeps the no-op. The web always sent both, so a rename or a goal-only save would have wiped a
change saved ahead, and the "Idempotency (3c)" note above ("re-sends the same amount … a no-op") no longer holds. Fixed:
`sendsAmountFrom` / `toLimitRequest(values, initial)` in `limit-editing/limit-form.ts` and
`toBudgetSettingsRequest(values, current)` in `settings-page/budget-settings-form.ts` send the month only when the
amount differs from the one in force or the month was moved off the default; `LimitDialog` and `BudgetSettingsCard`
keep the form's starting values (`useState`) for the comparison, the card compares with the live
`budget.monthlyLimit`. Unit tests in both form tests; page tests SC-WLA-005 (rename → no `amountFrom`, goal-only save →
no `monthlyLimitFrom`). R-WLA-001 / R-WLA-003, SC-WLA-005 and web ADR 0018 updated.

| ID    | Outcome                                                                                                                                                                                                                                                                                                                               |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| AF-01 | **fixed** — the SDD README has an as-built file map: `src/api-client/generated/api.ts`, `budget-api.ts` (`AmountStep`), `data/month.ts`, `entry-fields.ts` (`monthError`), `MonthField.tsx`, `amount-steps.ts`; the `entry-dialogs/` example replaced by the actual split rules.                                                      |
| AF-02 | **fixed (code kept)** — R-WLA-004 and SC-WLA-003 now say the page's amount format, «7 000,00 ₴» / «5 000,00 ₴».                                                                                                                                                                                                                       |
| AF-03 | **fixed** — pure `nextStep`, `nextAmountCaption` («з квітня 2027 — 6 000,00 ₴») and `monthlyLimitHint` in `amount-steps.ts`; the caption shows under the limit row's amount and in the monthly limit's helper text. New rule R-WLA-007, scenario SC-WLA-006, unit tests and a page test. No new file, so `settings-page/` stays at 6. |
| AF-04 | **fixed (accepted deviation recorded)** — R-WLA-004 now describes the merged helper text and that a validation error replaces it.                                                                                                                                                                                                     |
| AF-05 | accepted, no change (as above).                                                                                                                                                                                                                                                                                                       |
| AF-06 | **fixed** — `MonthField` takes an optional `label` (default «Діє з»); the settings card passes «Ліміт діє з»; `placeholder: 'РРРР-ММ'` on the input (checked in the SC-WLA-002 page test). R-WLA-003 / R-WLA-005 and the design note updated.                                                                                         |
| AF-07 | **fixed** — «Налаштування» on the touched line and on the other Expenses lines of `docs/architecture.md` (route table, `section/`, the settings paragraph); other modules' route rows left as they are (not this change).                                                                                                             |
| AF-08 | **fixed** — `traceability.md` maps SC-WLA-001 … 006 to the test names and records task evidence (TS-006 Red with the new logic switched off, then Green); status «implemented, audited» in the SDD and in `docs/specs/README.md`.                                                                                                     |
| AF-09 | open — owner to confirm the proposed web quality targets (inherited, not caused by this feature).                                                                                                                                                                                                                                     |

Sizes after the fix: `LimitDialog.tsx` 141 lines, `BudgetSettingsCard.tsx` 111, `LimitRow.tsx` 82, `MonthField.tsx` 50,
`amount-steps.ts` 89; `settings-page/` 6 code files, `limit-editing/` 4.

Verification in `web/` after the remediation:

```
npm run typecheck                # tsc -b, no errors
npm run lint                     # no problems
npm test                         # 109 files, 526 tests passed
npm run architecture:structure   # Structure is fine: 16 rules, 508 files
npm run format:check             # All matched files use Prettier code style!
npm run build                    # built (only the existing chunk size warning)
```
