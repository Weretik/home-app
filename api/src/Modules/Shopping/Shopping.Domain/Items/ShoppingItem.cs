using BuildingBlocks.Domain.Modeling;
using BuildingBlocks.Domain.Results;
using Shopping.Domain.Parsing;

namespace Shopping.Domain.Items;

/// <summary>
/// One thing to buy: a row of the old «Список» sheet, and after it is bought and leaves the cart, a row of
/// «История». The same name may be in history many times but is in the list once (re-adding updates or revives it).
/// </summary>
public sealed class ShoppingItem : Entity<Guid>, IAggregateRoot
{
    public const int MaxNameLength = ItemTextParser.MaxNameLength;
    public const int MaxQuantityLength = ItemTextParser.MaxQuantityLength;
    public const int MaxNoteLength = 200;
    public const int MaxImportKeyLength = 100;
    public const int MaxItemsPerAdd = 50;

    private ShoppingItem()
    {
    }

    public string Name { get; private set; } = string.Empty;

    /// <summary>The name as compared (<see cref="NameNormalizer"/>): re-adds, frequent items and learned keywords use it.</summary>
    public string NormalizedName { get; private set; } = string.Empty;

    /// <summary>Free text as typed («10», «300 г»); null when not given.</summary>
    public string? Quantity { get; private set; }

    public string? Note { get; private set; }

    public Guid DepartmentId { get; private set; }

    /// <summary>«Срочно»: shown first in its department.</summary>
    public bool Urgent { get; private set; }

    public ShoppingItemStatus Status { get; private set; }

    public Person AddedBy { get; private set; }

    public DateTimeOffset AddedAtUtc { get; private set; }

    public Person? BoughtBy { get; private set; }

    public DateTimeOffset? BoughtAtUtc { get; private set; }

    /// <summary>Set by «Убрать купленные»: the item leaves the cart for history before its time.</summary>
    public DateTimeOffset? ClearedAtUtc { get; private set; }

    /// <summary>Set when the row came from the sheet import; unique, so a repeated import skips it.</summary>
    public string? ImportKey { get; private init; }

    /// <summary>Changes on every save (PostgreSQL <c>xmin</c>); an edit based on an older version is refused.</summary>
    public uint Version { get; private set; }

    public static Result<ShoppingItem> Create(
        string name,
        string? quantity,
        string? note,
        Guid departmentId,
        bool urgent,
        Person addedBy,
        DateTimeOffset now,
        string? importKey = null)
    {
        var check = Check(name, quantity, note);
        if (check.IsFailure)
            return check.Error!;

        return new ShoppingItem
        {
            Id = Guid.CreateVersion7(now),
            Name = name.Trim(),
            NormalizedName = NameNormalizer.Normalize(name),
            Quantity = Clean(quantity),
            Note = Clean(note),
            DepartmentId = departmentId,
            Urgent = urgent,
            Status = ShoppingItemStatus.Needed,
            AddedBy = addedBy,
            AddedAtUtc = now,
            ImportKey = importKey,
        };
    }

    /// <summary>The edit dialog: replaces name, quantity, note, department and the urgent flag.</summary>
    public Result Update(string name, string? quantity, string? note, Guid departmentId, bool urgent)
    {
        var check = Check(name, quantity, note);
        if (check.IsFailure)
            return check;

        Name = name.Trim();
        NormalizedName = NameNormalizer.Normalize(name);
        Quantity = Clean(quantity);
        Note = Clean(note);
        DepartmentId = departmentId;
        Urgent = urgent;
        return Result.Success();
    }

    /// <summary>
    /// The same name is added again while this item is in the list (the bot's re-add rule): a needed item is
    /// refreshed, an item in the cart is needed again, added now by this person.
    /// </summary>
    public ReAddOutcome AddAgain(string? quantity, bool urgent, Person addedBy, DateTimeOffset now)
    {
        if (Status == ShoppingItemStatus.Needed)
        {
            Refresh(quantity, urgent);
            return ReAddOutcome.Updated;
        }

        Restore(quantity, urgent, addedBy, now);
        return ReAddOutcome.Restored;
    }

    /// <summary>
    /// The same item is added again while it is needed: it stays, the quantity is replaced when a new one is given,
    /// and «Срочно» is switched on when asked (never off).
    /// </summary>
    private void Refresh(string? quantity, bool urgent)
    {
        if (Clean(quantity) is { } given)
            Quantity = Cut(given, MaxQuantityLength);
        Urgent |= urgent;
    }

    /// <summary>The same item is added again while it is in the cart: it is needed again, added now by this person.</summary>
    private void Restore(string? quantity, bool urgent, Person addedBy, DateTimeOffset now)
    {
        Refresh(quantity, urgent);
        Status = ShoppingItemStatus.Needed;
        BoughtBy = null;
        BoughtAtUtc = null;
        ClearedAtUtc = null;
        AddedBy = addedBy;
        AddedAtUtc = now;
    }

    /// <summary>
    /// Sets the status to a target value, so a repeated or concurrent tick does nothing: «bought» keeps the first
    /// buyer and time, «needed» forgets them.
    /// </summary>
    public void SetStatus(ShoppingItemStatus status, Person by, DateTimeOffset now)
    {
        if (status == Status)
            return;

        Status = status;
        ClearedAtUtc = null;
        if (status == ShoppingItemStatus.Bought)
        {
            BoughtBy = by;
            BoughtAtUtc = now;
        }
        else
        {
            BoughtBy = null;
            BoughtAtUtc = null;
        }
    }

    private static Result Check(string name, string? quantity, string? note) =>
        Result.Combine(
            Result.Ensure(!string.IsNullOrWhiteSpace(name), ShoppingItemErrors.NameRequired),
            Result.Ensure(name is null || name.Trim().Length <= MaxNameLength, ShoppingItemErrors.NameTooLong),
            Result.Ensure(Clean(quantity) is not { Length: > MaxQuantityLength }, ShoppingItemErrors.QuantityTooLong),
            Result.Ensure(Clean(note) is not { Length: > MaxNoteLength }, ShoppingItemErrors.NoteTooLong));

    private static string? Clean(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    private static string Cut(string text, int length) => text.Length > length ? text[..length] : text;
}
