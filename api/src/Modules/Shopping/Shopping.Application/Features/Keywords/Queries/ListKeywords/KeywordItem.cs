namespace Shopping.Application.Features.Keywords.Queries.ListKeywords;

/// <summary>A keyword as the settings show it, with its department.</summary>
public sealed record KeywordItem(
    Guid Id,
    string Keyword,
    Guid DepartmentId,
    string DepartmentName,
    string DepartmentIcon,
    bool Learned,
    uint Version);
