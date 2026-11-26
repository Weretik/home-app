# 0007. Idempotency-Key for creates that must not happen twice

Status: accepted, 2026-11-16

## Context

Browsers double-submit, mobile networks retry, and users press the button again when nothing seems to happen. A
repeated order request must not create a second order.

## Decision

Creates of orders (and, in a real project, payments, stock movements, numbered documents and anything sent
outside) take an `Idempotency-Key` header. The key and a fingerprint of the request are saved with the result in
the same transaction under a unique index. A repeat returns the first result; the same key with different data
answers 409; a racing duplicate hits the unique index and is answered like a repeat.

## Consequences

- Retries are safe end to end; the client makes one key per attempt and reuses it when retrying.
- One extra indexed column per table that accepts keys.
