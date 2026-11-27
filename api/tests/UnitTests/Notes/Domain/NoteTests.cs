using Notes.Domain.Notes;

namespace UnitTests.Notes.Domain;

/// <summary>Spec notes/001: R-001…R-005, R-007, R-008.</summary>
public sealed class NoteTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 27, 10, 0, 0, TimeSpan.Zero);

    private static Note Milk(Person? author = Person.Vitaliy) =>
        Note.Create(Person.Maria, NoteKind.Dislikes, "  Молоко ", "  з магазину біля дому ", " АТБ ", false, null, author, Now).Value;

    [Fact]
    public void A_Note_IsTrimmed_AndRecordsItsAuthor_AsTheLastEditorToo()
    {
        var note = Milk();

        Assert.Equal(Person.Maria, note.About);
        Assert.Equal(NoteKind.Dislikes, note.Kind);
        Assert.Equal("Молоко", note.Title);
        Assert.Equal("з магазину біля дому", note.Details);
        Assert.Equal("АТБ", note.Place);
        Assert.False(note.IsImportant);
        Assert.Equal(Person.Vitaliy, note.CreatedBy);
        Assert.Equal(Person.Vitaliy, note.UpdatedBy);
        Assert.Equal(Now, note.CreatedAtUtc);
        Assert.Equal(Now, note.UpdatedAtUtc);
        Assert.NotEqual(Guid.Empty, note.Id);
    }

    [Fact]
    public void Empty_Details_AndPlace_AreStoredAsNone()
    {
        var note = Note.Create(Person.Vitaliy, NoteKind.Likes, "Окрошка", "  ", "", true, null, null, Now).Value;

        Assert.Null(note.Details);
        Assert.Null(note.Place);
        Assert.True(note.IsImportant);
        Assert.Null(note.CreatedBy);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void An_Empty_Title_IsRefused(string title)
    {
        Assert.Equal("notes.title_invalid", Note.Create(Person.Maria, NoteKind.Fact, title, null, null, false, null, null, Now).Error!.Code);
    }

    [Fact]
    public void A_Title_LongerThan200_IsRefused_ButSpacesAroundDoNotCount()
    {
        Assert.Equal("notes.title_invalid",
            Note.Create(Person.Maria, NoteKind.Fact, new string('т', 201), null, null, false, null, null, Now).Error!.Code);
        Assert.True(Note.Create(Person.Maria, NoteKind.Fact, $"  {new string('т', 200)}  ", null, null, false, null, null, Now).IsSuccess);
    }

    [Fact]
    public void Too_Long_Details_AndPlace_AreRefused()
    {
        Assert.Equal("notes.details_too_long",
            Note.Create(Person.Maria, NoteKind.Fact, "Чай", new string('д', 2001), null, false, null, null, Now).Error!.Code);
        Assert.Equal("notes.place_too_long",
            Note.Create(Person.Maria, NoteKind.Fact, "Чай", null, new string('м', 201), false, null, null, Now).Error!.Code);
    }

    [Fact]
    public void Update_ReplacesTheFields_KeepsTheAuthor_AndRecordsTheEditor()
    {
        var note = Milk();
        var category = Guid.NewGuid();
        var later = Now.AddHours(2);

        var result = note.Update(Person.Maria, NoteKind.Likes, "Молоко", null, " Агромол ", true, category, Person.Maria, later);

        Assert.True(result.IsSuccess);
        Assert.Equal(NoteKind.Likes, note.Kind);
        Assert.Null(note.Details);
        Assert.Equal("Агромол", note.Place);
        Assert.True(note.IsImportant);
        Assert.Equal(category, note.CategoryId);
        Assert.Equal(Person.Vitaliy, note.CreatedBy);
        Assert.Equal(Person.Maria, note.UpdatedBy);
        Assert.Equal(Now, note.CreatedAtUtc);
        Assert.Equal(later, note.UpdatedAtUtc);
    }

    [Fact]
    public void A_Refused_Update_ChangesNothing()
    {
        var note = Milk();

        var result = note.Update(Person.Vitaliy, NoteKind.Fact, " ", null, null, true, null, Person.Maria, Now.AddHours(1));

        Assert.Equal("notes.title_invalid", result.Error!.Code);
        Assert.Equal(Person.Maria, note.About);
        Assert.Equal("Молоко", note.Title);
        Assert.Equal(Person.Vitaliy, note.UpdatedBy);
    }
}
