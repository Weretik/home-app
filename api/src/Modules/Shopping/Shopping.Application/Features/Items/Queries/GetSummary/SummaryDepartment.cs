namespace Shopping.Application.Features.Items.Queries.GetSummary;

/// <summary>A department with something to buy.</summary>
public sealed record SummaryDepartment(Guid DepartmentId, string Name, string Icon, int NeededCount);
