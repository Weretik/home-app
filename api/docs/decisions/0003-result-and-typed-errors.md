# 0003. Business rules return Result with a typed Error

Status: accepted, 2026-11-16

## Context

Throwing exceptions for expected outcomes (a duplicate slug, an order that cannot be cancelled) hides the
possible failures from the method signature, costs a stack trace each time, and spreads try/catch and status
mapping across endpoints.

## Decision

Domain methods and handlers return `Result` or `Result<T>`. A failure carries an `Error` with a stable code, a
message and a type (`Validation`, `NotFound`, `Conflict`, `Forbidden`, `Unauthorized`). One mapping turns the type
into an HTTP status and an RFC 9457 problem response. Exceptions remain for bugs and outages and are handled once,
globally.

## Consequences

- Every expected failure is visible in the signature and tested like any other value.
- Clients get stable error codes they can rely on.
- Handlers check results explicitly; there is a little more code than with exceptions.
