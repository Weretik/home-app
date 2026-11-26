# Month summary — data model (read model, as implemented)

## Model

```text
MonthSummary
├── year, month
├── incomes, incomesByPerson[] { person, amount }
├── expenses, expensesByPayer[] { payer, amount }
├── monthlyLimit, daysLeft (int?)
├── result, savingsNet, savingsBalance, savingsGoal
├── limits[] LimitUsage { id, name, period, from, to, amount, spent, left, percent, perDay? }
└── topCategories[] { categoryId, name, amount } (≤ 5)
```

## Identifiers

No new identifiers: rows reuse limit and category ids.

## Invariants

- Nothing is stored; every figure is reproducible from entries and budget (ADR 0020).
