# 0005. One PostgreSQL database with a schema per module

Status: accepted, 2026-11-16

## Context

Modules must own their data, but separate database servers would cost money and complicate local development
for no benefit yet.

## Decision

All modules share one PostgreSQL database. Each module has its own schema (`identity`, `catalog`, `orders`), its
own `DbContext`, its own migrations and its own migration history table. There are no foreign keys between
schemas; a module keeps another module's ids as plain values.

## Consequences

- A module's tables can move to their own database without untangling constraints.
- Consistency across modules is eventual (through the outbox), not enforced by the database.
- One connection string and one backup for now.
