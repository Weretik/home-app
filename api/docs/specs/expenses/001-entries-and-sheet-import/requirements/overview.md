# Entries and sheet import — overview and scope

## Goal

As Masha or Vitaliy, I want to record every expense and income on the site and see them by month or period, so that
the Google Sheet and its Telegram bot are no longer needed.

## In scope (implemented)

- Expense list for a month or a period (`from`–`to`, both included, at most 366 days), filters by category and payer,
  50 rows a page by default (1–100), newest first, with the total of the whole filtered period.
- Income list for a month, filter by person, same paging.
- Category list in sheet order.
- Create (with `Idempotency-Key`), edit (with `If-Match` version), delete for expenses and incomes.
- The one-time, repeatable import of the sheet tabs «Расходы», «Доходы» and optionally «Копилка» through a keyed,
  hidden endpoint.

## Out of scope

- Managing categories (planned page «Категории»).
- Accounts or cards per payment (card numbers stay in the comment).
- Sign-in: open until the Identity module ([ADR 0015](../../../../decisions/0015-expenses-open-until-identity.md)).

## Actors and external systems

- Masha, Vitaliy through the web app (anonymous, rate limited).
- Operator (Vitaliy) calling the import with `X-Import-Key` from a local machine; Google Sheets API is read by the
  operator, not by the API.

## Non-functional requirements

| Requirement | Value | How it is checked today |
| --- | --- | --- |
| Data volume | about 100 expenses a month since 2026-10-01 | paging in SQL, page size ≤ 100 |
| Import size | ≤ 10 000 rows per tab, ≤ 20 cells per row, ≤ 1 000 characters per cell | `ImportSheetValidator` |
| Rate limit | `PublicWrite`, 60 writes a minute per client address | OpenAPI lists 429 on every write |
| Latency | system default; the project has no quality targets file | — |

## Open questions and gaps found while writing this SDD

- [GAP-001] Deletes of expenses and incomes take no version (`If-Match`), unlike edits; a delete can remove a row that
  someone else just changed. ADR 0017 speaks of "edits" only, so this matches the ADR, but the new SDD 005 requires
  `If-Match` on plan deletes — decide one rule for the module.
  **Open, product question (2026-11-26 audit):** behavior unchanged until Vitaliy decides one rule for deletes.
- [GAP-002] `api/docs/rules.md` says "domain tests and handler tests in `UnitTests`". There are no handler unit tests;
  handlers are covered only by integration tests (which need Docker).
  **Resolved 2026-11-26 ([audit 2026-11-26](../../../../audits/2026-11-26-expenses-budget-forecast.md)):** the rule now says what goes where — domain rules, validators and handler
  arithmetic in `UnitTests`, handler orchestration over the database in `IntegrationTests` (an in-memory database
  cannot check `xmin`, unique indexes or `ExecuteDelete`); entry validators got unit tests (`EntryInputValidatorTests`).
- [GAP-003] API tests split the cases between kinds: the add/change/delete path is tested only for expenses, the
  repeated-create and missing-key/version paths only for incomes (`ExpensesEditingApiTests`). Income edit/delete and
  the expense repeated create have no API test; the create/update validators of both have no unit tests.
  **Resolved 2026-11-26:** `ExpensesEditingApiTests.An_Income_IsChanged_RefusedWhenStale_AndDeleted` and
  `.A_Repeated_Expense_Create_ReturnsTheFirstId_AndOtherData_IsAConflict`; validators in `EntryInputValidatorTests`.
- [GAP-004] The version race path (`DbUpdateConcurrencyException` → 409 between read and save) has no test.
  **Resolved 2026-11-26:** `ExpensesRaceApiTests.An_Edit_RacedByAnotherEdit_IsAConflict_AndTheOtherEditStays` lets a
  second edit in between the read and the save (`TestHost/RacingExpensesDb`).
- [GAP-005] The import answers a wrong `X-Import-Key` with a bare 401 without a problem body (the endpoint is hidden
  from OpenAPI, so the "every error is a problem response" rule is not enforced there).
