# 0010. Every web rule has a check, or a written reason why it is review only

Status: accepted, 2026-11-26. Extends [0004](0004-feature-folders-checked-in-ci.md).

## Context

[0004](0004-feature-folders-checked-in-ci.md) put the folder rules, feature isolation and the API client boundary into
`tools/check-structure.mjs`. Other written rules were only in prose: one component per file, file naming, the import
direction between `app/`, `features/`, `platform/` and `api-client/`, no empty or wrapper folders, no dump file names,
no literal colours, the app never indexed, tests next to logic. The audit of 2026-11-26 found code that broke some of
them unnoticed (several components in one file, logic without a test), and the owner asked for every rule, the
boundaries and the files per folder, to be controlled by tests.

## Decision

- [rules.md](../rules.md) ends with a table: every rule and its check, or "review only" with the reason a tool cannot
  decide it. A new rule is added together with its check or its reason.
- The structure check is split by kind into `tools/architecture/` (`folder-rules.mjs`, `file-rules.mjs`,
  `import-rules.mjs`, `app-rules.mjs`, listed in `rules.mjs`); `tools/check-structure.mjs` runs them and stays the CI
  entry (`npm run architecture:structure`).
- `tools/architecture/rules.test.mjs`, run by `npm test`, gives each rule a breaking and a clean tree, so a rule that
  silently stops working fails the tests.
- Package-level rules stay in ESLint: `no-restricted-globals` for `fetch`, `no-restricted-imports` for `axios` and for
  `openapi-fetch` outside `src/api-client`.

## Consequences

- A breach of a mechanical rule fails CI with one line naming the file and the fix.
- Judgement rules (one responsibility under the line limit, Russian wording, design) stay with review and
  `frontend-code-audit`.
- The checks read source text with regular expressions, not a TypeScript parser: an unusual declaration style could
  slip past `oneComponentPerFile` or `hookFileNames`. Moving to the TypeScript compiler API is the next step if that
  happens.
