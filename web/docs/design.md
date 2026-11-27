# Design rules

How the app looks and how screens are laid out. Every feature follows these rules; a change to them is a decision
([ADR 0005](decisions/0005-blue-coat-theme-and-berry-style-shell.md)) and updates this file in the same change.

## Theme «Синий плащ»

The palette takes its mood from a game character in a blue coat: royal blue, steel and silver, with crimson as the
second colour. Colours and mood only: no game art, logos or fonts.

| Token                | Light     | Dark      | Used for                                                      |
| -------------------- | --------- | --------- | ------------------------------------------------------------- |
| `primary.main`       | `#1f45b8` | `#5b82ff` | the main action, the active menu item, links, focus           |
| `secondary.main`     | `#b3122e` | `#ff5468` | the second accent: the second highlight card, a second series |
| `background.paper`   | `#ffffff` | `#151a25` | header, side menu, cards                                      |
| `background.default` | `#eef2f6` | `#0b0e15` | the page panel behind the cards                               |
| `text.primary`       | `#121826` | `#e8ecf5` | text                                                          |
| `text.secondary`     | `#586174` | `#97a1b5` | captions, inactive menu items                                 |
| `divider`            | `#dfe3ec` | `#242b3a` | card borders, separators                                      |

- The palette lives only in `src/app/theme/palette.ts`; the theme in `src/app/theme/theme.ts`.
- **Components never use literal colours.** Use palette tokens (`color="primary"`, `sx={{ color: 'text.secondary' }}`)
  or the CSS variables (`theme.vars.palette.primary.mainChannel` for a tint:
  `` `rgba(${theme.vars.palette.primary.mainChannel} / 0.12)` ``). The only exception is the white overlay on the
  highlight cards.
- Status colours (`success`, `warning`, `error`, `info`) are MUI's defaults and mean only status, never decoration.
  Three exceptions: the kind mark of a «Про нас» note (see «Note cards»), a signed amount, where `error` marks money going out (see «Signed amounts»), and «Вместе», whose
  person colour is `info` (see «Person colours»).
- Tints: `0.08` for hover, `0.12` for a selected item or an icon tile, `0.16` for a pressed state.

## Light, dark, system

- Both colour schemes are always designed and checked. A screen is done only when it reads well in both.
- The switch in the header offers Светлая / Тёмная / Как в системе. **The default is Как в системе.** MUI stores the
  choice in `localStorage` (`mui-mode`) and sets `data-color-scheme` on `<html>`.
- Scheme-specific styling goes through `theme.applyStyles('dark', { ... })`, never through a check of the mode in code.

## Typography

- Headings (`h1`–`h6`): **Exo 2**, weight 700. Body and UI: **Inter**. Both are self-hosted through `@fontsource`
  (Cyrillic and Latin subsets), imported in `src/main.tsx`; no font CDN.
- A page title is `<Typography variant="h4" component="h1">`; a card title is `subtitle1` with weight 600 as `h2`.
- Numbers that line up (amounts, counts) use `fontVariantNumeric: 'tabular-nums'`.
- UI text is Ukrainian ([glossary](specs/platform/001-ukrainian-interface/glossary.md)), written plainly; buttons say what they do («Додати», «Зберегти»).

## Layout (the shell)

The shell is in `src/app/shell/`, styled after the Berry dashboard (our own code, not the template's).

```
Computer (md, 900px and wider)              Phone (below md)
+------------------------------------+      +------------------+
| logo  [menu]                 theme |      | logo       theme |
+--------+---------------------------+      +------------------+
| side   |  ( rounded tinted panel ) |      | ( page panel )   |
| menu   |   page title              |      |  page title      |
|        |   [ card ] [ card ]       |      |  [ card ]        |
|        |                           |      |  [ card ]        |
+--------+---------------------------+      +------------------+
                                            | bottom bar (5)   |
                                            +------------------+
```

- **Header**: full width, `background.paper`: the logo (links home), the button that collapses the side menu
  (computer only), the theme switch on the right.
- **Side menu** (computer): under the header, 260px wide, groups with a caption («Разделы», «Сервис»). The header
  button collapses it to 84px of icons with tooltips; the choice is remembered (`home.side-nav.collapsed`).
- **Bottom bar** (phone): the five main sections with icon and label; the active one has a tinted pill behind the
  icon. It respects the safe area at the bottom.
- **Page panel**: `background.default` with rounded top corners. Content starts at the left edge next to the side
  menu (never centred, which leaves a gap on wide screens) and stops at 1600px.
- Sections are declared once in `src/app/shell/sections.ts`. `mainSections` (at most five) go to both menus;
  `moreSections` («Про нас») and `serviceSections` go only to the side menu, and the home page links to them for
  phones.

## Pages

- Phone first: design at 360–400px, then widen. No horizontal page scroll.
- Spacing: `gap` of 2 (16px) on a phone, 3 (24px) on a computer, between blocks and between cards.
- Grids: one column on a phone, two from `sm`, three from `lg` where cards are small.
- A page starts with its title (`h4` as `h1`); the shell shows no extra page header.

## Sections with several pages and month lists

- A section with several pages shows them as tabs under the title (scrollable on a phone), one tab per built page,
  never a tab for a page that is not built ([ADR 0006](decisions/0006-section-pages-as-tabs-with-state-in-the-url.md)).
- A section's settings are a page of their own, reached from a gear icon button («Настройки», tooltip and
  `aria-label`) at the end of the tab row, not a tab; it is tinted while the settings are open, and the settings page
  has no month picker.
