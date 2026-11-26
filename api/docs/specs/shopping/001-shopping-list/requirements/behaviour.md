# Shopping list — behaviour

## Behaviour

- A person types one or more items; each becomes a list row in its department, or revives / updates the same item.
- The list shows needed items by department in store order (urgent first, then by when added), then the cart
  (bought in the last hours), newest bought first.
- Ticking an item moves it to the cart; unticking it brings it back. After the cart window or «Убрать купленные» it
  is history.

## Business rules

### R-001 — Text is split into items

The text (1..1000 characters) is split on `,`, `;` and new lines; bullets and trailing dots are trimmed; empty parts
are dropped. A quantity is a number (with `.` or `,`) at the end, optionally after `-`, `–`, `—`, `:` or `x`/`х`/`×`,
or at the start, followed by an optional unit (`шт`, `кг`, `г`, `л`, `мл`, `уп`, `пач…`, `бут…`, …, the bot's list).
The name is trimmed, cut to 80 characters and starts with a capital letter; the quantity is kept as typed, at most 30
characters. 1..50 items per add.

### R-002 — The department is matched from the dictionary

The name is normalised (lower case, `ё` → `е`, spaces collapsed) and split into words. A keyword equal to the whole
name scores 1000; otherwise a keyword matches when each of its words is the start of some word of the name, scoring
the keyword's length. The best score wins (on a tie the keyword of the department earlier in store order, then the
alphabetically first keyword); no match → the fallback department. A department chosen in the add form overrides
matching for all items of that add.

### R-003 — Re-adding the same item

Names compare normalised. If the same item is needed, it stays (its quantity is replaced when a new one is given;
«Срочно» is switched on when asked). If it is in the cart, it is needed again, added now by the person who added it
(quantity replaced when given). Otherwise a new row is added. Repeats inside one text behave the same.

### R-004 — List, cart and history are computed

An item is in the list when it is needed, or bought, not cleared and bought less than `CartKeepHours` ago. Every
other bought item is history. Nothing moves rows on a timer.

### R-005 — Status is set, not toggled

Setting a status is idempotent: «bought» records who and when the first time; «needed» clears who and when. An item
that is history cannot change status (404, as if missing).

### R-006 — Clearing the cart

«Убрать купленные» marks every item now in the cart as cleared and returns how many.

### R-007 — Editing teaches the dictionary

An edit (name 1..80, quantity ≤30, note ≤200, department, urgent) needs the version the person started from. When
the department changes to a non-fallback one, the normalised name (if 2..60 characters) becomes a learned keyword of
that department; an existing keyword with that text moves there and becomes learned. History rows cannot be edited.

### R-008 — Deleting an item

A list row or a history row is deleted for good with its version.

### R-009 — Frequent items

Bought items of the last `FrequentDays`, grouped by normalised name, the 20 bought most often (ties: most recently
bought), without names now in the list; shown with the latest name and department.

### R-010 — History

Bought items that are no longer in the list, newest bought first, page by page (≤100).

### R-011 — Summary

Needed, urgent and in-cart counts; bought in the last 7 and 30 days; departments with needed items in store order;
up to 8 needed items, urgent first; the top 5 frequent names.

### R-012 — Departments

Name 1..40 (trimmed, unique), icon up to 8 characters (emoji). A new department goes just before the fallback. The
order is set by sending every department id exactly once. Deleting a department moves its items and keywords to the
fallback department; the fallback cannot be deleted (409). Edits and deletes need the version.

### R-013 — Keywords

A keyword is stored normalised, 2..60 characters, unique; its department must exist. Created by hand it is not
learned. Edits and deletes need the version.

### R-014 — Settings

`CartKeepHours` 1..72 (default 3), `FrequentDays` 30..730 (default 183); edits need the version.

### R-015 — Repeats and races

Creates (add items, department, keyword) carry an `Idempotency-Key`: a repeat returns the first result, the same key
with other data is 409. An edit or delete with an old version is 409; names and keywords taken by a racing request
are 409.

