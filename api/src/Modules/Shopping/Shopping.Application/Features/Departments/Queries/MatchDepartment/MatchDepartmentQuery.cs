using BuildingBlocks.Domain.Results;
using Mediator;

namespace Shopping.Application.Features.Departments.Queries.MatchDepartment;

/// <summary>Which department an item with this name would go to (the preview while typing).</summary>
public sealed record MatchDepartmentQuery(string? Name) : IQuery<Result<DepartmentMatch>>;
