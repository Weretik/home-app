# Entries and sheet import — infrastructure design (as implemented)

## Storage and queries

- `ExpensesDbContext` (schema `expenses`) is the unit of work behind the `IExpensesDbContext` port
  ([ADR 0004](../../../../decisions/0004-dbcontext-port-instead-of-repositories.md)).
- Configurations in `Persistence/Entries/` (expenses, incomes, savings movements, create requests) and
  `Persistence/Reference/` (categories with `CategorySeed`).
- `ListExpenses`: one `SUM` over the filtered rows, then a paged projection joined to categories (`AsNoTracking`,
  `ToPagedListAsync`). `ListIncomes` the same without the join.
- Creates save the row and its `create_requests` row in one `SaveChangesAsync`; a unique violation (racing copy) is
  translated by `ModuleDbContext` into `UniqueConstraintViolationException` and answered like a repeat.
- Deletes use `ExecuteDeleteAsync` (one statement, returns the count for 404).
- Import: reads all stored import keys of the given rows in three queries (no query per row), builds entities in
  memory, saves once.

## Migration and deployment

Migrations `Initial` (categories with 14 seeded rows, expenses, incomes, create requests) and `Savings`, applied as EF
bundles in the deploy job ([ADR 0016](../../../../decisions/0016-migrations-as-bundles-in-deploy.md)). Production
data was loaded by running the import locally and moving rows with `pg_dump --inserts --on-conflict-do-nothing`
([ADR 0014](../../../../decisions/0014-one-time-sheet-import.md)); `Expenses__Import__Key` stays unset in production,
so the endpoint answers 404 there.
