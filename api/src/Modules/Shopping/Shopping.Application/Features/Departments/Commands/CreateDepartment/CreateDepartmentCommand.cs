using BuildingBlocks.Domain.Results;
using Mediator;
using Shopping.Application.Idempotency;

namespace Shopping.Application.Features.Departments.Commands.CreateDepartment;

/// <summary>Adds a department just before the fallback. A repeat with the same Idempotency-Key and data returns the first id.</summary>
public sealed record CreateDepartmentCommand(string IdempotencyKey, string Name, string? Icon) : ICommand<Result<ShoppingCreatedId>>;
