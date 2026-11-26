# Gifts section — scope and outcome

## Goal

As Masha or Vitaliy, I want to keep the gift wishes of both of us (and our shared ones) on the site, with the
variants, prices and occasions, so that either of us can see on a phone what to buy for whom and by when, move a
wish out of the list once it is gifted, and check the other person's sizes («нюансы») without asking.

## In scope

- The «Подарки» section at `/gifts` replacing the «Раздел скоро появится» placeholder.
- «Список»: the wishes still wanted, as a table (a compact list on a phone) in a coloured block per person (Маша, Виталий, Вместе), filterable by person and category,
  each with priority, price or «цена не указана», link, variants, occasion with date, notes.
- Add, edit and delete a gift, including adding and removing its variants.
- Quick «Подарено» (moves the wish to «Подаренные» with today's date) and «Не актуально» (moves it too, marked).
- «Подаренные»: gifted and dropped wishes, newest gift first, with «Вернуть в список».
- «Настройки»: the «нюансы» (label and value per person) and the categories, each added, renamed/edited, deleted.
- The home page's Gifts summary (switcher entry «Подарки»): per person and in total, what is wanted, its known sum,
  how many have no price, how many are 🔥, how many were gifted and for how much; the top 🔥 wishes per person; the
  nearest occasions; the «нюансы» read-only with a way to the settings.
- Designed loading, empty and error states; edits that never overwrite a newer change; creates that never run twice.

## Out of scope

- Importing the sheet (a separate SQL script run by the coordinator).
- Reordering «нюансы» and categories by hand (they keep the order the server gives).
- Paging or search: at most a few hundred wishes.
- Reminders or notifications about occasions.
- Images or price tracking of links.
- Sign-in and per-person access (the site is intentionally open, API ADR 0015).

## Actors and external systems

- Masha, Vitaliy: read and write everything from a phone or a computer.
- Gifts API (`/api/gifts/*`): the source of truth; computes the order of the lists and the home summary.
- The other person editing at the same time: may change or delete the wish that is open.

## Constraints

- UI text in Russian; phone first (360–400px), then computer; light and dark schemes ([design rules](../../../../design.md)).
- Server state only through the typed client generated from the contract ([web ADR 0003](../../../../decisions/0003-typed-client-from-openapi.md)).
- Writes are rate limited like Expenses; a create carries an `Idempotency-Key`, an edit the row's version in
  `If-Match` ([web ADR 0007](../../../../decisions/0007-editing-in-dialogs-with-idempotency-and-versions.md)).
- At most 20 variants per gift; the server replaces the whole variant list on every edit.

## Non-functional requirements

| Requirement    | Value                                                                                                                                                   | How to check                                                              |
| -------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------- |
| Data on screen | up to ~300 wishes per tab, all rendered without paging; the sheet has 91 today                                                                          | page test with 300 stub rows renders and filters without paging           |
| Speed          | the list shows from one request per tab; filtering is instant (in memory, no request)                                                                   | page test: changing a filter sends no request                             |
| Network        | a weak phone connection: a repeated tap or retry never creates a second gift                                                                            | page test: two submits with the same data send the same `Idempotency-Key` |
| Accessibility  | icon-only buttons have `aria-label` and tooltip; text contrast 4.5:1 in both schemes; links to shops open in a new tab with `rel="noopener noreferrer"` | component tests query by role/label; design rules checklist               |

## Open product questions

None blocking. Decisions taken without asking are listed in [design/frontend.md, Assumptions](../design/frontend.md#assumptions).
