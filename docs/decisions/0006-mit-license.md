# 0006. The code is published under the MIT License

**Status:** accepted, 2026-11-27

## Context

The repository is public (the free GitHub Actions scheduler of the Telegram bot relies on that), but it had no
license, so by default nobody could legally reuse any of it. Vitaliy wants others to be able to take the code, as
is usual for a pet project, provided they credit him.

## Decision

The whole repository (API, web app, docs and workflows) is licensed under the MIT License: `LICENSE` at the root with
`Copyright (c) 2026 Vitaliy Tsiupin`, `"license": "MIT"` in `web/package.json`, and a License section in the
README.

Considered: Apache-2.0 (also requires credit, adds a patent grant and a NOTICE file; more ceremony than a family site
needs) and CC BY 4.0 (made for content, not code). MIT is the shortest permissive license whose one condition is to
keep the copyright notice, which is the credit he asked for.

## Consequences

- Anyone may use, copy, change and sell the code, also inside closed products, if every copy keeps the copyright
  notice and the license text. There is no warranty.
- Data is not code: the family's records live only in the database and the import scripts outside the repository;
  nothing personal is published by this license. Secrets were never committed (AGENTS.md).
- Third-party packages keep their own licenses.
