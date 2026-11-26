namespace Shopping.Application.Features.Departments.Queries.ListDepartments;

/// <summary>A department as the settings and pickers show it.</summary>
public sealed record DepartmentItem(
    Guid Id,
    string Name,
    string Icon,
    int SortOrder,
    bool IsFallback,
    int KeywordCount,
    int NeededCount,
    uint Version);
