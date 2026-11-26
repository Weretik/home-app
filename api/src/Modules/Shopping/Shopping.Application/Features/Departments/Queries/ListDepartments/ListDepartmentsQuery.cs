using BuildingBlocks.Domain.Results;
using Mediator;

namespace Shopping.Application.Features.Departments.Queries.ListDepartments;

/// <summary>All departments in store order, with how many keywords and needed items each has.</summary>
public sealed record ListDepartmentsQuery : IQuery<Result<IReadOnlyList<DepartmentItem>>>;
