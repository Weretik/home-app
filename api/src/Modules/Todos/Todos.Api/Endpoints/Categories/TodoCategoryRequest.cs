namespace Todos.Api.Endpoints.Categories;

/// <summary>A category's name and optional emoji, for adding or changing one.</summary>
internal sealed record TodoCategoryRequest(string Name, string? Emoji);
