using BuildingBlocks.Domain.Modeling;
using BuildingBlocks.Domain.Results;
using Shopping.Domain.Parsing;

namespace Shopping.Domain.Departments;

/// <summary>
/// A word start, or several («зубн паст»), that puts an item into a department (a row of «Справочник», columns C:D).
/// Stored normalised and unique. <see cref="Learned"/> keywords were made when a person moved an item to another
/// department; the exact normalised name is learned then.
/// </summary>
public sealed class DepartmentKeyword : Entity<DepartmentKeywordId>, IAggregateRoot
{
    public const int MinKeywordLength = 2;
    public const int MaxKeywordLength = 60;

    /// <summary>How long typed input may be before normalising (extra spaces are collapsed); the stored keyword is checked against <see cref="MaxKeywordLength"/>.</summary>
    public const int MaxInputLength = MaxKeywordLength * 2;

    private DepartmentKeyword()
    {
    }

    public string Keyword { get; private set; } = string.Empty;

    public DepartmentId DepartmentId { get; private set; }

    public bool Learned { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private init; }

    /// <summary>Changes on every save (PostgreSQL <c>xmin</c>); an edit based on an older version is refused.</summary>
    public uint Version { get; private set; }

    public static Result<DepartmentKeyword> Create(string keyword, DepartmentId departmentId, bool learned, DateTimeOffset now)
    {
        var normalized = NameNormalizer.Normalize(keyword);
        var check = Check(normalized);
        if (check.IsFailure)
            return check.Error!;

        return new DepartmentKeyword
        {
            Id = DepartmentKeywordId.New(now), Keyword = normalized, DepartmentId = departmentId, Learned = learned, CreatedAtUtc = now,
        };
    }

    /// <summary>Whether a text can be a keyword at all (a learned name that is too long or too short is not learned).</summary>
    public static bool IsValid(string keyword) => Check(NameNormalizer.Normalize(keyword)).IsSuccess;

    public Result Update(string keyword, DepartmentId departmentId)
    {
        var normalized = NameNormalizer.Normalize(keyword);
        var check = Check(normalized);
        if (check.IsFailure)
            return check;

        Keyword = normalized;
        DepartmentId = departmentId;
        return Result.Success();
    }

    /// <summary>A person put an item with exactly this name into a department: the keyword points there now.</summary>
    public void Learn(DepartmentId departmentId)
    {
        DepartmentId = departmentId;
        Learned = true;
    }

    private static Result Check(string normalized) =>
        Result.Combine(
            Result.Ensure(normalized.Length >= MinKeywordLength, DepartmentErrors.KeywordTooShort),
            Result.Ensure(normalized.Length <= MaxKeywordLength, DepartmentErrors.KeywordTooLong));
}
