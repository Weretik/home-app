# Platform 002: Mobile view of every page

Kind: change · Status: in implementation (phase 1 done) · Backend counterpart: none (web only)

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
2. **Pages (R-M-002 – R-M-005):** the findings of the per-page review and their fixes, added here with that change.
