# 0029. Every backend rule is checked by a test, or marked review-only with a reason

Status: accepted, 2026-11-26

## Context

[0011](0011-folder-rules-enforced-by-tests.md) made the folder rules fail the build. The other rules in
[rules.md](../rules.md) and `AGENTS.md` were partly tested and partly only written down: one type per file, wrapper and
empty folders, central package versions, the solution layout, problem responses, `Idempotency-Key` and `If-Match` on
writes, rate limits, queries that never write, one save per command, schema ownership, the migration snapshot, scope
validation in the host, secrets in committed settings, the ADR and SDD indexes. Code written quickly, by people or by
AI agents, drifts from rules that nothing checks; the audit of SDD 005
([audits/2026-11-26-expenses-budget-forecast.md](../audits/2026-11-26-expenses-budget-forecast.md)) found such drift.

## Decision

- Every rule in `rules.md` that can be checked mechanically has a test in `tests/ArchitectureTests` (or, when it needs
  a database, in `tests/IntegrationTests`), and CI runs them on every push. The rule names its test in the
  [enforcement table](../rules.md#enforcement).
- Checks read what is most reliable for the rule: compiled types (layers, use cases, endpoints), the EF model
  (schemas, migrations), the generated OpenAPI document (responses, headers, bodies), project files, and, only where
  nothing better exists, the source text (one type per file, one save per command, queries that do not write).
- A rule that cannot be checked mechanically (judgement about meaning, process, “no query in a loop”) is marked
  **review only** in the table with the reason, and is checked by the audit of each feature.
- A justified exception lives in the test as a named allow-list entry with its reason and ADR, never as a skipped
  test.
- Adding a rule means adding its test in the same change, or its review-only reason.

## Consequences

- A rule broken by a change fails CI before review, including the rules about the contract and the database.
- Text-based checks are heuristics; they may need adjusting when the code style changes. They are kept short and
  each one says what it looks at.
- The table in `rules.md` is the one place to see how a rule is enforced.
