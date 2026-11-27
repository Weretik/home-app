using Notes.Application.Features.Notes.Commands.CreateNote;
using Notes.Application.Features.Notes.Commands.DeleteNote;
using Notes.Application.Features.Notes.Commands.UpdateNote;
using Notes.Domain.Notes;

namespace UnitTests.Notes.Input;

/// <summary>Spec notes/001: the shape of note input (R-002…R-005, R-009, R-010).</summary>
public sealed class NoteValidatorTests
{
    private static readonly CreateNoteValidator Create = new();
    private static readonly UpdateNoteValidator Update = new();
    private static readonly DeleteNoteValidator Delete = new();

    private static CreateNoteCommand NewNote(
        string key = "key-1", NoteKind kind = NoteKind.Likes, string title = "Півонії", string? details = null, string? place = null,
        Guid? categoryId = null) =>
        new(key, Person.Maria, kind, title, details, place, categoryId, true);

    [Fact]
    public void A_Plain_Note_IsValid()
    {
        Assert.True(Create.Validate(NewNote(details: "рожеві", place: "Квіткова лавка")).IsValid);
    }

    [Fact]
    public void A_Create_NeedsAnIdempotencyKey()
    {
        Assert.Contains(Create.Validate(NewNote(key: "")).Errors, e => e.PropertyName == "IdempotencyKey");
    }

    [Fact]
    public void An_Unknown_Kind_OrPerson_IsInvalid()
    {
        Assert.Contains(Create.Validate(NewNote(kind: (NoteKind)7)).Errors, e => e.PropertyName == "Kind");
        Assert.Contains(Create.Validate(NewNote() with { About = (Person)5 }).Errors, e => e.PropertyName == "About");
    }

    [Fact]
    public void Lengths_AreChecked()
    {
        Assert.Contains(Create.Validate(NewNote(title: " ")).Errors, e => e.PropertyName == "Title");
        Assert.Contains(Create.Validate(NewNote(title: new string('т', 201))).Errors, e => e.PropertyName == "Title");
        Assert.Contains(Create.Validate(NewNote(details: new string('д', 2001))).Errors, e => e.PropertyName == "Details");
        Assert.Contains(Create.Validate(NewNote(place: new string('м', 201))).Errors, e => e.PropertyName == "Place");
    }

    [Fact]
    public void An_Empty_CategoryId_IsInvalid()
    {
        Assert.Contains(Create.Validate(NewNote(categoryId: Guid.Empty)).Errors, e => e.PropertyName == "CategoryId");
    }

    [Fact]
    public void An_Edit_OrADelete_NeedsTheVersion()
    {
        var edit = new UpdateNoteCommand(Guid.NewGuid(), 0, Person.Vitaliy, NoteKind.Dislikes, "Томатний сік", null, null, null, false);

        Assert.Contains(Update.Validate(edit).Errors, e => e.PropertyName == "Version");
        Assert.True(Update.Validate(edit with { Version = 42 }).IsValid);
        Assert.Contains(Delete.Validate(new DeleteNoteCommand(Guid.NewGuid(), 0)).Errors, e => e.PropertyName == "Version");
        Assert.True(Delete.Validate(new DeleteNoteCommand(Guid.NewGuid(), 42)).IsValid);
    }
}
