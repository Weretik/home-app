using BuildingBlocks.Domain.Modeling;
using BuildingBlocks.Domain.Results;
using Notes.Domain.Categories;

namespace Notes.Domain.Notes;

/// <summary>
/// One statement about one of the two people (ADR 0039): what they like, dislike or what the other one should know,
/// with an optional description, place or brand and category. Both members read and change every note; who wrote it
/// and who changed it last come from the signed-in member, never from the request (R-008).
/// </summary>
public sealed class Note : Entity<NoteId>, IAggregateRoot
{
    public const int MaxTitleLength = 200;
    public const int MaxDetailsLength = 2000;
    public const int MaxPlaceLength = 200;

    private Note()
    {
    }

    /// <summary>Whom the note is about (R-001).</summary>
    public NotePerson About { get; private set; }

    public NoteKind Kind { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Details { get; private set; }

    /// <summary>A shop, brand or place such as «Агромол» (R-005).</summary>
    public string? Place { get; private set; }

    /// <summary>Important notes come first inside their category (R-007).</summary>
    public bool IsImportant { get; private set; }

    public NoteCategoryId? CategoryId { get; private set; }

    /// <summary>Who wrote the note; null when the member has no person in the household.</summary>
    public NotePerson? CreatedBy { get; private init; }

    /// <summary>Who changed the note last; the author until someone edits it.</summary>
    public NotePerson? UpdatedBy { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private init; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    /// <summary>Changes on every save (PostgreSQL <c>xmin</c>); an edit based on an older version is refused.</summary>
    public uint Version { get; private set; }

    public static Result<Note> Create(
        NotePerson about,
        NoteKind kind,
        string title,
        string? details,
        string? place,
        bool isImportant,
        NoteCategoryId? categoryId,
        NotePerson? createdBy,
        DateTimeOffset now)
    {
        var check = Check(title, details, place);
        if (check.IsFailure)
            return check.Error!;

        return new Note
        {
            Id = NoteId.New(now),
            About = about,
            Kind = kind,
            Title = title.Trim(),
            Details = Normalize(details),
            Place = Normalize(place),
            IsImportant = isImportant,
            CategoryId = categoryId,
            CreatedBy = createdBy,
            UpdatedBy = createdBy,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
    }

    /// <summary>Replaces what a member can edit; the author and the creation time stay, the editor is recorded.</summary>
    public Result Update(
        NotePerson about,
        NoteKind kind,
        string title,
        string? details,
        string? place,
        bool isImportant,
        NoteCategoryId? categoryId,
        NotePerson? updatedBy,
        DateTimeOffset now)
    {
        var check = Check(title, details, place);
        if (check.IsFailure)
            return check;

        About = about;
        Kind = kind;
        Title = title.Trim();
        Details = Normalize(details);
        Place = Normalize(place);
        IsImportant = isImportant;
        CategoryId = categoryId;
        UpdatedBy = updatedBy;
        UpdatedAtUtc = now;
        return Result.Success();
    }

    /// <summary>Optional text is trimmed; empty text is stored as none (R-004, R-005).</summary>
    public static string? Normalize(string? text) =>
        string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    private static Result Check(string title, string? details, string? place) =>
        Result.Combine(
            Result.Ensure(!string.IsNullOrWhiteSpace(title) && title.Trim().Length <= MaxTitleLength, NoteErrors.TitleInvalid),
            Result.Ensure(Normalize(details) is not { Length: > MaxDetailsLength }, NoteErrors.DetailsTooLong),
            Result.Ensure(Normalize(place) is not { Length: > MaxPlaceLength }, NoteErrors.PlaceTooLong));
}
