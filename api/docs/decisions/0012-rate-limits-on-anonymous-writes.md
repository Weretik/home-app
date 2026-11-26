# 0012. Rate limits on anonymous writes and sign-in

Status: accepted, 2026-11-17

## Context

`POST /api/orders` is open to anyone, and `POST /api/session/sign-in` accepts passwords. Identity locks an account
after repeated wrong passwords, but nothing stops one client from flooding the order endpoint or trying passwords
across many accounts. A gateway or CDN can limit traffic too, but the template must be safe without one.

## Decision

The API uses the built-in ASP.NET Core rate limiter (`AddRateLimiter`, `UseRateLimiter`). Endpoints opt in by name
with `RequireRateLimiting`; the names live in `BuildingBlocks.Api/RateLimiting/RateLimitPolicies`:

- `public-write`: anonymous creates such as placing an order, 10 requests per minute per client address.
- `sign-in`: 5 attempts per minute per client address.

Each policy is a fixed window partitioned by the client IP address, with no queue. The limits are configuration
(`RateLimiting:<Policy>:PermitLimit`, `:WindowSeconds`). A rejected request answers `429 Too Many Requests` with
`Retry-After` and a problem body with `code: too_many_requests`. Reads are not limited.

## Consequences

- One client cannot flood orders or guess passwords quickly; the limits can be tuned per environment without code.
- Behind a reverse proxy the API must read the client address from `X-Forwarded-For`, or every request shares the
  proxy's address and one limit (see [Deploy behind a reverse proxy](../../README.md#deploy-behind-a-reverse-proxy)).
- Counters live in memory, so with several API instances each instance counts on its own. When that matters, move
  the limits to the gateway or to a distributed store and write a new record.
- New anonymous writes must opt into `public-write`, and list `429` in their `Produces...` metadata.
