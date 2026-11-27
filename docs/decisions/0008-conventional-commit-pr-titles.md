# 0008. Commits and pull request titles follow Conventional Commits, checked in CI; no AI attribution

**Status:** accepted, 2026-11-27

## Context

The first commits used Conventional Commits (`fix(catalog): …`), but the rule was never written down. Pull requests
are squash-merged, so the pull request title becomes the commit on `main`, and titles drifted to free style
(`CI: check branch names …`, `Budget: each month keeps …`). Agent sessions also added their own signatures to commits
and pull requests (`Co-Authored-By: Claude …`, session links, "Generated with Claude Code"). Vitaliy set one commit
standard for all his projects, alongside the branch naming of [ADR 0007](0007-conventional-branch-names.md).

## Decision

Every commit message and every pull request title follows [Conventional Commits](https://www.conventionalcommits.org/):
`<type>(<optional scope>): <description>`, in English.

- Types: the branch types `feat`, `fix`, `hotfix`, `refactor`, `perf`, `docs`, `test`, `chore`, plus `build`, `ci`,
  `style` and `revert` when they fit better.
- The scope is optional, lowercase kebab-case, usually a module or half: `expenses`, `todos`, `web`, `api`, `ci`.
- `!` before the colon marks a breaking change.
- The description says the result, starts with a lowercase letter, has no trailing period; the title is at most 100
  characters (aim for 72). Examples: `fix(catalog): keep filter on reload`, `feat(todos): add due dates`.
- A larger commit gets a body after a blank line.
- No AI attribution anywhere: no `Co-Authored-By` lines for an assistant, no session links, no "Generated with …"
  lines or attribution comments, neither in commits nor in pull request descriptions.
- Dependabot titles get a prefix (`build(deps)`, `ci(deps)`) from `.github/dependabot.yml`.

The workflow `.github/workflows/pr-title.yml` (check `pr title`) fails a pull request whose title breaks the format;
it runs again when the title is edited. It is marked required for `main` in the repository ruleset, so such a pull
request cannot be merged. History already on `main` is not rewritten.

Considered: a commit-msg hook with commitlint (runs only where installed, needs Node in every checkout, and does not
see the squash title GitHub creates) and checking every commit of the branch (squash merge throws those commits away,
the title is what lands on `main`).

## Consequences

- `main` reads as a uniform changelog; release notes or a changelog can be generated from it later.
- A wrong title is fixed by editing it, without a new push.
- Commits inside a branch are not checked by CI; agents and people follow the rule by hand.
