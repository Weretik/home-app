# 0019. The phone's bottom bar ends with «Ще»: a sheet with every other page

Status: accepted, 2026-11-27

## Context

The bottom bar on a phone held the five main sections only. «Про нас», «Телеграм», «Стан» and «Профіль» were
reachable on a phone only through links on the home page or the profile page, so on the installed app (PWA) Vitaliy
could not find «Про нас» at all. [0017](0017-notes-section-person-tabs-me-first.md) kept the bar at five for that
reason; this record replaces that part of it.

## Decision

- The bottom bar shows the five main sections and a sixth item «Ще» (three dots). «Ще» is a button, not a link: it
  opens a bottom sheet (`Drawer anchor="bottom"`, rounded top corners, safe area at the bottom) listing every page
  that is not in the bar, in the side menu's order: «Про нас», «Стан», «Телеграм», «Профіль»
  (`phoneMoreSections` in `app/shell/navigation/sections.ts`).
- While one of those pages is open, «Ще» is the selected item of the bar; a tap on a page in the sheet opens it and
  closes the sheet.
- The computer keeps the side menu as it is; «Профіль» stays in the header's member menu there.

## Consequences

- Every page is two taps away on a phone, wherever the member is; the home page links stay as a shortcut.
- Six items at 360px leave each about 60px, enough for the longest label «Подарунки» at 11px. A seventh page goes
  into the sheet, never into the bar.
