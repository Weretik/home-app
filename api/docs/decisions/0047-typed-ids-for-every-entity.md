# 0047. Every entity has its own typed id

Status: accepted, 2026-11-28.

## Context

Every entity used a bare `Guid` as its id, and every reference to another record (an expense's category, a task's
category, an item's department) was a `Guid` too. The compiler could not tell a note id from a category id, so a
swapped argument compiled and failed only at run time, as a «not found» or, worse, a write to the wrong row. Finding
F7 of the [2026-11-27 architecture review](../../../docs/reviews/2026-11-27-architecture-before.md) noted that the
DDD rule of typed ids, used in Vitaliy's other backends, was not adopted here. Vitaliy decided on 2026-11-28 to adopt
it in every module at once.

## Decision

- Every entity has its own id type: `public readonly record struct <Entity>Id(Guid Value)` beside the entity, or in
  the aggregate's `Identifiers/` folder when the folder would pass six files. It has `New(now)` (a UUID v7 from the
  creation time, as before), `IComparable<T>` and a `ToString()` that prints the bare value for error texts.
- Every reference inside a module uses the referenced type (`Expense.CategoryId` is a `CategoryId`,
  `TodoTask.CategoryId` a `TodoCategoryId?`). A task's source is a `TaskSourceId`: the id of a record in the module
  named by `SourceKind`, which the task never reads.
- The Domain holds no bare `Guid` at all. `Contracts`, commands, queries and HTTP records keep the plain `Guid`; the
  handler wraps it (`new NoteId(c.Id)`) and a projection unwraps it (`n.Id.Value`) only in the final `Select`.
- Persistence: `ModuleDbContext` maps every typed id of the module's Domain with `TypedIdConventions.AddTypedIds`
  (a value converter per id type, also as the default mapping for query and raw SQL parameters). The columns stay
  `uuid`, so there is no migration; `DataOwnershipTests.The_Migrations_MatchTheModel` proves it.
- `ArchitectureTests/Boundaries/TypedIdTests` fails when an entity uses a bare or shared id type, or when a Domain type
  has a `Guid` field, property, parameter or return value.

## Options considered

- **Keep `Guid`** (no work): the review finding stays, and the compiler keeps accepting swapped ids. Rejected.
- **A source generator (Vogen, StronglyTypedId)**: less code per id, but one more package and generated code to
  learn; 25 small hand-written types are easy to read. Rejected for now, the same choice as Vitaliy's other backends.
- **Typed ids for roots only**: children (`TaskStep`, `GiftVariant`, `DepartmentKeyword`) would keep `Guid`, and the
  "no bare Guid in the Domain" rule would need exceptions. Rejected: one rule without exceptions is simpler to check.

## Consequences

- Passing a `NoteId` where a `NoteCategoryId` is expected no longer compiles.
- A query compares typed ids (`x.Id == new NoteId(c.Id)`); `.Value` inside a `Where` cannot be translated to SQL
  and is a review point. The integration tests run every query against PostgreSQL.
- An EF Core seed (`HasData`) and a shadow foreign key are written with the id type (`Property<TodoTaskId>("TaskId")`).
- A new entity needs one more small file, its id.
