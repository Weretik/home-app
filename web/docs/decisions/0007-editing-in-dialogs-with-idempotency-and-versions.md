# 0007. Editing in dialogs, with an Idempotency-Key, If-Match versions and signed colours

Status: accepted, 2026-11-26

## Context

Expenses, incomes and the savings jar («Копилка») are now written from the site instead of the Google Sheet. Two
people edit the same month, often from phones on a weak connection: a tap on «Сохранить» can be sent twice, and one
person can change a row the other has open. The backend protects creates with an `Idempotency-Key` (a repeat returns
the first id; the same key with other data is a 409) and edits with the row's `version` in `If-Match` (409
`expenses.changed_meanwhile` when someone changed it meanwhile). The jar stores signed amounts: plus put in, minus
taken out.

## Decision

- Add and edit open a dialog over the list (full screen on a phone); delete asks in a small confirmation dialog. Rows
  get a «⋮» menu with «Изменить» and «Удалить». No separate edit pages or routes: the list and its month stay in view.
- Writes are React Query mutations in the feature's `data/writes-api.ts`. A success invalidates the queries of that
  kind of entry; a 404 or `expenses.changed_meanwhile` also reloads them and the form says so in Russian.
- A create sends an `Idempotency-Key`. One key belongs to one opened form and to one set of data: a retry of the same
  data reuses it, changed data takes a new one, and a form opened again starts with a new one (`useIdempotencyKey`).
- An edit sends the `version` of the row the list showed in `If-Match`. The client never resolves a conflict itself:
  it shows «Запись изменили в другом месте — обновите и попробуйте снова» and reloads the list.
- Form logic is shared (`entry-editing/useEntryForm`): the form's own checks first (amount above zero with at most
  two decimals, date required, category for an expense, comment up to 500 characters), then the API's field errors
  under the same fields. A 429 says there were too many changes in a row.
- The jar's form asks for the direction («Положили в копилку» / «Взяли из копилки») and a positive amount; the code
  applies the sign. Signed amounts are shown with an explicit «+» or «−», plus in `primary` blue and minus in `error`
  red (docs/design.md, «Signed amounts»).

## Consequences

- A double tap or a retry after a lost answer never creates a second entry, and a stale edit never overwrites
  someone else's change.
- A new editable list reuses the dialog frame, the row menu and `useEntryForm`; it adds a form file (values ↔ request)
  and a dialog with its fields.
- `error` red now has a second meaning on signed amounts; the sign keeps it readable without colour.
- The month total cards are gone from Расходы and Доходы; totals move to a future summary page.
