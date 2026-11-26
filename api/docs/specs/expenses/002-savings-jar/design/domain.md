# Savings jar — domain design (as implemented)

## Responsibility

`SavingsMovement` (`Domain/Savings/`) is an aggregate like `Income` with a signed amount checked by
`EntryRules.CheckSigned` (zero → `AmountZero`, otherwise the usual rules on the absolute value). `Direction` is a
computed property. `Version` maps `xmin`. The savings goal belongs to `BudgetSettings` (feature 003), not to the jar.

## Cross-module interaction

None.

## Risks

- Negative balance is allowed (as in the sheet); the web shows it signed.
- Same concurrency and repeat handling as entries (001).
