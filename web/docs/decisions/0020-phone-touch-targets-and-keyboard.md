# 0020. Phone: 44px tap targets in the theme, rows that never squeeze, room for the keyboard

Status: accepted, 2026-11-27

## Context

The site is used mostly as an installed app on Android phones. A review of every page at 360 and 412px
([spec platform/002](../specs/platform/002-mobile-ux/README.md)) found no sideways scroll, but: row «⋮» buttons of
30px and icon buttons of 36px, buttons 37px high; phone list rows that put a comment in a 150px column next to the
amount; spending limits on «Налаштування» squeezed to a few characters a line next to their amounts; two dialogs that
were not full screen; scroll arrows of the section tabs taking room even when disabled; filters that took a third of
the screen; and Chrome on Android leaves the layout viewport as it is when the keyboard opens, so the actions at the
bottom of a full-screen dialog can sit under the keyboard.

## Decision

- **Tap targets in the theme**, not per screen (`app/theme/phone-overrides.ts`), only under `@media (pointer: coarse)`
  so a computer keeps the compact look: icon buttons and toggle buttons at least 44×44px; buttons at least 40px with an
  invisible 44px hit area (`::after`); clickable chips and small icon buttons inside fields and chips keep their look
  and get the same hit area; list item buttons at least 48px.
- **Tabs on a phone** have no 90px minimum width, and a disabled scroll arrow takes no room.
- **Phone rows**: the title, the amount and «⋮» share the first line; details (date, who, comment) take the whole width
  under them (`expenses/month-view/CompactRow.tsx`). Rows with more to say (a spending limit) stack: name with «⋮»,
  then the amount and captions, then chips. A name is never put in a narrow column beside other content.
- **Keyboard**: the viewport meta says `interactive-widget=resizes-content`, so the page (and a full-screen dialog
  with its actions) shrinks to the space above the keyboard; the bottom bar hides while a text field has focus.
- Every add or edit dialog is full screen below `sm`.

## Consequences

- New screens get the touch sizes for free; a screen does not set its own minimum sizes for touch.
- A computer with a touch screen gets the touch sizes too, which is what its user needs.
- `:has()` hides the bottom bar while typing; a browser without it keeps the bar above the keyboard, which still works.
