# 002 — Savings jar «Копилка» and the savings goal

- **Module:** Expenses
- **Type:** feature — **retroactive, describes implemented behavior**
- **Status:** implemented (jar in PR #8 `12ba66c`, goal in PR #9 `b6fb5d7`); SDD written 2026-11-26 from the code at `8378e4a`
- **Owner:** Vitaliy
- **Created:** 2026-11-26
- **Frontend counterpart:** [web 002](../../../../../../web/docs/specs/web/expenses/002-savings-page/README.md)
- **Related:** [001 entries](../001-entries-and-sheet-import/README.md) (shared entry rules, idempotency, versions),
  [003 budget settings](../003-budget-settings-and-limits/README.md) (where the goal is stored and edited)

The jar (card *8140) is a log of signed movements: plus puts money in, minus takes it out
([ADR 0018](../../../../decisions/0018-savings-jar-as-signed-movements.md)). Its balance is never stored. A month shows
its movements with balances at start and end; an overview lists every month with the running balance. The savings
goal is a number in the budget settings that the web compares with the current balance.

Retroactive: no behavior change; gaps are open questions in [overview](requirements/overview.md).

## Requirements

- [Overview and scope](requirements/overview.md)
- [Savings jar](requirements/savings-jar.md)

## Technical design

- [Domain](design/domain.md)
- [Infrastructure](design/infrastructure.md)
- [Data model](data-model.md)
- [API contract](contracts/api-contract.md)

## File map (implemented)

Paths relative to `api/src/Modules/Expenses/`.

| Path | State | One responsibility |
| --- | --- | --- |
| `Expenses.Domain/Savings/{SavingsMovement,SavingsDirection}.cs` | existing | the movement aggregate; direction derived from the sign |
| `Expenses.Domain/Entries/EntryRules.cs` (`CheckSigned`) | existing | money rules for a signed amount |
| `Expenses.Application/Features/Savings/Commands/{Create,Update,Delete}SavingsMovement/` | existing | writes |
| `Expenses.Application/Features/Savings/Queries/ListSavingsMovements/`, `ListSavingsMonths/` | existing | month list with balances; months overview |
| `Expenses.Infrastructure/Persistence/Entries/SavingsMovementConfiguration.cs` | existing | mapping |
| `Expenses.Api/Endpoints/Savings/` | existing | endpoints |
| `Expenses.Domain/Budget/BudgetSettings.cs` (`SavingsGoal`) | existing | the goal (owned by 003) |

## Planning and delivery

- [Scenario traceability](traceability.md)
- [Delivery readiness](checklist/delivery-readiness.md)

## Change history

- 2026-11-26 — retroactive SDD created from the code at `8378e4a`.
