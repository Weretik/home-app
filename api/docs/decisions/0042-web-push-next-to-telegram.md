# 0042. Web Push with VAPID next to Telegram, sent by the API itself

Status: accepted, 2026-11-27

Specification: [SDD notifications/002 — Web Push](../specs/notifications/002-web-push/README.md).

## Context

The site became an installable PWA (web ADR 0019, 0020). Vitaliy asked that everything the Telegram bot sends also
arrives as a push notification, while Telegram keeps working for now. Everything must stay free, and the API runs
on an App Service F1 without background threads (ADR 0034).

Options: Firebase Cloud Messaging (a Google project, a service account secret and the Firebase SDK in the web app),
a paid push service, or the standard Web Push protocol that every browser's push service accepts directly.

## Decision

- Standard Web Push: RFC 8030 delivery, RFC 8291 `aes128gcm` encryption, RFC 8292 VAPID authorization. The API posts
  to each subscription's endpoint itself; no Firebase project, no third-party account, no SDK. The encryption and the
  ES256 token use only `System.Security.Cryptography` (no NuGet package), checked against the RFC 8291 example.
- One VAPID key pair in the App Service settings `WebPush__PublicKey` and `WebPush__PrivateKey`; the browser gets the
  public key from `GET /api/notifications/push`. Push is off until both are set, and the tick then plans Telegram only.
- A push subscription belongs to the signed-in person (`Identity.Contracts`, ADR 0040), is unique by endpoint and is
  saved by an upsert (`PUT`), so the page may save it on every start without an `Idempotency-Key`.
- The tick plans every digest per channel: a `Delivery` row with `channel` and its own key (`push:` + the Telegram key)
  plus its outbox message, in one save, so the idempotency of ADR 0033 holds per channel. Push has no family group:
  common messages go to every person with a device.
- The outbox handler pushes to every device of the person, deletes devices the push service reports gone
  (`404`/`410`), and only retries when no device accepted the message.

## Consequences

- Free: the browsers' push services do not charge, and the payload stays under 4 KB (text cut to 1000 characters).
- On an iPhone push works only in the app added to the home screen (iOS 16.4+); the page says so.
- Rotating the key pair invalidates every subscription; each device turns push on again.
- A partial failure (one device refused for a transient reason, another accepted) does not retry the refused one;
  that device misses that message.
- Turning Telegram off later is a routing change only: the push channel does not depend on linked chats.
