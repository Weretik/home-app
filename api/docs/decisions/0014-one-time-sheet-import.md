# 0014. One-time import from the Google Sheet through a keyed endpoint

Status: accepted, 2026-11-26

## Context

About 250 expenses and 20 incomes from October 2026 on live in the Google Sheet. They must reach the production
database once, exactly, without putting personal financial data into the repository, and without a person typing
them again. The database password lives only in the App Service and in a GitHub secret.

## Decision

- An `ImportSheet` command in the module reads both tabs as the Sheets API returns them (rows of cell text, header
  first), checks every cell, and stores all rows or none.
- Every row gets an import key (hash of its values plus its occurrence among identical rows), stored under a unique
  index. Repeating the import skips what is stored; a racing repeat ends in `409`.
- The command is exposed as `POST /api/expenses/imports/sheet`, hidden from the OpenAPI document, enabled only while
  the App Service variable `Expenses__Import__Key` is set, checked in constant time against the `X-Import-Key`
  header, and rate limited. The operator sets the key, runs the import, checks the monthly totals and removes the
  key.
- The sheet is only read; it is not changed.

## Consequences

- The data never passes through git; the import logic is tested code (unit tests for reading cells and keys,
  integration tests for the endpoint).
- The same endpoint can catch up rows added to the sheet while both are in use.
- When the sheet is retired, the endpoint and `Imports/` can be removed in one change; the import keys stay as a
  record of where a row came from.
