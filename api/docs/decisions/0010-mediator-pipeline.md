# 0010. Source-generated Mediator with a validation pipeline

Status: accepted, 2026-11-16

## Context

Endpoints should stay thin, and cross-cutting steps (logging, validation) should run the same way for every use
case without being repeated in each handler.

## Decision

Endpoints send commands and queries through the source-generated `Mediator` library. Two pipeline behaviors wrap
every handler: logging, then FluentValidation. A validation failure returns a `Validation` result before the
handler runs.

## Consequences

- Handlers contain only the use case; validation and logging are uniform.
- Source generation avoids reflection at runtime and reports missing handlers at compile time.
