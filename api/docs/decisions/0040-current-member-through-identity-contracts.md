# 0040. Other modules learn who is signed in through Identity.Contracts

Status: accepted, 2026-11-27

## Context

Until the Notes module, no module needed to know who sends a request: the Identity module closed every endpoint
([0037](0037-google-sign-in-with-server-sessions.md)) and nothing more. Notes records who wrote and who last changed a
note ([0039](0039-notes-module-model.md)). The session's claims are an Identity detail (claim names, the allowlist
order that says who is Vitaliy and who is Maria), and a module may use another only through its `Contracts`
([0002](0002-module-projects-and-contracts.md)).

## Decision

- `Identity.Contracts` exposes `ICurrentMember` with `SignedInMember? Get()`; `SignedInMember(string Email, string
  Name, HouseholdPerson? Person)`; `HouseholdPerson` is `Vitaliy` or `Maria`.
- `Identity.Api` implements it (scoped) over `IHttpContextAccessor` and the session principal; the person comes from
  the allowlist order. It answers `null` for the scheduler key and outside a request.
- Modules map `HouseholdPerson` to their own person enum in Application and never read claims or `HttpContext.User`
  themselves; an architecture test keeps `System.Security.Claims` out of every module except Identity.

## Consequences

- Any module can record or filter by the signed-in person without knowing how sign-in works.
- Background work (outbox handlers, the scheduler) has no member, so a module must not require one there.
