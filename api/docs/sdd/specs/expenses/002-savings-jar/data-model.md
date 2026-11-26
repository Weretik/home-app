# Savings jar — data model (as implemented)

## Model

```text
expenses.savings_movements  id, date, person (text enum), amount numeric(12,2) signed ≠ 0, comment (500),
                            import_key (100, unique where not null), created_at_utc, xmin
                            index ix_savings_movements_date
expenses.budget_settings    savings_goal numeric(12,2) ≥ 0 (owned by feature 003)
```

## Identifiers

| Object | ID / key | Where created | Public id? | Reason |
| --- | --- | --- | --- | --- |
| `SavingsMovement` | `uuid` v7 | domain `Create` | yes | module strategy |
| create request | `Idempotency-Key`, kind `savings` | client | no | ADR 0007 |
| import key | `sheet:savings:…` | import | no | repeatable import |

## Invariants and integrity

- Non-zero signed money: domain. Balance: computed, never stored ([ADR 0018](../../../../decisions/0018-savings-jar-as-signed-movements.md)).
