namespace Shopping.Application.Features.Items.Queries.ListFrequent;

/// <summary>A name bought often, in its latest spelling and department; one tap adds it.</summary>
public sealed record FrequentItem(
    string Name,
    Guid DepartmentId,
    string DepartmentName,
    string DepartmentIcon,
    int TimesBought,
    DateTimeOffset LastBoughtAtUtc);
