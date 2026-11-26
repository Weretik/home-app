# 0001. A modular monolith, not microservices

Status: accepted, 2026-11-16

## Context

A small team starts a product whose boundaries will still move. Microservices would add network calls,
distributed transactions, several deployments and a message broker from day one. A classic layered monolith is
cheap to run but lets every part reach into every other part, so it gets harder to change and impossible to split.

## Decision

Build one deployable API made of modules. Each module owns its data and its use cases and exposes a small public
surface. Modules run in one process and share one database, but behave as if they could not see each other.

## Consequences

- One deployment, one database, in-process calls: cheap to run, easy to debug, transactions stay local.
- Boundaries are real and tested, so a module can be extracted into a service later without rewriting it.
- Discipline is required: the architecture tests, not good intentions, keep modules apart.
