# Shopping list — overview and scope

## Goal

As Masha or Vitaliy, I want one shared shopping list on the site that understands «молоко, хлеб, яйца 10», sorts
items by the store's departments and lets either of us tick items off in the shop, so that we stop using the Google
Sheet and its Telegram bot.

## In scope

- The list: add from free text (several items at once, quantities, automatic department), edit (name, quantity,
  note, department, «Срочно»), tick bought / not bought, delete, clear the cart.
- The cart window: a bought item stays visible for `CartKeepHours` (default 3) or until «Убрать купленные», then it
  is history. Computed on read, no background job.
- History of bought items, newest first, paged; delete a history row; «Вернуть в список» is an ordinary add.
- ⭐ Frequent items: top 20 names bought in the last `FrequentDays` (default 183), without names already in the list.
- The home page summary (counts, departments with needed items, next items, top frequent).
- Settings: departments (rename, icon, add, delete → fallback, reorder), keywords (search, filter, add, edit, delete,
  «выучено» marker), module settings (cart hours, frequent days).
- A keyword is learned when a person moves an item to another (non-fallback) department.
- Seed: the 13 departments and all keywords of the sheet's «Справочник».

## Out of scope

- Sign-in and per-user rights: the site is open and rate limited until the Identity module (as Expenses,
  [ADR 0015](../../../../decisions/0015-expenses-open-until-identity.md)). «Кто я» is chosen on the device and sent
  as `addedBy` / `by`.
- The Telegram bot and «-молоко» text removal (the site has a delete button), undo as an endpoint (the web undoes an
  add by deleting the added rows).
- Import of the sheet's list and history: done once with a SQL script by the operator (`import_key` column is ready).
- Links to Expenses (prices, receipts).

## Actors and external systems

- Masha, Vitaliy: use the list from phones at the same time (the web polls every 20 s).
- Google Sheet «🛒 Список покупок»: read once to seed the dictionary (and later by the operator for the import).

## Non-functional requirements

| Requirement | Value | How it is checked |
| --- | --- | --- |
| Volume | list ≤ ~100 rows, history ~3 000 rows a year, ~400 keywords | queries bounded: list in memory, history paged in SQL (≤100), frequent grouped in SQL |
| Freshness | another person's change visible within 20 s | web polling; the API is stateless |
| Concurrency | two people tick and edit at once without lost updates | status toggle sets a target value; edits use `xmin` + `If-Match` → 409 |
| Repeats | a flaky phone repeating an add does not duplicate items | `Idempotency-Key` on creates (ADR 0007) |
| Security | open writes rate limited by `PublicWrite` (60/min per address) | every write endpoint opts in; tested on the OpenAPI document conventions |
| Personal data | only the names «Vitaliy» / «Maria» as who added / bought | no new personal data list entries |

## Open questions

None. History paging follows the Expenses list pattern (`page`, `pageSize`, `totalCount`) instead of a cursor: the
brief asks to follow that pattern, and history of two people stays small.
