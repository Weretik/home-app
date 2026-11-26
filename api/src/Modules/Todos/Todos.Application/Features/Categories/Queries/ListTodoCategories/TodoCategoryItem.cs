namespace Todos.Application.Features.Categories.Queries.ListTodoCategories;

/// <summary>A category with its version for <c>If-Match</c> and how many active tasks it holds.</summary>
public sealed record TodoCategoryItem(Guid Id, string Name, string? Emoji, int SortOrder, uint Version, int ActiveCount);
