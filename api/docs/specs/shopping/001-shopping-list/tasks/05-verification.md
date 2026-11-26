# 05 — Verification and delivery

## TS-017 — Documentation

- `docs/modules/shopping.md` (model, rules, use cases, layers, database, seed, import notes),
  `docs/decisions/0031-shopping-module-model.md`, `docs/decisions/0032-text-parsing-and-department-matching.md`,
  `docs/decisions/README.md`, `docs/architecture/c4.md`, `api/README.md` (module list), root `AGENTS.md`
  (product paragraph). Substitute check: links resolve, names match the code.

## TS-018 — Verification and audit

- `cd api && dotnet build HomeApp.slnx` (warnings are errors), `dotnet test tests/UnitTests`,
  `dotnet test tests/ArchitectureTests`, `dotnet build tests/IntegrationTests`,
  `dotnet ef migrations has-pending-model-changes` for `ShoppingDbContext`.
- Self-audit with `backend-code-audit` against this SDD and `docs/rules.md`, report in
  [code-audit/audit.md](../code-audit/audit.md); findings fixed or recorded.
- Outside `api/` (owner: the person who commits): add the Shopping migration bundle to `.github/workflows/api.yml`.
