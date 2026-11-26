# Shopping list — infrastructure design

## Storage

- `ShoppingDbContext : ModuleDbContext`, schema `shopping`, migration history `shopping.__ef_migrations_history`,
  implements `IShoppingDbContext` (sets + `SaveChangesAsync` + `Database` for the one explicit transaction).
- Configurations in `Persistence/Items/` (items, create requests) and `Persistence/Reference/` (departments,
  keywords, settings, `DictionarySeed`, `SeedIds`).
- Enums stored as text; `Version` mapped to `xmin` (`IsRowVersion`), as in Expenses (ADR 0017).
- Commands save once. Exception: deleting a department moves items and keywords with two set-based `UPDATE`s and
  deletes the row in one explicit transaction (items can be thousands of history rows; loading them would break the
  "no query in a loop / bounded load" rules). Creating a department and reordering departments change several
  department rows (sort orders) but still in one `SaveChanges`, so one commit; the list is at most a few dozen rows.
- Queries: the list is read whole (bounded by its nature) and ordered in memory; history is paged in SQL; frequent
  items are grouped in SQL (`GROUP BY normalized_name`), then the latest name and department of the ≤20 names are
  read; the summary reuses the list query and the frequent statistics.
- Indexes: `ix_shopping_items_status_bought_at_utc` (status, bought_at_utc) for the cart window, history and frequent
  items; `ix_shopping_items_normalized_name` for re-add lookups; `ix_shopping_items_department_id`;
  `ux_shopping_items_import_key` (filtered); `ux_departments_name`; `ux_department_keywords_keyword`;
  `ix_department_keywords_department_id`; `ix_department_keywords_keyword_trgm` (gin, `gin_trgm_ops`, extension
  `pg_trgm` created by the migration) for the keyword search. Foreign keys inside the schema: items → departments and keywords →
  departments, both `restrict` (a department is deleted only after its rows moved).

## Seed

`DictionarySeed` holds the «Справочник» departments (in order, icon and name split) and keywords per department, as
read from the sheet on 2026-11-26. Keywords are normalised and de-duplicated (`сёмг` = `семг`, `мёд` = `мед`).
Ids are UUID v5 of `department:<name>` / `keyword:<keyword>` in a fixed namespace (`SeedIds`), so `HasData` is stable
and the import script can compute them too. Settings row: fixed id, 3 hours, 183 days.

## Migration and deployment

One migration `Initial` creates the schema, tables, indexes and seed rows; it only adds, so the previous release is
unaffected. The deploy workflow must build a migration bundle for `ShoppingDbContext` (step in
`.github/workflows/api.yml`, next to the Expenses bundle). Rollback: the previous release does not use the schema;
`DROP SCHEMA shopping CASCADE` removes it if ever needed. Locally (`Database:InitializeOnStartup`) the
`ShoppingDatabaseInitializer` applies the migration.
