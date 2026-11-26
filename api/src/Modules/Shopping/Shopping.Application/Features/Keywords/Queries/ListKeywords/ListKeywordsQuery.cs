using BuildingBlocks.Domain.Results;
using Mediator;

namespace Shopping.Application.Features.Keywords.Queries.ListKeywords;

/// <summary>Keywords alphabetically, optionally of one department and containing every word of <see cref="Search"/>.</summary>
public sealed record ListKeywordsQuery(Guid? DepartmentId, string? Search) : IQuery<Result<IReadOnlyList<KeywordItem>>>;
