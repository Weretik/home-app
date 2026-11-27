using BuildingBlocks.Domain.Results;
using Mediator;

namespace Notes.Application.Features.Categories.Queries.ListNoteCategories;

/// <summary>Every note category in display order.</summary>
public sealed record ListNoteCategoriesQuery : IQuery<Result<IReadOnlyList<NoteCategoryItem>>>;
