# 002 — Web Push next to Telegram

- **Module:** `Notifications` (`api/src/Modules/Notifications`), schema `notifications`
- **Architecture source:** [architecture](../../../architecture/README.md), [rules](../../../rules.md), the module page
  [Notifications](../../../modules/notifications.md), [SDD 001 — Telegram bot](../001-telegram-bot/README.md)
- **Status:** accepted for delivery
- **Owner:** Vitaliy
- **Created:** 2026-11-27 · **Updated:** 2026-11-27
- **Decisions:** [ADR 0042](../../../decisions/0042-web-push-next-to-telegram.md) (standard Web Push with VAPID, no
  Firebase; subscriptions per member; a delivery per channel)
- **Frontend SDD:** [web platform/003 — installable app, offline data and queued writes](../../../../../web/docs/specs/platform/003-pwa-offline/README.md),
  [web notifications/002 — push settings](../../../../../web/docs/specs/notifications/002-push-settings/README.md)

On 2026-11-27 Vitaliy asked to make «Дім» a full PWA and to send everything the Telegram bot sends as push
notifications too, keeping Telegram for now («все что шлется в телеграм сделай пуш уведомлениями. Но телеграм оставь
пока что»). This SDD is the backend half: push subscriptions of each person's devices, the VAPID sender, and the tick
planning every digest for both channels.

Written from the backend SDD template (Weretik/obsidian `Templates/backend-sdd`), condensed to one page per part.

## State

- Specification: accepted
- Implementation: done (domain, migration, endpoints, sender, tick, tests, docs)
- Verification: see [tasks](tasks.md#delivery-readiness-filled-before-merge) and the [audit](audit.md)
- Blockers: the VAPID keys (`WebPush__PublicKey`, `WebPush__PrivateKey`) are App Service settings Vitaliy adds; without
  them push answers "not configured" and the tick plans Telegram only, exactly as before.

## Parts

- [Requirements: scope, rules and scenarios](requirements.md)
- [Design: model, schema, contract, encryption](design.md)
- [Tasks and traceability](tasks.md)
- [Final backend code audit](audit.md)

## Change history

- 2026-11-27: first version.
