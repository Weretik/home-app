using BuildingBlocks.Domain.Results;
using Mediator;

namespace Shopping.Application.Features.Departments.Commands.UpdateDepartment;

/// <summary>Renames a department or changes its icon, if nobody changed it since <see cref="Version"/>.</summary>
public sealed record UpdateDepartmentCommand(Guid Id, uint Version, string Name, string? Icon) : ICommand<Result>;
