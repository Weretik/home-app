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
  The one exception is a signed amount, where `error` marks money going out (see «Signed amounts»).
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
- UI text is Russian, written plainly; buttons say what they do («Добавить», «Сохранить»).

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
  `serviceSections` go only to the side menu, and the home page links to them for phones.

## Pages

- Phone first: design at 360–400px, then widen. No horizontal page scroll.
- Spacing: `gap` of 2 (16px) on a phone, 3 (24px) on a computer, between blocks and between cards.
- Grids: one column on a phone, two from `sm`, three from `lg` where cards are small.
- A page starts with its title (`h4` as `h1`); the shell shows no extra page header.

## Sections with several pages and month lists

- A section with several pages shows them as tabs under the title (scrollable on a phone), one tab per built page,
  never a tab for a page that is not built ([ADR 0006](decisions/0006-section-pages-as-tabs-with-state-in-the-url.md)).
- A page that shows one month has a month picker next to the title: arrows with `aria-label` and tooltips around
  «Октябрь 2026», and «К текущему» when another month is open. The month and the filters are in the URL.
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
- Each row ends with a «⋮» button (`aria-label` «Действия: …» names the row) that opens «Изменить» and «Удалить».
  Delete always asks in a small dialog with the entry in one line and a red «Удалить».

## Signed amounts

- Money that goes in is shown in `primary.main` (blue) with «+», money that goes out in `error.main` (red) with «−»
  (a real minus sign). The sign is always written, so the colour is never the only hint. Zero is plain text.
- Only signed amounts use these colours (`savings-page/SignedAmount.tsx`); plain sums and balances stay
  `text.primary`. This is the one place `error` marks a direction instead of a problem.

## Cards

- **Card**: `background.paper`, radius 16px, 1px `divider` border, a soft shadow in the light scheme and none in the
  dark one (set in the theme; do not override per card).
- **Highlight card** (`features/home/HighlightCard.tsx` pattern): the top row of a dashboard, at most two to four.
  A gradient in `primary` or `secondary`, two soft white circles in the top right corner, an icon tile, a big value,
  a title and a caption. The whole card is a link to its section. In the dark scheme it uses the deep shade so white
  text stays readable.
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
