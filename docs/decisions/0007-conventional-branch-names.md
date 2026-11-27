# 0007. Branch names follow Conventional Branch, checked in CI

**Status:** accepted, 2026-11-27

## Context

Branches were named by whoever opened them: agent sessions used generated names such as `claude/project-thread-…`
with random suffixes, which say nothing about the change and differ between the repositories of one feature. Vitaliy
set one naming standard for all his projects.

## Decision

Every branch is created from `main` and named by [Conventional Branch](https://conventional-branch.github.io/):
`<type>/<short-kebab-case-description>`.

- Types: `feat` (new feature), `fix` (bug), `hotfix` (urgent production fix), `refactor`, `perf`, `docs`, `test`,
  `chore` (infrastructure, dependencies, CI).
- The description is English, lowercase letters, digits and hyphens, 3 to 6 words: `feat/order-history-export`,
  `fix/login-redirect-loop`, `chore/update-ci-node`.
- One feature across several repositories uses the same branch name in each of them.
- No `claude`, no random suffixes, hashes or dates.
- Bot branches `dependabot/*` and `renovate/*` are allowed as they are.

The workflow `.github/workflows/branch-name.yml` (check `branch name`) fails a pull request whose branch breaks the
rule. Its job is marked required for `main` in the repository settings, so such a pull request cannot be merged.

Considered: a free-form convention written only in AGENTS.md (agents and people forget it, nothing stops a wrong
name) and a server-side push rule (branch name restrictions in GitHub rulesets are not available on a free personal account, and they would not explain the rule in the log).

## Consequences

- Branch names say what a change is, and the branches of one feature match across repositories.
- A pull request opened from a wrong name has to be reopened from a renamed branch.
- Skills and templates that suggest other names (for example `codex/...`) were aligned in Weretik/obsidian.