- A goal is shown as a ring: a determinate `CircularProgress` in `primary` with round ends over a full track tinted
  `0.12`, the whole percent in the middle, and «Сейчас», «План», «Осталось» beside it (or «План выполнен!» with a check
  icon). Without a goal the card says so and links to the settings.
- A page that shows one month has a month picker next to the title: arrows with `aria-label` and tooltips around
  «Октябрь 2026», and «К текущему» when another month is open. The month and the filters are in the URL.
- A list that can show any days (Расходы) has a «Период» button beside its filters. On, it becomes two date fields
  «с» and «по» (the open month to start) with «К месяцу» to go back; the month picker hides meanwhile. A wrong period
  says why under the fields («Выберите обе даты», «Дата «с» позже даты «по»», «Период не длиннее 366 дней») and the
  list loads nothing.
- Amounts are formatted as hryvnias with two decimals («95 024,00 ₴»), right-aligned and in tabular figures.
- A month list starts with its filters and the add button («Добавить расход») on one row, then the list in a card.
  Below `md` the list is compact (title, a caption line, the amount on the right); from `md` it is a small table.
  More rows load with «Показать ещё». Lists show no month total card; totals belong to a summary page.
- An empty month says «За этот месяц записей нет»; an empty filtered list offers «Сбросить фильтр». An error shows
  the reason and «Повторить».

## Editing entries

- Add and edit happen in a dialog (`entry-dialogs/EntryDialog.tsx`): a title («Новый расход», «Изменить расход»),
  the fields one per row, «Отмена» and «Сохранить» at the bottom. Below `sm` the dialog is full screen. While saving,
  «Сохранить» reads «Сохраняю…» and both buttons are disabled.
- Fields check themselves on «Сохранить»: the message sits under the field («Введите сумму», «Выберите категорию»);
  the API's field errors go under the same fields, its message in an error `Alert` above them.
- Amounts are typed as positive numbers with up to two decimals («1 234,5» works); the phone shows the decimal
  keypad. Where the direction matters (the jar), it is a separate choice, not a minus sign.
- A choice that changes how an entry counts is a checkbox with a helper line under it saying what it does («Не в
  лимиты» — «Считается в итогах, но не в лимитах и не переносится»); a list marks such rows with a small outlined
  chip («вне лимитов») after the title.
- Each row ends with a «⋮» button (`aria-label` «Действия: …» names the row) that opens «Изменить» and «Удалить»;
  a row with a step of its own lists it first («Записать как расход»), whose dialog names its button by the step
  («Записать»). Delete always asks in a small dialog with the entry in one line and a red «Удалить».

## Gift lists

- The Gifts tabs follow the Expenses pattern: a table from `md` (`gifts/gift-table`), a compact list below it
  (`gifts/gift-list`), each row ending with «⋮». No cards.
- Table columns: Подарок (name 600, the shop link icon, notes clamped to two lines), Варианты (right after the name:
  the count; a click opens the variant links in a row under the gift), Категория, Приоритет («Список» only), Цена
  (right, tabular), Повод (occasion and date) or Итог («Подарено 26 ноября 2026», «Не актуально»), then the quick status button
  («Подарено» or «Вернуть в список», an icon with a tooltip) and «⋮» (Не актуально, Изменить, Удалить).
- Phone row: the name and the price on the first line, the category under it, then small outlined chips (priority or
  how it ended, the occasion) and «N вариантов» unfolding the links; every action is in «⋮».
- A priority chip is neutral (outlined); «🔥 Очень хочу» is only bolder. Crimson belongs to Маша (see below).
- A gift price drops «,00» («1 625 ₴») and keeps real kopecks («1 234,50 ₴»); «цена не указана» in
  `text.secondary`, never 0. A person's group sum is rounded («≈ 4 500 ₴»).

## Person colours

- Each person has one theme colour everywhere in Gifts and in the gifts summary: **Виталий `primary`**, **Маша
  `secondary`**, **Вместе `info`**. The mapping lives in one file per feature (`gifts/display/recipient-tones.ts`;
  the home summary keeps its own copy, because features never import each other). Never a literal colour.
- The mark is a circle in `<tone>.main` with `<tone>.contrastText`: «В», «М», or the two-people icon for «Вместе»
  (`aria-hidden`; the name is always written next to it, so colour is never the only hint).
- A person's group: a header with the circle, the name (`subtitle1` 700) and «3 подарка · ≈ 4 500 ₴» on a 0.08 tint
  of the colour, and a 4px line in the colour on the left of the header and of every row in the group (table: a
  `th` row per group in its own `tbody`; phone: a bordered block per person, 16px apart).
