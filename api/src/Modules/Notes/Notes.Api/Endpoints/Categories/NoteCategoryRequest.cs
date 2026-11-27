namespace Notes.Api.Endpoints.Categories;

/// <summary>A category's name and optional emoji, for adding or changing one.</summary>
internal sealed record NoteCategoryRequest(string Name, string? Emoji);
