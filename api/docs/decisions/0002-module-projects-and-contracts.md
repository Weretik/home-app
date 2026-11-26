# 0002. Five projects per module; other modules only through Contracts

Status: accepted, 2026-11-16

## Context

Folders and namespaces alone do not stop a class in one module from using an internal class of another one.
The compiler and the tests have to see the boundaries.

## Decision

Each module has five projects: `Domain`, `Application`, `Infrastructure`, `Api`, `Contracts`. A module may
reference another module's `Contracts` and nothing else from it. `Contracts` hold interfaces, DTOs and messages
and reference no other module. The host references only the modules' `Api` projects.

## Consequences

- A forbidden dependency does not compile, and the architecture tests catch the rest.
- More projects per module; adding a module follows a fixed recipe in the backend README.
- A contract change is visible in review because it lives in one small project.
