using Notes.Domain.Categories;

namespace UnitTests.Notes.Domain;

public sealed class NoteCategoryTests
{
    [Fact]
    public void A_Category_IsTrimmed_AndAnEmptyEmoji_IsNone()
    {
        var category = NoteCategory.Create(Guid.NewGuid(), "  Дача ", " ", 90).Value;

        Assert.Equal("Дача", category.Name);
        Assert.Null(category.Emoji);
        Assert.Equal(90, category.SortOrder);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void An_Empty_Name_IsRefused(string name)
    {
        Assert.Equal("notes.category_name_invalid", NoteCategory.Create(Guid.NewGuid(), name, null, 10).Error!.Code);
    }

    [Fact]
    public void A_Name_LongerThan50_IsRefused()
    {
        Assert.Equal("notes.category_name_invalid", NoteCategory.Create(Guid.NewGuid(), new string('к', 51), null, 10).Error!.Code);
    }

    [Fact]
    public void An_Emoji_LongerThan8_IsRefused()
    {
        Assert.Equal("notes.category_emoji_too_long", NoteCategory.Create(Guid.NewGuid(), "Дім", "123456789", 10).Error!.Code);
    }

    [Fact]
    public void Rename_ChangesNameAndEmoji_AndKeepsThePlace()
    {
        var category = NoteCategory.Create(Guid.NewGuid(), "Дім", "🏠", 10).Value;

        Assert.True(category.Rename(" Квартира ", "🛋️").IsSuccess);

        Assert.Equal("Квартира", category.Name);
        Assert.Equal("🛋️", category.Emoji);
        Assert.Equal(10, category.SortOrder);
    }
}
