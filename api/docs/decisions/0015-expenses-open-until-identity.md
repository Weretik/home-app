# 0015. Expenses are readable without sign-in until the Identity module

Status: accepted, 2026-11-26 (temporary)

## Context

The site has no authentication yet ([0009](README.md) is still open). The Expenses module serves the family's real
incomes and expenses. Vitaliy decided to build the module and import the data first and to add sign-in as the next
separate feature.

## Decision

The Expenses read endpoints are anonymous for now. The site is not indexed and its addresses are not published, but
anyone who knows the API address can read the data. Writes stay closed: the only write is the keyed import
([0014](0014-one-time-sheet-import.md)).

## Consequences

- A known, accepted risk until sign-in exists. The Identity module must put every Expenses endpoint behind
  authorization in the same change that introduces it, and this record then becomes `superseded`.
- No create, edit or delete endpoints for expenses are added before sign-in.
