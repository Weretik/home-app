using BuildingBlocks.Domain.Modeling;
using BuildingBlocks.Domain.Results;
using Gifts.Domain.People;

namespace Gifts.Domain.Wishlist;

/// <summary>
/// A gift idea for one of the two people or for both: one row of the old «🎁 Список подарков» sheet, with its variants
/// (other shops or models, each with a link). It is <see cref="GiftStatus.Wanted"/> until it is gifted or dropped,
/// then it is in the archive.
/// </summary>
public sealed class Gift : Entity<Guid>, IAggregateRoot
{
    public const int MaxNameLength = 200;
    public const int MaxLinkLength = 2000;
    public const int MaxOccasionLength = 100;
    public const int MaxNotesLength = 2000;
    public const int MaxVariants = 20;
    public const decimal MaxPrice = 10_000_000m;

    private static readonly DateOnly MinDate = new(2000, 1, 1);
    private static readonly DateOnly MaxDate = new(2100, 12, 31);

    private List<GiftVariant> _variants = [];

    private Gift()
    {
    }

    public Recipient Recipient { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public Guid CategoryId { get; private set; }

    public Priority Priority { get; private set; }

    /// <summary>Null when the price is not known yet.</summary>
    public decimal? Price { get; private set; }

    public string? Link { get; private set; }

    public GiftStatus Status { get; private set; }

    /// <summary>The day it was given or bought; set only while the status is <see cref="GiftStatus.Gifted"/>.</summary>
    public DateOnly? GiftedOn { get; private set; }

    /// <summary>What it is for, such as «День рождения» or «Новый год».</summary>
    public string? Occasion { get; private set; }

    public DateOnly? OccasionDate { get; private set; }

    public string? Notes { get; private set; }

    public IReadOnlyList<GiftVariant> Variants => _variants;

    public DateTimeOffset CreatedAtUtc { get; private init; }

    /// <summary>Changes on every save (PostgreSQL <c>xmin</c>); an edit based on an older version is refused.</summary>
    public uint Version { get; private set; }

    /// <summary>A new idea; it starts in the active list.</summary>
    public static Result<Gift> Create(
        Recipient recipient,
        string name,
        Guid categoryId,
        Priority priority,
        decimal? price,
        string? link,
        string? occasion,
        DateOnly? occasionDate,
        string? notes,
        IReadOnlyList<VariantDraft> variants,
        DateTimeOffset now)
    {
        var gift = new Gift { Id = Guid.CreateVersion7(now), Status = GiftStatus.Wanted, CreatedAtUtc = now };
        var set = gift.Update(recipient, name, categoryId, priority, price, link, occasion, occasionDate, notes, variants, now);
        return set.IsFailure ? set.Error! : gift;
    }

    /// <summary>
    /// Replaces everything a person edits, the variants as a whole list in the given order. The status, the gifted
    /// day and the creation time stay. Nothing changes when a rule is broken.
    /// </summary>
    public Result Update(
        Recipient recipient,
        string name,
        Guid categoryId,
        Priority priority,
        decimal? price,
        string? link,
        string? occasion,
        DateOnly? occasionDate,
        string? notes,
        IReadOnlyList<VariantDraft> variants,
        DateTimeOffset now)
    {
        variants ??= [];
        var check = Result.Combine(
            Result.Ensure(!string.IsNullOrWhiteSpace(name), GiftErrors.NameRequired),
            Result.Ensure(name is null || name.Trim().Length <= MaxNameLength, GiftErrors.NameTooLong),
            Result.Ensure(price is null || IsPrice(price.Value), GiftErrors.PriceInvalid),
            Result.Ensure(Normalize(link) is null || IsLink(link!), GiftErrors.LinkInvalid),
            Result.Ensure(Normalize(occasion) is not { Length: > MaxOccasionLength }, GiftErrors.OccasionTooLong),
            Result.Ensure(occasionDate is null || IsInRange(occasionDate.Value), GiftErrors.OccasionDateOutOfRange),
            Result.Ensure(Normalize(notes) is not { Length: > MaxNotesLength }, GiftErrors.NotesTooLong),
            Result.Ensure(variants.Count <= MaxVariants, GiftErrors.TooManyVariants),
            Result.Ensure(variants.All(IsVariant), GiftErrors.VariantInvalid));
        if (check.IsFailure)
            return check;

        Recipient = recipient;
        Name = name!.Trim();
        CategoryId = categoryId;
        Priority = priority;
        Price = price;
        Link = Normalize(link);
        Occasion = Normalize(occasion);
        OccasionDate = occasionDate;
        Notes = Normalize(notes);
        _variants = [.. variants.Select((draft, position) => GiftVariant.From(draft, position, now))];
        return Result.Success();
    }

    /// <summary>
    /// Moves the gift between the active list and the archive. <see cref="GiftStatus.Gifted"/> records the day it was
    /// given: the day sent, else the day it already has (so a repeated «Подарено» changes nothing), else
    /// <paramref name="today"/>. Any other status clears it.
    /// </summary>
    public Result ChangeStatus(GiftStatus status, DateOnly? giftedOn, DateOnly today)
    {
        if (status != GiftStatus.Gifted)
        {
            Status = status;
            GiftedOn = null;
            return Result.Success();
        }

        var day = giftedOn ?? (Status == GiftStatus.Gifted ? GiftedOn : null) ?? today;
        if (!IsInRange(day))
            return GiftErrors.GiftedOnOutOfRange;

        Status = status;
        GiftedOn = day;
        return Result.Success();
    }

    /// <summary>Free text is trimmed; an empty one is stored as nothing.</summary>
    internal static string? Normalize(string? text) =>
        string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    private static bool IsVariant(VariantDraft? variant) =>
        variant is not null
        && variant.Link is not null
        && IsLink(variant.Link)
        && Normalize(variant.Title) is not { Length: > MaxNameLength }
        && (variant.Price is null || IsPrice(variant.Price.Value));

    private static bool IsPrice(decimal price) =>
        price > 0 && price <= MaxPrice && decimal.Round(price, 2) == price;

    private static bool IsLink(string link)
    {
        var trimmed = link.Trim();
        return trimmed.Length is > 0 and <= MaxLinkLength
            && Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static bool IsInRange(DateOnly day) => day >= MinDate && day <= MaxDate;
}
