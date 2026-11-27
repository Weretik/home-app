# 001 — Identity module (Google sign-in for the family)

- **Module:** `Identity` (`api/src/Modules/Identity`), schema `identity`
- **Architecture source:** [architecture](../../../architecture/README.md), [rules](../../../rules.md), reference module
  [Notifications](../../../modules/notifications.md)
- **Status:** accepted for delivery
- **Owner:** Vitaliy
- **Created:** 2026-11-27 · **Updated:** 2026-11-27
- **Decisions:** [ADR 0037](../../../decisions/0037-google-sign-in-with-server-sessions.md) (supersedes
  [0015](../../../decisions/0015-expenses-open-until-identity.md) and closes 0009)
- **Frontend SDD:** [web/docs/specs/identity/001-sign-in](../../../../../web/docs/specs/identity/001-sign-in/README.md)

Until now the site and the API were open: anyone who found the API address could read and change the family's
expenses, gifts, to-dos and shopping list ([ADR 0015](../../../decisions/0015-expenses-open-until-identity.md)). On
2026-11-27 Vitaliy asked for sign-in («давай добавим … авторизацию»). Only Masha and Vitaliy may use the site; they
sign in with their Google accounts, and every read and write of every module needs a signed-in member. The Telegram
webhook and the scheduler keep their own secrets, and `/health` stays public for the deploy checks.

Written from the backend SDD template (Weretik/obsidian `Templates/backend-sdd`), condensed to one page per part.

## State

- Specification: accepted
- Implementation: done
- Verification: see [tasks](tasks.md#delivery-readiness-filled-before-merge)
- Blockers: Vitaliy creates the Google OAuth client and sets `Identity__Google__ClientId` and
  `Identity__AllowedEmails__0/1` in the App Service before the merge ([deployment](../../../../../docs/deploy.md#sign-in-with-google)).
  Without them nobody can sign in and every module answers `401`.

## Parts

- [Requirements: scope, rules and scenarios](requirements.md)
- [Design: model, schema, contract, authentication](design.md)
- [Tasks and traceability](tasks.md)
- [Final backend code audit](audit.md)

## Change history

- 2026-11-27: first version (Vitaliy: «давай добавим в отдельном треде авторизацию»).
