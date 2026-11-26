# 0008. The OpenAPI document is generated from the code

Status: accepted, 2026-11-16

## Context

A hand-written API description drifts from the real endpoints, and the frontend then calls endpoints that do not
exist or sends the wrong shape.

## Decision

Every build generates `docs/contracts/openapi.json` from the Minimal API endpoints. Each endpoint declares an
operation id, a summary and its responses. The document is committed; CI fails when it differs from the code, and
an architecture test checks the operation ids, summaries and number types. The frontend generates its TypeScript
types from this file.

## Consequences

- The contract is always the real API, and a breaking change shows up as a diff in review.
- Frontend type errors appear as soon as the contract changes.
- Endpoints carry a few more lines of metadata.
