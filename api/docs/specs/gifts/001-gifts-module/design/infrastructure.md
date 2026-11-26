# Gifts — infrastructure design

## Storage

- `GiftsDbContext : ModuleDbContext` with `Schema = "gifts"` and its own history table
  `gifts.__ef_migrations_history` (`AddModuleDbContext`, as Expenses). `IGiftsDbContext` in Application is the unit of
  work: `Gifts`, `Categories`, `Details`, `CreateRequests`, `SaveChangesAsync`.
- `Gift` maps to `gifts.gifts`; its variants are an EF owned collection (`OwnsMany`) mapped to `gifts.gift_variants`
  with their own `id` key and `gift_id` foreign key `ON DELETE CASCADE`, so they load and save with the gift and a gift
  delete (`ExecuteDeleteAsync`) removes them in the database.
- Version: `xmin` mapped with `IsRowVersion()` on `gifts` and `details` (ADR 0017). `GiftsDbContext.SaveChangesAsync`
  marks a gift as modified when one of its variants is added or deleted, so the gift row is updated (with the
  `xmin` check) and gets a new version even when only the variants changed.
- `gifts.create_requests` is the idempotency table, the same shape as `expenses.create_requests` (ADR 0007); kinds
  `gift`, `category`, `detail`.
- Categories are seeded with EF `HasData` from `CategorySeed` (fixed ids and sort orders from the brief).

Database rules applied:

| Rule | Application here |
| --- | --- |
| Reads without tracking, projections | every query uses `AsNoTracking` and projects to records; the list projects the variants in the same query |
| Paging | lists are unpaged on purpose: a few hundred gifts, tens of details (recorded in the overview); the summary reads the wanted and gifted rows' few columns once |
| No query or `SaveChanges` in a loop | none; the summary groups in SQL and in memory |
| Indexes | `ix_gifts_status` (both lists filter by status), `ix_gifts_category_id` (FK and the in-use check), `ix_gift_variants_gift_id` (FK), `ux_categories_name` unique, `ix_details_recipient_sort_order` (summary and settings order) |
| Commits per command | one `SaveChanges` per command; deletes are one `ExecuteDelete` (gift, detail) or one `SaveChanges` (category, to catch the foreign key) |
| Race guards | `xmin` on gift and detail edits and the status change; unique `ux_categories_name` for names (caught → 409); FK `restrict` on category delete (caught → 409); primary key on `create_requests.key` (caught → the earlier result) |
| Search | none (no search box in this feature) |

## Migration and deployment

- One migration `Initial` (EN-003) creates the schema `gifts`, the five tables, the indexes and the 11 categories. It
  only adds objects, so the previous release keeps working (expand only).
- Production: the deploy workflow builds one migration bundle per module; the `gifts` bundle is added to
  `.github/workflows/api.yml` and runs before the new version starts (ADR 0016).
- Development and integration tests: `GiftsDatabaseInitializer` applies the migrations on start
  (`Database:InitializeOnStartup`).
- Rollback: the release before this one does not know the schema `gifts`; leaving it in place is harmless. A full
  revert is `DROP SCHEMA gifts CASCADE` by hand, after confirming the data is not needed (it would lose the imported
  gifts).
- The import SQL runs after the `Initial` migration and depends on the exact table and column names of the
  [data model](../data-model.md).

## Rate limiting and access (architecture questions of the new-feature review)

| Question | Answer |
| --- | --- |
| New library, database, external service, broker | ➖ none; existing EF Core, Npgsql, Mediator, FluentValidation |
| Money, stock, limits, promo codes | ➖ prices are wishes, not money movements; no shared resource |
| Permission per endpoint, owner check | ➖ open until the Identity module (ADR 0015); both people own all data |
| User id from the token | ➖ no tokens yet; nothing taken from the request that a server should compute: status day defaults on the server, sort orders are computed |
| New personal data | details hold body sizes of the two people; kept until deleted in settings; never logged (the logging behavior logs the message type, not its fields) |
| Rate limit per endpoint | reads: global defaults (no policy, as every read of the project); every write: `PublicWrite` (60/min per address), reason: anonymous writes (ADR 0012) |
| Structured log line | the Mediator `LoggingBehavior` logs every command with its type and outcome; no extra business log line needed for a family wishlist |
| Action journal | ➖ none in the project |
| Monthly cost | ➖ a few hundred rows in the existing database; no new paid service |
| Non-functional target as a number | ➖ no quality-targets file in the repository; recorded as audit finding AF-001 |
