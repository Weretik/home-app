# Platform 002: Mobile view of every page

Kind: change · Status: implemented, audited ([audit](audit.md)) · Backend counterpart: none (web only)

## Goal

The site is used mostly as an installed app (PWA) on Android phones, 360–412px wide. Every page must be fully usable
there with one thumb: every page reachable from the menu, nothing cut off or squeezed, tap targets of at least 44px,
forms that fit with the keyboard open. Vitaliy asked for a deep review of every page and the fixes (2026-11-27).

## Requirements

| Id      | Requirement                                                                                             |
| ------- | ------------------------------------------------------------------------------------------------------- |
| R-M-001 | On a phone every page is reachable from the bottom bar: the five main sections plus «Ще» with the rest. |
| R-M-002 | No page scrolls sideways at 360px; long names wrap, they are never squeezed into a narrow column.       |
| R-M-003 | Tap targets (row actions, icon buttons, chips that act) are at least 44px high on a phone.              |
| R-M-004 | Every add/edit dialog is full screen below `sm`; its actions stay visible above the keyboard.           |
| R-M-005 | Content never hides under the bottom bar or the system areas (safe areas).                              |

## Scenarios

| Id       | Scenario                                                                                  | Test                                 |
| -------- | ----------------------------------------------------------------------------------------- | ------------------------------------ |
| SC-M-001 | On a phone, a tap on «Ще» opens a sheet listing «Про нас», «Стан», «Телеграм», «Профіль». | `app/shell/frame/AppLayout.test.tsx` |
| SC-M-002 | With «Про нас» open, «Ще» is the selected bottom-bar item.                                | `app/shell/frame/AppLayout.test.tsx` |
| SC-M-003 | A tap on a page in the sheet opens it and closes the sheet.                               | `app/shell/frame/AppLayout.test.tsx` |

## Phases

1. **Menu (R-M-001):** «Ще» in the bottom bar ([web ADR 0019](../../../decisions/0019-phone-more-sheet-in-the-bottom-bar.md)).
   The shell folder split into `frame/` and `navigation/` to stay within six files per folder.
2. **Pages (R-M-002 – R-M-005)** ([web ADR 0020](../../../decisions/0020-phone-touch-targets-and-keyboard.md)): the
   findings below and their fixes.

## Findings (review at 360 and 412px, 2026-11-27)

Checked in a phone-sized browser (touch, 360×760 and 412×760) on every route with long generated texts, measuring
sideways scroll, elements past the right edge, tap targets under 40px, clipped text and text squeezed into a narrow
column; then the pages read in code.

| Id      | Page                                                   | Finding                                                                                       | Fix                                                                   |
| ------- | ------------------------------------------------------ | --------------------------------------------------------------------------------------------- | --------------------------------------------------------------------- |
| F-M-001 | all                                                    | «Про нас», «Стан», «Телеграм», «Профіль» not in the phone menu                                | «Ще» in the bottom bar (phase 1)                                      |
| F-M-002 | all                                                    | No page scrolls sideways at 360 or 412px                                                      | none needed                                                           |
| F-M-003 | Бюджет (all tabs), Подарунки, Налаштування             | Row «⋮» buttons 30×30px; icon buttons 36×36px; buttons 37px high                              | Theme touch sizes under `(pointer: coarse)`                           |
| F-M-004 | Покупки history and settings, Справи, Про нас settings | Icon buttons (return, delete, up, down, edit) 36×36px                                         | Theme touch sizes                                                     |
| F-M-005 | Бюджет: Витрати, Доходи, Скарбничка, Прогноз           | The comment sits in a ~150px column beside the amount, wrapping into many lines               | `CompactRow`: details across the whole row                            |
| F-M-006 | Бюджет › Налаштування                                  | A limit's name and chips squeezed to a few characters per line beside its amount and captions | Limit row stacks: name with «⋮», amount and captions, chips           |
| F-M-007 | Справи and Про нас settings                            | «Нова категорія» dialog is a small centred box on a phone                                     | Full screen below `sm`, like every other dialog                       |
| F-M-008 | all full-screen dialogs                                | Android keeps the layout height when the keyboard opens, so «Зберегти» can hide under it      | `interactive-widget=resizes-content`; bottom bar hides while typing   |
| F-M-009 | Про нас, Справи, Подарунки, Покупки, Бюджет            | Section tabs: a 90px minimum width and an empty space for the disabled left arrow             | Phone tabs without minimum width; disabled arrows take no room        |
| F-M-010 | Справи, Про нас                                        | Filters one per row: about 260px before the first task or note                                | Category and term (Справи), category and search (Про нас) two per row |
| F-M-011 | Профіль                                                | The «Про нас» caption squeezed beside «Відкрити»; long name and e-mail cut with «…»           | The caption wraps the button under it; name and e-mail wrap           |
| F-M-012 | Подарунки list row                                     | Name squeezed beside the price                                                                | Done in the Gifts change (#34), not here                              |

Kept as they are: category rows in settings (emoji, name, two 44px buttons: real names are short); the home page
figure cards in one column (two columns would cut the amounts); the shopping row (the checkbox is the main action and
already 52px).
