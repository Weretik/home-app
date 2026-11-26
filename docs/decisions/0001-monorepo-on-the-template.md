# 0001. One repository, built on fullstack-modular-template

**Status:** accepted, 2026-11-26

## Context

The site replaces several Google Sheets and Telegram bots (expenses, shopping list, to-dos, gift wishlist) and may
grow. Vitaliy already has [fullstack-modular-template](https://github.com/Weretik/fullstack-modular-template): a .NET
modular monolith with a React frontend, used as the base for real projects. A first version of this repository
simplified it (one API project, no layers); that was dropped on purpose so that growth does not mean a rewrite.

## Decision

- One repository with two halves that know nothing about each other except the OpenAPI document:
  - `api/`: the template's backend unchanged in structure: `BuildingBlocks`, `Host.Api`, modules of five projects
    (`Api`, `Application`, `Contracts`, `Domain`, `Infrastructure`), Mediator with a validation pipeline, `Result`
    errors, outbox, schema per module, architecture tests. Its decisions are in
    [api/docs/decisions](../../api/docs/decisions/README.md).
  - `web/`: the template's frontend structure (app, platform, features, a client generated from OpenAPI, React Query,
    Material UI) as one Vite app without Nx. Its decisions are in [web/docs/decisions](../../web/docs/decisions/README.md).
- Each half has its own workflow with path filters, so a change deploys only what it touches.

## Consequences

The same rules and tools as in the template and the work projects, so skills and checklists apply as they are. A new
area costs five projects and a migration, which is the price of keeping it ready to grow.
