# 0018. A limit change says from which month it applies

Status: accepted, 2026-11-27

Specification: [web SDD 006](../specs/expenses/006-limit-amounts-by-month/README.md); backend:
[api ADR 0041](../../../api/docs/decisions/0041-limit-amounts-kept-by-month.md).

## Context

The API now keeps each limit's amount by the period it starts in. The page must let a person say when a new amount
starts, without turning the settings page into a history editor.

## Decision

- Editing an amount (a category limit or the monthly limit) shows one month field «Діє з», defaulting to the current
  month in Kyiv; the request carries its first day only when the amount differs from the one in force or the month
  was moved off the default. The API treats an explicit month as "replace every step from that period on", so a save
  that changes neither leaves it out. Creating a limit has no such field.
- The page shows only the step in force and the amount before it, and the next change saved ahead, as captions; the
  full list stays in the API.
- The month is plain `YYYY-MM` text in the form, converted in the request mappers; the helpers that pick the default
  month and the earlier step are pure and unit-tested.

## Consequences

- A person who only renames a limit (or saves only the savings goal) sends no month, so neither the past nor a change
  saved ahead is touched.
- Picking a past month rewrites that month on purpose; the hint under the field says so.
- If a full history view is ever needed it can be built on the same `amounts` field.
