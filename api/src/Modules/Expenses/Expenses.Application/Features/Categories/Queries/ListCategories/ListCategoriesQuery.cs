using BuildingBlocks.Domain.Results;
using Mediator;

namespace Expenses.Application.Features.Categories.Queries.ListCategories;

/// <summary>All expense categories in their display order.</summary>
public sealed record ListCategoriesQuery : IQuery<Result<IReadOnlyList<CategoryItem>>>;
