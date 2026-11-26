# 0006. Outbox delivered by a scheduled HTTP tick

Status: accepted, 2026-11-16

## Context

After an order is placed, staff must be notified. Doing it inside the request makes checkout slow and fragile;
doing it after the commit can lose the message if the process stops. A message broker or a background job server
adds infrastructure, and a background thread prevents the API from scaling to zero on a serverless platform.

## Decision

Write messages to an outbox table in the same transaction as the data. An external scheduler (for example Cloud
Scheduler) calls `POST /internal/outbox/tick` every few minutes with a shared key. Each tick claims due messages
with a conditional `UPDATE`, runs their handlers, and retries failures with growing delays up to five attempts.

## Consequences

- A message is stored if and only if the data is stored; nothing is lost on a crash.
- No broker and no background thread; the API can scale to zero.
- Delivery is delayed by up to one tick interval, and handlers must be safe to run twice.
- Moving to a broker later only changes the delivery step.
