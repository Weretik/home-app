using BuildingBlocks.Domain.Results;
using Mediator;

namespace Shopping.Application.Features.Departments.Commands.DeleteDepartment;

/// <summary>Deletes a department; its items (list and history) and keywords move to the fallback department.</summary>
public sealed record DeleteDepartmentCommand(Guid Id, uint Version) : ICommand<Result>;
