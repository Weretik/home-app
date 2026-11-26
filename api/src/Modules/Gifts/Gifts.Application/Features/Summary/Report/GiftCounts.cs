namespace Gifts.Application.Features.Summary.Report;

/// <summary>
/// The figures of one recipient or of everyone. Sums count only the gifts with a known price; dropped gifts count
/// nowhere.
/// </summary>
public sealed record GiftCounts(
    int WantedCount,
    decimal WantedPriceSum,
    int WantedWithoutPrice,
    int HighCount,
    int GiftedCount,
    decimal GiftedPriceSum);
