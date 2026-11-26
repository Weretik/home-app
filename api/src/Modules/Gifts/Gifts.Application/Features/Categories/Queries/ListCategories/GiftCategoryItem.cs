namespace Gifts.Application.Features.Categories.Queries.ListCategories;

/// <summary>A gift category as lists and pickers show it.</summary>
public sealed record GiftCategoryItem(Guid Id, string Name, int SortOrder);
