# Audit: Platform 002, mobile view of every page

Frontend code audit (`frontend-code-audit`) of both phases against [web rules](../../../rules.md), the
[design rules](../../../design.md) and this [spec](README.md). 2026-11-27.

| Check                             | Result                                                                                                                           |
| --------------------------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| Folders: at most 6 code files     | `app/shell` split into `frame/` and `navigation/` by meaning; `expenses/month-view` 6 files; checked by `architecture:structure` |
| Component ≤150 lines, module ≤200 | MoreSheet 75, BottomNav ~105, CompactRow 65, LimitRow ~95, phone-overrides ~80                                                   |
| Features do not import each other | `CompactRow` lives in Expenses and is used only there; the theme rules are app-wide                                              |
| No literal colours                | none added; tints through `theme.vars`                                                                                           |
| Accessibility                     | «Ще» is a button with `aria-haspopup` / `aria-expanded`; the sheet list is named; hit areas do not change accessible names       |
| Contract, data, idempotency       | no API change                                                                                                                    |
| Tests                             | SC-M-001 – SC-M-006 covered; full suite, lint, typecheck, format and structure pass                                              |
| Docs                              | design.md «Phone and touch», architecture.md, ADR 0019 and 0020                                                                  |

Findings: none open. Residual: the `:has()` rule that hides the bottom bar while typing is not testable in jsdom; it
is a style only, checked in the browser at 360px.
