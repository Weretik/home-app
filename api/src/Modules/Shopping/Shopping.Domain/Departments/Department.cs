using BuildingBlocks.Domain.Modeling;
using BuildingBlocks.Domain.Results;

namespace Shopping.Domain.Departments;

/// <summary>
/// A part of the store («🥦 Овощи и фрукты»). Departments are ordered as the family walks through the store, and
/// exactly one of them, «Другое», is the fallback for items no keyword matches; it cannot be deleted.
/// </summary>
public sealed class Department : Entity<Guid>, IAggregateRoot
{
    public const int MaxNameLength = 40;
    public const int MaxIconLength = 8;
    public const int SortStep = 10;

    private Department()
    {
    }

    public string Name { get; private set; } = string.Empty;

    /// <summary>An emoji shown before the name; empty when none.</summary>
    public string Icon { get; private set; } = string.Empty;

    /// <summary>The position in the store walk.</summary>
    public int SortOrder { get; private set; }

    public bool IsFallback { get; private init; }

    /// <summary>Changes on every save (PostgreSQL <c>xmin</c>); an edit based on an older version is refused.</summary>
    public uint Version { get; private set; }

    public static Result<Department> Create(string name, string? icon, int sortOrder, DateTimeOffset now)
    {
        var check = Check(name, icon);
        if (check.IsFailure)
            return check.Error!;

        return new Department { Id = Guid.CreateVersion7(now), Name = name.Trim(), Icon = CleanIcon(icon), SortOrder = sortOrder };
    }

    public Result Rename(string name, string? icon)
    {
        var check = Check(name, icon);
        if (check.IsFailure)
            return check;

        Name = name.Trim();
        Icon = CleanIcon(icon);
        return Result.Success();
    }

    public void MoveTo(int sortOrder) => SortOrder = sortOrder;

    /// <summary>The fallback stays: deleting another department moves its items and keywords there.</summary>
    public Result EnsureDeletable() =>
        Result.Ensure(!IsFallback, DepartmentErrors.FallbackCannotBeDeleted);

    private static Result Check(string name, string? icon) =>
        Result.Combine(
            Result.Ensure(!string.IsNullOrWhiteSpace(name), DepartmentErrors.NameRequired),
            Result.Ensure(name is null || name.Trim().Length <= MaxNameLength, DepartmentErrors.NameTooLong),
            Result.Ensure(CleanIcon(icon).Length <= MaxIconLength, DepartmentErrors.IconTooLong));

    private static string CleanIcon(string? icon) => icon?.Trim() ?? string.Empty;
}
