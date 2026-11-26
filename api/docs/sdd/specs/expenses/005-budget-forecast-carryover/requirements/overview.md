# Budget forecast and carry-over — overview and scope

## Goal

As Masha or Vitaliy, I want the budget to carry overspending into the next period, to keep one-off expenses out of
the limits, and to see the month's planned expenses and the expected result, so that the site replaces the «Бюджет»
and «Сводка» tabs of the Google Sheet without losing how they worked.

## In scope

- The expense flag «Не в лимиты» (`OutsideLimits`) on create, edit, list, and its effect on the summary
  ([outside-limits](outside-limits.md)).
- Carry-over of overspend for every spending limit (monthly and quarterly) and for the monthly total limit, starting
  at a configurable date `LimitsFrom` ([carry-over](carry-over.md)).
- Planned expenses for a month: list, add, edit, delete, and «record as expense» ([forecast](forecast.md)).
- The month summary's forecast block and per-limit planned amount.

## Out of scope

- Carrying unspent money forward (the sheet does not do it: carry never goes below zero).
- A per-limit history of past periods' amounts (the current limit amount is used for past periods, as in the sheet).
- An «in limits / above limits» choice on a plan (a plan always counts toward its limits in the forecast).
- Recurring payments («Платежи»), the «План / Факт» block per category, «Баланс по месяцам».
- Authentication: the endpoints stay open and rate limited ([ADR 0015](../../../../../decisions/0015-expenses-open-until-identity.md)).

## Actors and external systems

- Masha, Vitaliy: use the web app; anonymous until the Identity module.
- No external system. No outbox message: every command changes only the `expenses` schema.

## Non-functional requirements

| Requirement | Value | How to check |
| --- | --- | --- |
| Data volume | about 100 expenses and up to 30 plans a month; carry-over iterates at most a few dozen periods since 2026-11-01 | summary handler runs a fixed number of grouped SQL queries, none per period or per limit |
| Latency | system default (the project has no quality targets file) | — |
| Concurrency | two people may edit the same plan; edits and record use the row version | integration test: stale `If-Match` answers 409 |
| Repeats | a repeated plan create returns the first id; a repeated record cannot create a second expense | integration tests |
| Security and access | open writes with the `PublicWrite` rate limit (60/min per client address); no personal data added | `429` listed in OpenAPI for every write |

## Open questions

- [NEEDS CLARIFICATION: Quarterly limits start at the quarter that holds `LimitsFrom` (2026-11-01 → Q4 2026 from
  2026-10-01), so October 2026 spending counts toward the first quarter's carry. The note also says "periods that
  start before LimitsFrom are ignored". The SDD follows the explicit quarterly rule (R-CO-004); confirm with Vitaliy.]
- [NEEDS CLARIFICATION: Does a plan's amount reduce `Left`/`PerDay` of a limit, or is `Planned` shown only next to
  it? The note adds only `LimitUsage.Planned`; the SDD keeps `Left` based on real spending.]
- [NEEDS CLARIFICATION: Do plans of a month whose limit period is a quarter count for the whole quarter in
  `LimitUsage.Planned`? The note says "planned amounts of the limit's categories in the months of its current
  period"; the SDD follows it (R-FC-006).]
- [NEEDS CLARIFICATION (audit 2026-11-26, AF-002): a plan delete takes no version. [forecast](forecast.md) R-FC-003 /
  SC-FC-004, the [contract](../contracts/api-contract.md) and TS-010 require `If-Match` on `deletePlannedExpense`;
  [ADR 0022](../../../../../decisions/0022-planned-expenses-for-the-forecast.md), the module page and the code delete
  without a version, like every other delete in the module (001 GAP-001, 003 GAP-010). Behavior is unchanged until
  Vitaliy picks one rule for all deletes; then either the SDD or the code (and the web client) follows.]
