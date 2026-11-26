namespace Shopping.Application.Features.Departments.Queries.MatchDepartment;

/// <summary>The department the dictionary picks for a name.</summary>
public sealed record DepartmentMatch(Guid DepartmentId, string DepartmentName, string DepartmentIcon);
