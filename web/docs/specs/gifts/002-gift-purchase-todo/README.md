# 002 — Gift purchase: «Кто дарит», «Купить до», the to-do and the link

- **App type:** React Web (Vite, MUI, React Query, React Router)
- **App:** `web/`, features `src/features/gifts` and `src/features/todos`
- **Architecture source:** [architecture](../../../architecture.md), [rules](../../../rules.md), [design rules](../../../design.md)
- **Status:** accepted for delivery
- **Owner:** Vitaliy · **Created:** 2026-11-26 · **Updated:** 2026-11-26
- **Backend SDD:** [api/docs/specs/gifts/002](../../../../../api/docs/specs/gifts/002-gift-purchase-todo/README.md)
  (gift ↔ to-do integration events, `giver`, `buyBy`, task `sourceKind` / `sourceId`) and
  [notifications 001](../../../../../api/docs/specs/notifications/001-telegram-bot/README.md) (the reminder)
- **Related:** [001 — Gifts section](../001-gifts-section/README.md), [todos 001](../../todos/001-todos-section/README.md),
  [notifications 001](../../notifications/001-telegram-page/README.md), [ADR 0013](../../../decisions/0013-deep-link-opens-a-record.md)

Vitaliy: «ставить дату в подарках, к какой дате купить подарок, для кого и кто кому дарит; он попадает в список дел
этого человека, напоминание за 2 недели со ссылкой на подарки; когда ставишь выполнено — подарок переносится в
подаренные». The backend creates the to-do and the reminder and moves the gift; the site only edits the two new fields,
shows them, and links the two sections both ways.

## State

- Specification: accepted · Implementation: done · Verification: local checks green, audit in
  [../../notifications/001-telegram-page/audit.md](../../notifications/001-telegram-page/audit.md) · Blockers: none

## Requirements and behaviour

| ID     | Behaviour                                                                                                                                                                                                                                                                                                                                                                                                            |
| ------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| FR-001 | The gift dialog gets a «Покупка» block under the occasion: «Кто дарит» select (не указано · Маша · Виталий · Вместе) and «Купить до» date. «Для кого» stays the existing recipient field. A hint under the block: «Если указать, кто дарит, и дату — появится задача в «Делах» и напоминание в Телеграме за 2 недели».                                                                                               |
| FR-002 | Checks: «Купить до» must be a year in 2000–2100 (as the occasion date). A date without a giver or a giver without a date is allowed, but the hint says the to-do appears only when both are set. When the gift has an occasion date and «Купить до» is empty, choosing a giver pre-fills nothing (no hidden magic); the user picks the date.                                                                         |
| FR-003 | `toGiftRequest` sends `giver` (`Maria`/`Vitaliy`/`Together` or `null`) and `buyBy` (`YYYY-MM-DD` or `null`); `fromGift` reads them back.                                                                                                                                                                                                                                                                             |
| FR-004 | A wanted gift with `buyBy` shows a line under its name in the table and the phone list: «🛒 купить до 12.12 · дарит Виталий» (giver part only when set); red when the date has passed. Archived gifts do not show it.                                                                                                                                                                                                |
| FR-005 | Deep link `/gifts?gift=<id>` (from Telegram and from a to-do) opens that gift's edit dialog once the list is loaded and removes the `gift` param (replace). If the gift is not in «Список», the page goes to `/gifts/archive?gift=<id>` and opens it there; not found there either → the param is dropped and a snackbar says «Подарок не найден» ([ADR 0013](../../../decisions/0013-deep-link-opens-a-record.md)). |
| FR-006 | Todos: a task whose `sourceKind` is `gift` shows «🎁» in its meta line (label «Задача из подарков») and, in the task dialog, a link «🎁 Открыть подарок» to `/gifts?gift=<sourceId>` (an in-app link).                                                                                                                                                                                                               |

## Design

- `gifts/gift-editing/PurchaseFields.tsx` (new), `gift-form.ts` (+ `giver`, `buyBy`), `GiftDialog` renders it.
- `gifts/display/formatting.ts` gets `formatPurchase(gift, today)` → `{ text, overdue } | null`; `display/labels.ts`
  gets `giverLabels` (the recipient labels plus «не указано»).
- `gifts/gift-rows/GiftName.tsx` shows the purchase line for active rows.
- `gifts/url-state/gift-link.ts` (pure: `resolveGiftLink(id, items, archived)` → `open` / `toArchive` / `missing`)
  and `use-gift-link.ts` (reads `?gift=`, waits for the list, opens the editor through `useGiftList`).
- `todos/task-row/GiftSourceLink.tsx` (dialog link) and a 🎁 mark in `TaskMeta`; types from the generated client
  (`TodoTaskItem.sourceKind`, `sourceId`).
- No new query keys: the fields travel with the existing gift and task requests; every Gifts write already refreshes
  the Gifts prefix. A gift save also changes Todos on the server (through the outbox, a few minutes later), so the
  gift dialog's save additionally invalidates `moduleKeys.todos` — cheap and keeps «Дела» fresh after the tick.

## Tasks and traceability

| ID     | Responsibility                                                  | Test                          | Covers         |
| ------ | --------------------------------------------------------------- | ----------------------------- | -------------- |
| TS-301 | `gift-form.ts`: giver/buyBy to and from the request, year check | Vitest unit                   | FR-001…FR-003  |
| TS-302 | `formatPurchase`                                                | Vitest unit                   | FR-004         |
| TS-303 | `resolveGiftLink`                                               | Vitest unit                   | FR-005         |
| TS-304 | Gifts page: deep link opens the dialog; dialog sends the fields | Testing Library, mocked fetch | FR-001, FR-005 |
| TS-305 | Todos: gift mark and the dialog link                            | Testing Library               | FR-006         |

## Readiness

- [x] Behaviour defined; contract fields from the backend SDD.
- [x] Delivery: typecheck, lint, tests, structure check green locally (CI pending).
