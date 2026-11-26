using BuildingBlocks.Domain.Results;
using Mediator;

namespace Gifts.Application.Features.Categories.Queries.ListCategories;

/// <summary>All gift categories in their display order.</summary>
public sealed record ListCategoriesQuery : IQuery<Result<IReadOnlyList<GiftCategoryItem>>>;
