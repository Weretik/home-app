# 0004. Every feature: SDD first, then implementation, then an architecture audit

**Status:** accepted, 2026-11-26

## Context

Until the budget forecast work the features of this repository were documented only by the module page, ADRs and the
architecture docs, and checked only by the automated architecture tests in CI. Vitaliy's fullstack-modular-template
workflow is specification-first with an audit at the end, and he asked for it to be mandatory here as well. His
templates and checklists live in the Weretik/obsidian repository: `Templates/backend-sdd`, `Templates/frontend-sdd`,
`Architecture/Checklists` and the skills under `Skills/`.

## Decision

Every feature, and above all a new one, is delivered in this order:

1. **Specification.** A backend SDD and a frontend SDD (only the halves the feature touches) from the obsidian
   templates (skills `backend-feature-spec`, `frontend-feature-spec`), stored in `api/docs/specs` and `web/docs/specs`.
   New lasting decisions get ADRs in the same change.
2. **Implementation** from the SDD (skills `backend-feature-delivery`, `frontend-feature-delivery`), with tests and
   docs in the same change.
3. **Architecture audit** of the change against the SDD, the repository rules and the obsidian checklists (skills
   `backend-code-audit`, `frontend-code-audit`): modules and layers, folders and files, component split and size,
   contract, data, idempotency and concurrency. Findings are fixed or recorded with a reason before review.
4. **Merge** only after the audit, with green CI, on Vitaliy's explicit approval.

A small fix that restores documented behavior keeps the short path from AGENTS.md (fix, focused test, short note).

## Consequences

More documents per feature and a slower first step, in exchange for features that can be checked against a written
expectation. Features built before this decision got retroactive SDDs that describe the implemented behavior.
