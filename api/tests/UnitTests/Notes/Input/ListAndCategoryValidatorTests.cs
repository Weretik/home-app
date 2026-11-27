using Notes.Application.Features.Categories.Commands.CreateNoteCategory;
using Notes.Application.Features.Categories.Commands.UpdateNoteCategory;
using Notes.Application.Features.Notes.Queries.ListNotes;
using Notes.Domain.Notes;

namespace UnitTests.Notes.Input;

/// <summary>Spec notes/001: list filters (R-013) and category input (R-011).</summary>
public sealed class ListAndCategoryValidatorTests
{
    private static readonly ListNotesValidator List = new();
    private static readonly CreateNoteCategoryValidator CreateCategory = new();
    private static readonly UpdateNoteCategoryValidator UpdateCategory = new();

    private static ListNotesQuery Query(Person? about = null, Guid? categoryId = null, bool withoutCategory = false, string? search = null) =>
        new(about, [NoteKind.Likes, NoteKind.Fact], categoryId, withoutCategory, search);

    [Fact]
    public void Every_Filter_IsOptional()
    {
        Assert.True(List.Validate(new ListNotesQuery(null, [], null, false, null)).IsValid);
        Assert.True(List.Validate(Query(Person.Maria, Guid.NewGuid(), search: "агромол")).IsValid);
        Assert.True(List.Validate(Query(withoutCategory: true)).IsValid);
    }

    [Fact]
    public void A_Category_AndWithoutCategory_Together_AreInvalid()
    {
        Assert.Contains(List.Validate(Query(categoryId: Guid.NewGuid(), withoutCategory: true)).Errors, e => e.PropertyName == "categoryId");
    }

    [Fact]
    public void Unknown_People_OrKinds_AndLongSearches_AreInvalid()
    {
        Assert.False(List.Validate(Query(about: (Person)9)).IsValid);
        Assert.False(List.Validate(new ListNotesQuery(null, [(NoteKind)9], null, false, null)).IsValid);
        Assert.False(List.Validate(Query(search: new string('с', 101))).IsValid);
    }

    [Fact]
    public void A_Category_NeedsAName_UpTo50_AndAnEmoji_UpTo8()
    {
        Assert.True(CreateCategory.Validate(new CreateNoteCategoryCommand("Книги", "📚")).IsValid);
        Assert.False(CreateCategory.Validate(new CreateNoteCategoryCommand("", null)).IsValid);
        Assert.False(CreateCategory.Validate(new CreateNoteCategoryCommand(new string('к', 51), null)).IsValid);
        Assert.False(CreateCategory.Validate(new CreateNoteCategoryCommand("Книги", "123456789")).IsValid);
    }

    [Fact]
    public void A_Category_Edit_NeedsTheVersion()
    {
        Assert.Contains(UpdateCategory.Validate(new UpdateNoteCategoryCommand(Guid.NewGuid(), 0, "Книги", null)).Errors, e => e.PropertyName == "Version");
        Assert.True(UpdateCategory.Validate(new UpdateNoteCategoryCommand(Guid.NewGuid(), 3, "Книги", null)).IsValid);
    }
}
