using BuildingBlocks.Domain.Results;
using Mediator;

namespace Shopping.Application.Features.Departments.Commands.ReorderDepartments;

/// <summary>Sets the store walk: every department id exactly once, in the new order.</summary>
public sealed record ReorderDepartmentsCommand(IReadOnlyList<Guid> DepartmentIds) : ICommand<Result>;
