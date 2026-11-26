namespace Shopping.Application.Features.Items.Queries.ListSuggestions;

/// <summary>
/// A name ever added to the list, in its latest spelling and department, for the quick add to suggest while typing.
/// </summary>
public sealed record NameSuggestion(
    string Name,
    Guid DepartmentId,
    string DepartmentName,
    string DepartmentIcon,
    int TimesBought,
    DateTimeOffset LastAddedAtUtc,
    bool InList);
