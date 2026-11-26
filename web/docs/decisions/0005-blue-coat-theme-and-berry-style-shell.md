# 0005. The «Синий плащ» theme, light/dark/system, and a Berry-style dashboard shell

Status: accepted, 2026-11-26

## Context

The site replaces Google Sheets and Telegram bots for two people and is used on phones and on a computer. Before any
feature is built it needs one look, one colour system and one frame with navigation, so that expenses, shopping,
to-dos and gifts all look alike. Vitaliy compared three palettes and four MUI dashboard templates (the official MUI
dashboard, Minimal, Berry, Mantis).

## Decision

- Colour theme «Синий плащ»: royal blue primary, crimson secondary, steel and silver neutrals, inspired by the mood of
  a game character (colours only). Headings in Exo 2, text in Inter, self-hosted through `@fontsource`.
- Light and dark colour schemes through MUI CSS variables (`colorSchemes`, `data-color-scheme` on `<html>`). The user
  chooses Светлая, Тёмная or Как в системе; the default is the system setting.
- The app frame follows the style of the Berry dashboard: a full-width header, a side menu under it that collapses to
  icons on a computer, a bottom bar with five sections on a phone, pages on a rounded tinted panel, bright gradient
  highlight cards on the dashboard. It is our own code, not a copy of the template.
- The rules every feature follows are in [design.md](../design.md).

## Consequences

- Features only use theme tokens; a palette change happens in one file.
- Every screen is checked in both colour schemes and at phone width.
- The bottom bar holds at most five main sections; more go to the side menu's «Сервис» group or into a section.
- Two font packages and `@mui/icons-material` are added to the bundle; icons are imported one by one, so only used
  icons are shipped.
