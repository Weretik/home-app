# Month summary — domain design (as implemented)

## Responsibility

The summary is a read model in Application (`Features/Summary/Report/`), not a domain aggregate (ADR 0020). It uses
the domain's `SpendingLimit.PeriodAround` for period bounds; everything else is arithmetic in the handler.

## Cross-module interaction

None.

## Risks

- Figures change when past entries or limits change (by design: computed on read).
- Time zone fallback (GAP-013).
