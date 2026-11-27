# Web Push — scope, rules and scenarios

## Goal

As Masha or Vitaliy, I want every reminder and summary the family bot sends in Telegram to also pop up as a
notification on my phone and computer where I turned it on, so that the site itself can remind me even without
Telegram, while Telegram keeps working.

## In scope

- Storing a browser push subscription (endpoint and keys) for the signed-in person; listing and removing the person's
  devices; a test push to the person's devices.
- Sending standard Web Push (RFC 8030) with VAPID (RFC 8292) and `aes128gcm` payload encryption (RFC 8291), from the
  API itself, with no third-party service account.
- Every scheduled digest of [SDD 001](../001-telegram-bot/requirements.md) (R-008…R-013) also planned as a push
  delivery for each person with at least one device, with the same windows, periods and idempotency.

## Out of scope

- Firebase Cloud Messaging accounts or SDKs (the browsers' own push services are used through the standard protocol).
- Interactive push actions (ticking a task from the notification); a tap opens the right page instead.
- Push for the bot's interactive answers (`/today`, buttons): they stay Telegram-only.
- Turning Telegram off (later, when Vitaliy decides).

## Actors

- Masha, Vitaliy: site users with one or more devices (phone PWA, computer browser).
- Browser push services (Google FCM endpoint, Mozilla autopush, Apple Web Push): accept encrypted messages.
- Scheduler: the same GitHub Actions cron ([ADR 0034](../../../decisions/0034-scheduler-github-actions-cron.md)).

## Business rules

| ID | Rule |
| --- | --- |
| R-101 | A push subscription belongs to the signed-in person (`Vitaliy` or `Maria`, from `Identity.Contracts`, ADR 0040). A member without a person (an allowlisted e-mail past the first two) cannot subscribe: `409` `notifications.push_no_person`. |
| R-102 | Saving a subscription is an upsert by its endpoint (unique): saving the same endpoint again updates its keys, device label, owner and `last_seen_at_utc`, so a repeat is harmless and needs no `Idempotency-Key`. The endpoint must be an absolute `https` URL of at most 1000 characters; `p256dh` is a base64url P-256 public key (65 bytes), `auth` a base64url 16-byte secret; the device label is trimmed to 120 characters. |
| R-103 | A person sees and removes only their own devices; removing another person's device or a missing one answers `404` `notifications.push_device_not_found`. |
| R-104 | Push is configured when both `WebPush:PublicKey` and `WebPush:PrivateKey` are set (App Service settings). The status endpoint returns `configured` and the public key the browser needs; without keys, saving and testing answer `409` `notifications.push_not_configured`, and the tick plans no push. |
| R-105 | The tick plans each due digest for Telegram (as before, when the bot is configured) and for push (when push is configured) for every person with at least one device. Push routing: personal messages to that person; common messages (common to-dos, shopping, expenses) to each person with a device; a gift reminder to the giver (`Together` → both). |
| R-106 | Every push delivery is a `delivery` row with channel `Push` and key `push:<kind>:<period>:<audience>` (Telegram keys stay as they were), written in the same transaction as its outbox message; a repeated or parallel tick plans nothing twice (R-007 holds per channel). |
| R-107 | A push message has a short title per kind, the digest text as plain text (no HTML, at most 1000 characters, cut with «…»), the page to open (`/todos`, `/shopping`, `/expenses/forecast`, `/gifts?gift=<id>`) and a tag per kind, so a newer digest of the same kind replaces the older one on the device. |
| R-108 | Sending goes to every device of the audience. A device the push service reports as gone (`404`, `410`) is deleted. The delivery counts as sent when at least one device accepted it or none is left; when every device failed for another reason, the outbox retries later. |
| R-109 | A test push from the site goes at once to all the person's devices and answers how many accepted it (`404` `notifications.push_no_devices` when there are none, `409` `notifications.push_failed` when every device refused). |
| R-110 | The private key never leaves the server and is never logged; the public key is not a secret (it is in every browser's subscription). |
| R-111 | All push endpoints need a signed-in member (closed by default, ADR 0037); the writes are rate limited (`PublicWrite`). The scheduler key may read the status (the deploy smoke test). |

## Acceptance scenarios

| ID | Scenario |
| --- | --- |
| SC-101 | Vitaliy turns push on on his phone: the subscription is saved for `Vitaliy`; the device appears in his list; Masha does not see it. |
| SC-102 | The phone saves the same subscription again on the next start: still one device, `last_seen` moved. |
| SC-103 | 08:00 Kyiv with Vitaliy's chat linked and his phone subscribed: one Telegram delivery and one push delivery of his morning to-dos; the 08:15 tick plans nothing new. |
| SC-104 | No Telegram token but push configured: the tick still plans push deliveries. No push keys: the tick plans Telegram only (SDD 001 unchanged). |
| SC-105 | A subscription the push service answers `410` for is deleted, and the delivery counts as sent. |
| SC-106 | «Надіслати тест» sends a push to every device of the person and reports the count. |
| SC-107 | The RFC 8291 example (Appendix A) encrypts to exactly the published message. |

## Non-functional requirements

| Requirement | Value | Check |
| --- | --- | --- |
| Free | no paid service: the API sends to the browsers' push services itself | ADR 0042 |
| Payload size | under 4 KB after encryption: text cut to 1000 characters | `PushTextsTests` |
| Secrets | `WebPush:PrivateKey` only from App Service settings; nothing in the repo | `ConfigurationRulesTests` |
| Delivery | at least once per device that is reachable; duplicates possible on a retry after a partial failure | R-108 |
