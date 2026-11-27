namespace Notes.Application.Features.Categories.Queries.ListNoteCategories;

/// <summary>A category with its version for <c>If-Match</c> and how many notes it holds.</summary>
public sealed record NoteCategoryItem(Guid Id, string Name, string? Emoji, int SortOrder, uint Version, int NoteCount);
