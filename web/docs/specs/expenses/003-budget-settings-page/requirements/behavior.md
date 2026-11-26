# Budget settings page — rules and scenarios

## Business rules

### R-W3-001 — Settings form

Both fields may be empty (sent as 0 = none); otherwise an amount ≥ 0 with at most two decimals and at most
10 000 000. A positive value shows as typed text; 0 shows empty. Saved with the budget's `version` in `If-Match`.

### R-W3-002 — Limits list

Limits in the API's order, each with name, amount, «в месяц» / «в квартал», and its categories by name.

### R-W3-003 — Limit form

Name required, trimmed, ≤ 100; amount as entries; at least one category; period month by default. The request sends
distinct categories.

### R-W3-004 — Limit writes

Add with `Idempotency-Key` (one per opened form), change with `If-Match`, delete after confirmation; each refreshes
the budget.

## Acceptance scenarios

### SC-W3-001 — Budget shown

**Covers:** R-W3-001, R-W3-002
Given the seeded budget → the form shows 50 000 and 100 000 and the seven limits with their categories

### SC-W3-002 — Save settings with the version

**Covers:** R-W3-001
When the user changes the limit and saves → `If-Match` carries the budget version and «Сохранено» shows

### SC-W3-003 — Check before saving

**Covers:** R-W3-001
When an amount has three decimals → the field message shows and nothing is sent

### SC-W3-004 — Add a limit

**Covers:** R-W3-003, R-W3-004
When the user adds a limit with a name, amount and categories → the request carries an `Idempotency-Key` and the categories

### SC-W3-005 — Change and delete a limit

**Covers:** R-W3-004
When the user changes a limit → `If-Match` carries its version; when deleting → confirmation first

### SC-W3-006 — Limit form rules

**Covers:** R-W3-003
Given a new limit → monthly and empty; given no name, no amount or no category → messages; the request trims the
name and removes duplicate categories