### R-016 — Starting dictionary

The 13 departments of «Справочник» in store order (icon split from the name, «Другое» is the fallback) and all its
keywords, normalised and de-duplicated, are there after the first migration.

## Acceptance scenarios

### SC-001 — Add several items from text

**Covers:** R-001, R-002

Given an empty list
When Vitaliy adds «молоко, хлеб, яйца 10, сыр 300 г»
Then four items are needed: Молоко (Молочное и яйца), Хлеб (Хлеб), Яйца ×10 (Молочное и яйца), Сыр ×300 г (Сыр и колбаса), added by Vitaliy

| Input | Name | Quantity |
| --- | --- | --- |
| `2 кг картошки` | Картошки | 2 кг |
| `молоко x 3` | Молоко | 3 |
| `сыр 300 г` | Сыр | 300 г |
| `- хлеб.` | Хлеб | — |

### SC-002 — Department matching

**Covers:** R-002

| Name | Department |
| --- | --- |
| Зубная паста | Гигиена и аптека («зубн паст» beats «паст») |
| a name that is exactly a keyword | that keyword's department (score 1000) |
| Штуковина | Другое (fallback) |

### SC-003 — Re-add a needed item / an item in the cart

**Covers:** R-003

Given «Молоко» is needed with quantity 1
When Masha adds «молоко 2»
Then there is still one «Молоко», quantity 2, reported as updated
Given «Хлеб» is in the cart
When Masha adds «хлеб»
Then «Хлеб» is needed again, added by Maria, reported as restored

### SC-004 — Tick and untick

**Covers:** R-004, R-005

Given «Хлеб» is needed
When Vitaliy marks it bought twice
Then it is in the cart, bought by Vitaliy at the first time
When he marks it needed
Then it is needed and has no buyer

### SC-005 — The cart window

**Covers:** R-004, R-014

Given an item bought 2 hours ago and one bought 4 hours ago, cart window 3 hours
When the list is read
Then the first is in the cart and the second is history; a status change of the second answers 404

### SC-006 — Clear the cart

**Covers:** R-006

Given two items in the cart
When «Убрать купленные» is pressed
Then the answer is 2, both are in history, and the list keeps only needed items

### SC-007 — Edit an item and learn its department

**Covers:** R-007, R-015

Given «Штуковина» in Другое
When it is moved to «Бакалея» with its version
Then it is in «Бакалея» and «штуковина» is a learned keyword of «Бакалея»; the next «штуковина» goes there
And the same edit with the old version answers 409

### SC-008 — History and frequent items

**Covers:** R-009, R-010

Given «Кефир» bought three times and now not in the list
When history and frequent items are read
Then history lists the purchases newest first with who bought them, and «Кефир» is among frequent items with 3

### SC-009 — Departments are managed

**Covers:** R-012

When a department «Рынок» is added it is placed before «Другое»; when it is deleted its items and keywords move to
«Другое»; deleting «Другое» answers 409; a reorder that misses an id answers 400.

### SC-010 — Keywords are managed

**Covers:** R-013

A keyword « Зубн  Паст » is stored as «зубн паст»; a duplicate answers 409; a keyword of one character answers 400.

### SC-011 — Settings

**Covers:** R-014

Cart hours 0 or 73 answer 400; a valid change with the current version is stored; an old version answers 409.

### SC-012 — A repeated add

**Covers:** R-015

Given an add with key K succeeded
When the same body is sent with K
Then the same items are returned and nothing new is added; another body with K answers 409

### SC-013 — Summary

**Covers:** R-011

Given needed, urgent and bought items
When the summary is read
Then counts, departments in store order, next items (urgent first) and top frequent names match the list

### SC-014 — Starting dictionary

**Covers:** R-016

After migrations there are 13 departments, the first «Овощи и фрукты» 🥦, the last «Другое» 🧺 (fallback), and
the seeded keywords.

## Deferred scenarios

- Removing items by text («-молоко»), undo endpoint, sheet import endpoint (see overview).
