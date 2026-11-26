using BuildingBlocks.Domain.Modeling;
using BuildingBlocks.Domain.Results;

namespace Gifts.Domain.People;

/// <summary>
/// Something worth knowing when choosing a gift for someone («нюансы»): Маша's ring size, shoe size, necklace length.
/// Edited only in the settings of the gifts section; the home summary shows them.
/// </summary>
public sealed class Detail : Entity<Guid>, IAggregateRoot
{
    public const int MaxLabelLength = 100;
    public const int MaxValueLength = 200;

    private Detail()
    {
    }

    public Recipient Recipient { get; private set; }

    /// <summary>What it is about, such as «Кольцо».</summary>
    public string Label { get; private set; } = string.Empty;

    /// <summary>The size or the note, such as «18,5–19,0».</summary>
    public string Value { get; private set; } = string.Empty;

    /// <summary>The position among the recipient's details, smallest first.</summary>
    public int SortOrder { get; private set; }

    /// <summary>Changes on every save (PostgreSQL <c>xmin</c>); an edit based on an older version is refused.</summary>
    public uint Version { get; private set; }

    public static Result<Detail> Create(Recipient recipient, string label, string value, int sortOrder, DateTimeOffset now)
    {
        var detail = new Detail { Id = Guid.CreateVersion7(now), SortOrder = sortOrder };
        var set = detail.Update(recipient, label, value);
        return set.IsFailure ? set.Error! : detail;
    }

    /// <summary>Replaces the recipient, label and value; the position stays.</summary>
    public Result Update(Recipient recipient, string label, string value)
    {
        var check = Result.Combine(
            Result.Ensure(!string.IsNullOrWhiteSpace(label), DetailErrors.LabelRequired),
            Result.Ensure(label is null || label.Trim().Length <= MaxLabelLength, DetailErrors.LabelTooLong),
            Result.Ensure(!string.IsNullOrWhiteSpace(value), DetailErrors.ValueRequired),
            Result.Ensure(value is null || value.Trim().Length <= MaxValueLength, DetailErrors.ValueTooLong));
        if (check.IsFailure)
            return check;

        Recipient = recipient;
        Label = label!.Trim();
        Value = value!.Trim();
        return Result.Success();
    }
}
