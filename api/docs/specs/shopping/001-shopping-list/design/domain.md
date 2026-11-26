# Shopping list — domain design

## Responsibility

`Shopping.Domain` (no EF Core, no ASP.NET Core):

- `Items/ShoppingItem` (aggregate root): `Create`, `Update`, `SetStatus`, `Restore` (re-add of an item in the cart),
  `RaiseQuantity`-style `Refresh` for re-adding a needed item. Holds the name, normalised name, quantity, note,
  department id, urgent flag, status, who and when added / bought, cleared time, import key and `xmin` version.
- `Items/CartWindow`: the one home of R-004 — the cutoff (`now − CartKeepHours`), `IsInList`, and the same rule as
  LINQ expressions (`InList`, `InHistory`) so handlers filter in SQL with exactly the domain rule.
- `Items/ShoppingItemStatus` (`Needed`, `Bought`), `Items/Person` (`Vitaliy`, `Maria`; the module's own copy).
- `Departments/Department` (aggregate root): name, icon, sort order, `IsFallback`; `Create`, `Rename`, `MoveTo`;
  `EnsureDeletable` refuses the fallback.
- `Departments/DepartmentKeyword` (aggregate root): normalised keyword, department, `Learned`; `Create`, `Update`,
  `Learn` (move + mark learned).
- `Settings/ShoppingSettings` (singleton): `CartKeepHours`, `FrequentDays`, `Update`.
- `Parsing/NameNormalizer`, `Parsing/ItemTextParser` (+ `ParsedItem`): pure ports of the bot's `norm_` and
  `parseItems_` including its `UNITS` regex; `[0-9]` instead of `\d` so only ASCII digits count, as in JavaScript.
- `Matching/DepartmentMatcher` (+ `KeywordRule`): pure port of `deptOf_`; rules are given in priority order (store
  order of their department, then keyword), the first best score wins.

Errors are typed (`shopping.*` codes) in `ShoppingItemErrors`, `DepartmentErrors`, `ShoppingSettingsErrors`.

## Cross-module interaction

None. `Shopping.Contracts` stays empty; `Person` is a copy, not a reference to Expenses.

## Risks

- Two people tick the same item: `SetStatus` sets a target value, so the second request is a no-op, not a reversal.
- Two people edit the same item, department or keyword: `xmin` row version, `If-Match` → 409.
- Two adds of the same new name at the same moment can produce two rows (no unique index on the name, because the
  same name may be in history many times and in the list once); acceptable for two people, the second is visible
  at once and can be deleted.
- Learning a keyword races with a manual keyword of the same text: the unique index answers 409.
