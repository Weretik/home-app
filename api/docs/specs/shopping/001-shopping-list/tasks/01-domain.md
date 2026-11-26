# 01 — Domain

Test level: unit (`tests/UnitTests/Shopping/Text`, `tests/UnitTests/Shopping/Domain`). Each task: write the test,
see it fail for the behaviour (Red), implement, Green, refactor, run `dotnet test tests/UnitTests` (Regression).

## TS-001 — Parsing and matching (SC-001, SC-002)

- `Shopping.Domain/Parsing/{NameNormalizer,ItemTextParser,ParsedItem}.cs`,
  `Shopping.Domain/Matching/{DepartmentMatcher,KeywordRule}.cs`.
- Cases: «молоко, хлеб, яйца 10, сыр 300 г»; «2 кг картошки»; «молоко x 3»; «яйца х10»; bullets and trailing
  dots; `;` and new lines; name cut to 80 and capitalised; ё → е; «зубн паст» beats «паст»; exact 1000 beats a
  longer prefix match; no match → fallback; tie → first rule.

## TS-002 — Item rules (SC-003, SC-004, SC-007)

- `Shopping.Domain/Items/{ShoppingItem,ShoppingItemStatus,Person,ShoppingItemErrors}.cs`.
- Cases: name 1..80 trimmed, quantity empty → null and ≤30, note ≤200; status bought twice keeps the first buyer;
  needed clears buyer; restore resets added by/at and cleared; refresh replaces quantity only when given.

## TS-003 — Cart window (SC-005)

- `Shopping.Domain/Items/CartWindow.cs`.
- Cases: needed always in list; bought 2 h ago in, 4 h ago out (3 h window); cleared out; the expression gives the
  same answer as `IsInList`.

## TS-004 — Departments and keywords (SC-009, SC-010)

- `Shopping.Domain/Departments/{Department,DepartmentKeyword,DepartmentErrors}.cs`.
- Cases: name 1..40, icon ≤8; fallback cannot be deleted; keyword normalised, 2..60; learn marks learned.

## TS-005 — Settings (SC-011)

- `Shopping.Domain/Settings/{ShoppingSettings,ShoppingSettingsErrors}.cs`. Cases: defaults 3 / 183; bounds
  1..72 and 30..730; invalid keeps old values.
