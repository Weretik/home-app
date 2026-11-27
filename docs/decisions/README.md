# Decisions (ADRs)

Repository-wide decisions (structure, hosting, database server, delivery process). Backend and web decisions live next to their code:
[api/docs/decisions](../../api/docs/decisions/README.md) and [web/docs/decisions](../../web/docs/decisions/README.md).

Short records of decisions that are expensive to change. Free format: context, decision, consequences, half a page.
New file: `NNNN-short-name.md`, next number in order.

| # | Decision | Status |
| --- | --- | --- |
| [0001](0001-monorepo-on-the-template.md) | One repository, built on fullstack-modular-template | Accepted |
| [0002](0002-azure-free-hosting.md) | Hosting on Azure free tiers, deployment from GitHub Actions | Accepted |
| [0003](0003-postgres-at-hosting-provider.md) | PostgreSQL: a separate database at the existing hosting provider | Accepted |
| [0004](0004-sdd-first-feature-delivery.md) | Every feature: SDD first, then implementation, then an architecture audit | Accepted |
| [0005](0005-post-deploy-smoke-reads-every-section.md) | Tests use real-format secrets, outside clients run for real, the deploy reads every section | Accepted |
| — | Authentication for two users | Open |
