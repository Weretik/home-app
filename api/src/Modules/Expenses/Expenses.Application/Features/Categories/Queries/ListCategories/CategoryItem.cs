namespace Expenses.Application.Features.Categories.Queries.ListCategories;

/// <summary>A category as lists and pickers show it.</summary>
public sealed record CategoryItem(Guid Id, string Name, int SortOrder);
