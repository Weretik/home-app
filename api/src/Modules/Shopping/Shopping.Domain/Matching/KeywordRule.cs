namespace Shopping.Domain.Matching;

/// <summary>A keyword of the dictionary (normalised, one or more word starts) and the department it points to.</summary>
public sealed record KeywordRule(string Keyword, Guid DepartmentId);