- Groups appear while no person is picked; with a person picked the rows are flat and keep only the left line.
- The «Для кого» filter buttons carry the same circle; a picked person's button is tinted 0.12 in their colour with a
  border in it, and its text stays `text.primary` (600): `info.main` text on a light tint is under 4.5:1.

## Signed amounts

- Money that goes in is shown in `primary.main` (blue) with «+», money that goes out in `error.main` (red) with «−»
  (a real minus sign). The sign is always written, so the colour is never the only hint. Zero is plain text.
- Only signed amounts use these colours (`savings-page/SignedAmount.tsx`); plain sums and balances stay
  `text.primary`. This is the one place `error` marks a direction instead of a problem.
- The «Копилка» balance card puts the month's four figures in tinted tiles in a 2×2 grid: under the balance on a phone,
  beside it from `md` up, so the wide card has no empty half next to «План копилки».

## Summaries on the home page

- The home page shows one section's summary at a time. A `ToggleButtonGroup` («Сводка») under the title switches them;
  it scrolls sideways on a phone instead of wrapping. Sections without a summary yet stay in it, disabled, with a
  small «скоро» ([ADR 0008](decisions/0008-home-page-as-a-registry-of-section-summaries.md)).
- A month summary has the month picker on the left and a link to the section's month («Открыть расходы») on the right.
- A month with nothing in it still shows its figure cards (the jar's balance is real) and replaces the rest with the
  empty state «За этот месяц записей нет».
- Limits show how full they are with a thin bar in status colours: `success` under 80 %, `warning` from 80 % to
  100 %, `error` past 100 %. A negative «Остаток» is `error`. «Можно в день» is «—» when the period is not running.
  The last row «ИТОГО» is bold. Below `md` the table becomes a list: the name and the percent, the bar, then «X из
  Y · остаток Z · в день W».
- A limit row notes, under its figures, the overspend carried in («перенос N»), what moves on to the next period
  («→ след. мес. N») and what is planned («план N»), each only when it is not zero, in `text.secondary`; the notes
  wrap between each other, never inside an amount.
- A figure card may have a second caption line for something worth knowing that is not always there («Вне лимитов:
  2 траты на 3 500 ₴»); it is left out, not shown as zero, when there is nothing.
- Amounts in summary tables and lists have kopecks like every list; only the figure cards round them.

## Note cards

- «Про нас» shows notes as cards in the card grid, grouped under a `subtitle2` heading per category («🍕 Їжа · 3»,
  «Без категорії» last).
- The kind mark at the top of a card: the icon in `success` (heart, «Любить»), `error` (crossed circle, «Не
  любить») or `info` (lightbulb, «Факт») on a 0.12 tint of the same colour, the word in `text.primary` 600, so the
  contrast holds in both schemes and the colour is never the only hint. These colours mark the kind only there.
- Then ⭐ when important, and in «Усі» the person circle (Віталій `primary`, Маша `secondary`, with an `aria-label`);
  the title `subtitle1` 600, the place as a small outlined chip with a shop icon, the description clamped to three
  lines, and «Маша · 27.11 · змінив Віталій» in `text.secondary`. The whole card opens the dialog.

## Cards

- **Card**: `background.paper`, radius 16px, 1px `divider` border, a soft shadow in the light scheme and none in the
  dark one (set in the theme; do not override per card).
- **Highlight card**: a gradient card in `primary` or `secondary` with two soft white circles in the top right corner,
  an icon tile, a big value, a title and a caption; the whole card is a link. Kept as a pattern for a dashboard's top
  row (at most two to four); no screen uses it now. In the dark scheme it uses the deep shade so white text stays
  readable.
- **Figure card** (`home/expenses-summary/figures/KpiCard.tsx`): the top row of a summary, one to four in a row (one
  column on a phone, two from `sm`, four from `lg`). An emoji and a title in `text.secondary` (the emoji is
  `aria-hidden`), the rounded amount as `h4` («95 024 ₴», no kopecks), one caption line, an optional thin progress bar.
  The value is `text.primary`, except a result that is good or bad: `success` from zero up, `error` below.
- **Section card**: a light card with a tinted icon tile, a title, one line of caption and a chevron; a link.
- Empty and loading states are designed, not blank: say what will appear and how to add the first item.

## Icons and shape

- The site icon is a white house with a crimson door on the blue gradient tile: `web/public/favicon.svg` is the
  source, used by the header logo too. The PNG sizes, `favicon.ico` and `site.webmanifest` are made from it; redraw
  them all together when it changes.
- Icons from `@mui/icons-material`, the **Outlined** set; `fontSize="small"` inside compact controls.
- Radius: 12px by default (theme `shape`), 16px cards, 12px icon tiles and menu items.
- Interactive elements look interactive: a hover tint, a visible keyboard focus, a pointer.

## Accessibility

- Every icon-only button has an `aria-label` and a tooltip.
- Text contrast is at least 4.5:1 in both schemes; check new colour pairs before adding them.
- Respect `prefers-reduced-motion`: motion only for short state changes (menu width, hover).
