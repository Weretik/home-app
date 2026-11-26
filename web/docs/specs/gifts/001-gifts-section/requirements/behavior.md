# Gifts section — business rules and scenarios

## Business rules

### R-001 — What a gift is

A gift (a wish) is for one recipient: Маша, Виталий or Вместе. It has a name (1–200 characters after trimming), one
category, a priority («🔥 Очень хочу», «⭐ Хочу», «💭 Когда-нибудь»; «⭐ Хочу» for a new wish), and optionally: a price
(above zero, at most 10 000 000, up to two decimals), a link (an absolute http/https address up to 2000 characters),
an occasion (up to 100 characters) with a date (years 2000–2100), and notes (up to 2000 characters).

### R-002 — Variants

A gift has from 0 to 20 variants. Each variant has a link (required, same rule as the gift's link) and optionally a
title (up to 200 characters) and a price (same rule as the gift's price). Saving a gift saves exactly the variants
the form shows, in the order shown.

### R-003 — Unknown price

A price that is not given is unknown, never zero: it reads «цена не указана» and does not count in sums.

### R-004 — Where a wish lives

A wish is in «Список» while it is wanted. «Подарено» and «Не актуально» move it to «Подаренные»; «Вернуть в список»
brings it back. A gifted wish remembers the day it was gifted (today in Kyiv when nothing else is chosen); a wish that
is not gifted has no such day.

### R-005 — Order

«Список» shows 🔥 first, then ⭐, then 💭; within a priority, the nearest occasion first (wishes without an occasion
date last), then the oldest wish first. «Подаренные» shows the most recently gifted first (no date last), then the
newest wish first. The page keeps the server's order.

### R-006 — Filters and links

The lists can be narrowed to one recipient and to one category. The open tab and the filters are part of the link,
so a link, a reload or «Назад» opens the same view. A filter value that does not exist is ignored. Without a recipient
filter both tabs show the gifts in a block per person (Маша, Виталий, Вместе), each with the person's colour and
circle, the count and the sum of the known prices, so it is clear at a glance whose wish is whose. With a recipient
filter the rows are flat. On a computer the gifts are a table, on a phone a compact list (the Expenses pattern); each
row ends with «⋮».

### R-007 — No lost or doubled changes

A tap that is sent twice, or retried after a lost answer, creates one gift, «нюанс» or category. A change to an item
that the other person changed or deleted meanwhile is refused with a message, and the list reloads; nothing is
overwritten silently.

### R-008 — Deleting asks

Deleting a gift, a «нюанс» or a category asks first, naming the item. A category used by any gift cannot be deleted.

### R-009 — «Нюансы» are kept in the settings

A «нюанс» is a label and a value for one recipient (label up to 100, value up to 200 characters, both required). They
are added, changed and deleted only in the Gifts settings; everywhere else they are read-only.

### R-010 — Categories are editable

Categories have a unique name of up to 100 characters. They are added, renamed and deleted in the settings; a gift
always has one of them.

### R-011 — The home Gifts summary

The home page's «Подарки» summary shows, for Маша, Виталий, Вместе and in total: the number of wanted wishes, the sum
of their known prices, how many of them have no price, how many are 🔥, how many were gifted and their sum. It also
shows, per person, up to five 🔥 wishes (name, price or «цена не указана», link); up to five nearest occasions from
today; and every «нюанс», by person. It changes nothing.

### R-012 — Too many writes

When the server refuses a write because there were too many in a row, the person is told to wait a minute and retry;
the form keeps what was typed.

## Acceptance scenarios

### SC-001 — The wishlist opens in a block per person

**Covers:** R-001, R-003, R-005, R-006

Given wanted wishes for Маша, Виталий and Вместе
When the person opens «Подарки»
Then the «Список» tab is selected and the wishes show in three blocks «Маша», «Виталий», «Вместе», each headed by the
person's coloured circle, name, count and sum («2 подарка · ≈ 3 200 ₴»), with Виталий in `primary`, Маша in
`secondary` and Вместе in `info`
And within a block the order is the server's (🔥 first, nearest occasion first)
And each wish is one row (a table row on a computer, a compact row on a phone) with its name, category, priority, the
price («1 625 ₴», or «цена не указана»), the occasion with its date when set, a link button when it has a link, and
the number of variants when it has any

### SC-002 — Filter by recipient

**Covers:** R-006

Given the «Список» tab with wishes for every person
When the person picks «Маша» in the recipient filter
Then only Masha's wishes show as flat rows, without block headers, and the «Маша» button is tinted in her colour
And the link now opens the same filter after a reload
And picking «Все» shows the blocks again

### SC-003 — Filter by category, and an empty filtered list

**Covers:** R-006

Given the «Список» tab
When the person picks the category «Украшения»
Then only wishes in «Украшения» show, in the blocks of their recipients
And when no wish matches the filters, the list says «С этими фильтрами подарков нет» and offers «Сбросить фильтры»,
which shows every wish again

### SC-004 — An unknown filter in a link is ignored

**Covers:** R-006

Given a link with `recipient=Bob` and a category id that does not exist
When the person opens it
Then the list shows as without those filters, and nothing fails

### SC-005 — An empty wishlist

**Covers:** R-004

Given no wanted wishes
When the person opens «Список»
Then the page says «Список желаний пуст» with a hint how to add the first one, and «Добавить подарок» is available

### SC-006 — Loading and a failed load

**Covers:** R-006

Given the server is slow
When a tab opens
Then a progress indicator labelled for the tab shows instead of the list
And when the load fails, the page says «Не удалось загрузить» with the reason and «Повторить», which loads again

### SC-007 — Add a gift with variants

**Covers:** R-001, R-002, R-007

Given the «Список» tab with the recipient filter «Маша»
When the person taps «Добавить подарок»
Then a dialog «Новый подарок» opens (full screen on a phone) with «Для кого» preset to «Маша» and priority «⭐ Хочу»
When the person types a name, picks a category, adds two variants with links (one with a title and a price) and taps
«Сохранить»
Then «Сохранить» reads «Сохраняю…» and both buttons are disabled until the answer
And the dialog closes and the new wish shows in Masha's list with «2 варианта»

### SC-008 — A repeated add creates one gift

**Covers:** R-007

Given the add dialog with filled fields
When the first save fails with no answer and the person taps «Сохранить» again with the same data
Then the repeat carries the same `Idempotency-Key` as the first attempt
And when the person changes a field before retrying, a new key is used
And when the server says the key was used for other data, the form says so and the next attempt uses a new key

### SC-009 — The form checks its fields

**Covers:** R-001, R-002

Given the add or edit dialog
When the person taps «Сохранить» with an empty name, no recipient or no category, a price of 0 or «12,345», a link
without `http(s)://`, a variant without a link, or an occasion date outside 2000–2100
Then nothing is sent and each problem shows under its field («Введите название», «Выберите, для кого», «Выберите
категорию», «Цена больше нуля, до двух знаков после запятой», «Ссылка должна начинаться с http:// или https://»,
«Укажите ссылку варианта», «Дата между 2000 и 2100 годом»)
And when the server rejects a field, its message shows under the same field and the server's message above the fields
And «Добавить вариант» is unavailable once the gift has 20 variants

### SC-010 — Edit a gift and its variants

**Covers:** R-001, R-002, R-007

Given a wish with three variants
When the person opens «⋮» → «Изменить», removes the second variant, adds a new one, changes the price and saves
Then the request carries the wish's version and exactly the two kept variants plus the new one in the form's order
And the row shows the new price and «3 варианта»

### SC-011 — An edit that came too late

**Covers:** R-007

Given the person has a wish open in the edit dialog
And the other person changed or deleted it meanwhile
When the person saves
Then the dialog says «Подарок изменили в другом месте — список обновлён, откройте его снова» (or «Этого подарка уже
нет: его удалили в другом месте») and keeps the typed values
And the list behind it reloads

### SC-012 — Delete a gift

**Covers:** R-008

Given a wish in either tab
When the person opens «⋮» → «Удалить»
Then a small dialog asks «Удалить подарок?» naming the wish, with a red «Удалить» and «Отмена»
And «Удалить» removes it from the list; «Отмена» changes nothing

### SC-013 — «Подарено» moves the wish to the archive

**Covers:** R-004

Given a wanted wish in «Список»
When the person taps «Подарено» (the row's quick button on a computer, «⋮» → «Подарено» on a phone)
Then the button is disabled while the change is sent
And the wish leaves «Список» and shows in «Подаренные» with today's date («Подарено 26 ноября 2026»)
And the home summary counts it as gifted

### SC-014 — «Не актуально» moves the wish to the archive, marked

**Covers:** R-004

Given a wanted wish
When the person picks «⋮» → «Не актуально»
Then the wish leaves «Список» and shows in «Подаренные» marked «Не актуально», without a gifted date

### SC-015 — «Вернуть в список»

**Covers:** R-004

Given a gifted or dropped wish in «Подаренные»
When the person taps «Вернуть в список» (the row's quick button on a computer, in «⋮» on a phone)
Then it leaves «Подаренные» and shows in «Список» again, without a gifted date

### SC-016 — The archive tab

**Covers:** R-005, R-006

Given gifted and dropped wishes
When the person opens «Подаренные»
Then the gifts show in a block per person like «Список», the newest gifted first in a block, each gifted one shows
its date in «Итог», dropped ones are marked «Не актуально»
And the recipient, category and priority filters work like in «Список»; an empty archive says «Здесь появятся подаренные
подарки»

### SC-017 — Looking through variants

**Covers:** R-002

Given a wish with variants
When the person taps the variants count of its row («3» on a computer, «3 варианта» on a phone)
Then the list of variants opens under the row: each a link (its title, or the shop's address when there is none) with
its price when known, opening in a new tab
And tapping again folds it

### SC-018 — Tabs and settings keep the view

**Covers:** R-006

Given «Список» filtered to «Маша»
When the person switches to «Подаренные»
Then the recipient, category and priority filters stay
And the gear «Настройки» at the end of the tabs opens the settings, tinted while open, without filters

### SC-019 — «Нюансы» in the settings

**Covers:** R-007, R-008, R-009

Given the settings page
Then the «Нюансы» card shows the «нюансы» grouped by person, each as «label — value»
When the person adds «Кольцо — 18,5–19,0» for Маша
Then it shows under «Маша»
And «⋮» → «Изменить» changes it (with its version), «⋮» → «Удалить» asks and deletes it
And an empty card says «Нюансов пока нет» with what to add (ring size, clothes, shoes)

### SC-020 — Categories in the settings

**Covers:** R-007, R-008, R-010

Given the settings page
When the person adds the category «Книги», renames it to «Книги и комиксы», then deletes it
Then each change shows in the «Категории» card and in the category filter and the gift form
And deleting a category that a gift uses is refused: «Эта категория есть у подарков — сначала перенесите их в другую»
And a name that already exists is refused under the name field

### SC-021 — The home Gifts summary

**Covers:** R-003, R-011

Given wishes and «нюансы»
When the person opens Главная and picks «Подарки» in the switcher (or opens `/?s=gifts`)
Then four figure cards show Маша, Виталий, Вместе and «Всего»: the number of wishes, «≈ sum» of known prices with
«N без цены», the 🔥 count, and «подарено N на sum»
And «🔥 Очень хочу» lists up to five per person with price or «цена не указана» and a link button when there is a link
And «Ближайшие поводы» lists up to five upcoming occasions with date, wish and person
And «Нюансы» shows every «нюанс» by person with no edit controls, and «Изменить» opens the Gifts settings
And «Открыть подарки» opens `/gifts`

### SC-022 — The home summary when empty or failing

**Covers:** R-011

Given no wanted wishes and no «нюансы»
When the Gifts summary opens
Then the figure cards show zeros, and instead of the lists it says «Желаний пока нет» with «Открыть подарки»
And «Нюансы» says «Нюансов пока нет» with a link to the settings
And when the summary fails to load, it shows the reason and «Повторить»

### SC-023 — Too many writes in a row

**Covers:** R-012

Given any add, edit, delete or status change
When the server answers that there were too many writes
Then the person reads «Слишком много изменений подряд. Подождите минуту и повторите.» and what was typed stays

## Examples

| Input / state                                             | Expected observable result                                    |
| --------------------------------------------------------- | ------------------------------------------------------------- |
| price `null`                                              | «цена не указана»; not in «≈ sum»; counted in «N без цены»    |
| price typed «1 234,5»                                     | sent as `1234.5`; shown «1 234,50 ₴»                          |
| price typed «0» or «-5» or «12,345»                       | «Цена больше нуля, до двух знаков после запятой»              |
| link «shop.ua/item»                                       | «Ссылка должна начинаться с http:// или https://»             |
| variant with title empty, link `https://rozetka.com.ua/x` | shown as «rozetka.com.ua»                                     |
| 20 variants in the form                                   | «Добавить вариант» disabled, caption «Не больше 20 вариантов» |
| occasion «День рождения», date 2027-03-12                 | chip «День рождения · 12 марта 2027»                          |
| occasion date set, occasion empty                         | chip «12 марта 2027»                                          |
| status Gifted, giftedOn 2026-11-26                        | «Подарено 26 ноября 2026»                                     |
| status Dropped                                            | chip «Не актуально»                                           |
| `/gifts?recipient=Maria&category=<id>`                    | Masha's wishes of that category, flat rows, no block headers  |
| `/gifts?recipient=Bob`                                    | every wish, in a block per person                             |

## Deferred scenarios

- Reordering «нюансы» and categories by drag or arrows.
- Choosing another gifted date than today in the quick «Подарено» (the edit dialog does not change status either).
- Undo after «Подарено» other than «Вернуть в список».
- Reminders about upcoming occasions.
- Sorting or searching the list on the client.
