# Entries and sheet import — behavior

## Behavior

- A person lists the expenses of a month or of a period and narrows them by category and payer; incomes by month and
  person.
- A person adds, changes and deletes an expense or an income.
- The operator imports the sheet once (and may run it again safely).

## Business rules

### R-EN-001 — Money

An amount is hryvnias with kopecks: greater than 0, at most 10 000 000, at most two decimals; stored as
`numeric(12,2)`.

### R-EN-002 — Dates and comments

A date is a calendar day between 2000-01-01 and 2100-12-31. A comment is trimmed, at most 500 characters; empty is
stored as `null`.

### R-EN-003 — Who

An expense has a payer `Vitaliy`, `Maria` or `Shared` and an existing category; an income has a person `Vitaliy` or
`Maria`. A payment method is `Card` or `Cash`.

### R-EN-004 — Listing expenses

The query takes either `year` and `month`, or `from` and `to` (both included, `to ≥ from`, at most 366 days, years
2000–2100); any other combination answers 400. Optional `categoryId`, `payer`. Ordered by date then id, newest
first. `totalAmount` is the sum of the whole filtered period, not of the page. Each item carries its `version`.

### R-EN-005 — Listing incomes

`year` 2000–2100 and `month` 1–12 are required; optional `person`; same order, paging and `totalAmount`.

### R-EN-006 — Paging

`page` ≥ 1, `pageSize` 1–100 (default 50).

### R-EN-007 — Idempotent create

A create needs an `Idempotency-Key` (1–100 characters). The key, the kind (`expense`, `income`), a SHA-256
fingerprint of the data and the new id are stored with the row in one transaction. Same key + same data → 201 with the
first id; same key + other data or another kind → 409 `expenses.idempotency_key_reused`; a racing copy hits the
primary key and is answered like a repeat.

### R-EN-008 — Versioned edit

An edit sends the version from the list in `If-Match` (quoted or not, `W/` ignored). Missing → 400. Different from the
stored one, or changed between read and save → 409 `expenses.changed_meanwhile`. An edit replaces date, payer/person,
category, amount, payment method and comment; the import key and creation time stay.

### R-EN-009 — Delete

A delete removes the row for good: 204, or 404 for an unknown id. No version is checked.

### R-EN-010 — Sheet import

- The endpoint is off (404) unless `Expenses:Import:Key` is set; the `X-Import-Key` header must match it (constant
  time), else 401.
- Headers must match the sheet's; dates `dd.MM.yyyy`; amounts `2,424.00`; savings amounts may be signed; names
  «Виталий», «Мария»/«Маша», «Общее»; «Карта»/«Наличные»; categories by name, case-insensitive.
- All or nothing: any unreadable cell, unknown category or broken rule answers 400 `expenses.import_invalid_rows`
  listing every problem with its sheet row number, and nothing is stored.
- Every row gets an import key (hash of its values plus its occurrence among identical rows); rows whose key is stored
  are skipped, so a repeat adds nothing and a later row is picked up.
- The answer reports rows added and skipped per tab and sums per month.

## Acceptance scenarios

### SC-EN-001 — List a month

**Covers:** R-EN-004, R-EN-006

Given expenses on several days of a month
When the month is requested with `pageSize` 2
Then the newest two come first, `totalCount` is the month's count and `totalAmount` is the month's sum

### SC-EN-002 — List a period

**Covers:** R-EN-004

Given expenses on 2043-03-31, 2043-04-01 and 2043-04-30
When `from=2043-03-31&to=2043-04-30` is requested
Then all three are listed, both ends included, with their total

### SC-EN-003 — Wrong period or month

**Covers:** R-EN-004, R-EN-005

When `from` is after `to`, the period is 367 days, only `from` is sent, both a month and a period are sent, or
`month=13`
Then the answer is 400 and nothing is listed

### SC-EN-004 — Add, change, delete an expense

**Covers:** R-EN-007, R-EN-008, R-EN-009

Given a new key
When an expense is created
Then 201 with `{ id }` and a `Location`
When it is changed with its listed version
Then 204 and the list shows the change with a new version
When it is changed again with the old version
Then 409 `expenses.changed_meanwhile`
When it is deleted
Then 204, and a second delete answers 404

### SC-EN-005 — Repeated create

**Covers:** R-EN-007

Given an expense created with key K
When the same body is sent with K
Then 201 with the same id and one row
When another body is sent with K
Then 409 `expenses.idempotency_key_reused`

### SC-EN-006 — Missing key or version

**Covers:** R-EN-007, R-EN-008

When a create has no `Idempotency-Key`, or an edit has no `If-Match`
Then 400 with a field error

### SC-EN-007 — Invalid entry

**Covers:** R-EN-001, R-EN-002, R-EN-003

When an amount is 0, negative, 10 000 000.01 or 1.005, a date is 1999-12-31, a comment has 501 characters, or the
category is unknown
Then 400 (404 `expenses.category_not_found` for the category) and nothing changes

### SC-EN-008 — Import needs the key

**Covers:** R-EN-010

Given no key configured
Then the import answers 404
Given a key configured
When the header is missing or wrong
Then 401

### SC-EN-009 — Import stores rows and a repeat skips them

**Covers:** R-EN-010

Given sheet rows including two identical water bills on one day
When the import runs
Then both identical rows are stored and the report shows them added
When it runs again with the same rows
Then every row is reported skipped and nothing is added

### SC-EN-010 — A bad row stops the import

**Covers:** R-EN-010

Given one row with an unknown category and one with an unreadable amount
When the import runs
Then 400 `expenses.import_invalid_rows` names both rows by sheet row number and nothing is stored

## Deferred scenarios

- Category management.
