using Gifts.Application.Features.Summary.Queries.GetGiftsSummary;
using Gifts.Domain.People;
using Gifts.Domain.Wishlist;

namespace UnitTests.Gifts.Summary;

/// <summary>The gifts summary's counts and sums per recipient and in total (backend SDD gifts 001, the summary).</summary>
public sealed class SummaryCountsTests
{
    private static readonly DateOnly Today = new(2026, 11, 26);
    private static readonly DateTimeOffset Created = new(2026, 11, 1, 10, 0, 0, TimeSpan.Zero);

    private static SummaryRow Row(Recipient recipient, GiftStatus status, decimal? price, Priority priority = Priority.Normal) =>
        new(Guid.NewGuid(), recipient, "подарок", priority, price, null, status, null, null, Created);

    [Fact]
    public void Every_Recipient_IsListed_InOrder_EvenWithoutGifts()
    {
        var summary = SummaryFigures.Build([], [], Today);

        Assert.Equal([Recipient.Maria, Recipient.Vitaliy, Recipient.Together], summary.Recipients.Select(r => r.Recipient));
        Assert.All(summary.Recipients, r => Assert.Equal(0, r.Counts.WantedCount));
        Assert.Equal(0, summary.Total.WantedCount);
    }

    [Fact]
    public void Sums_CountOnlyKnownPrices_AndDroppedGifts_CountNowhere()
    {
        SummaryRow[] rows =
        [
            Row(Recipient.Maria, GiftStatus.Wanted, 1_000m, Priority.High),
            Row(Recipient.Maria, GiftStatus.Wanted, null),
            Row(Recipient.Maria, GiftStatus.Gifted, 2_500m),
            Row(Recipient.Maria, GiftStatus.Dropped, 9_999m),
            Row(Recipient.Vitaliy, GiftStatus.Wanted, 300m),
        ];

        var summary = SummaryFigures.Build(rows, [], Today);

        var maria = summary.Recipients.Single(r => r.Recipient == Recipient.Maria).Counts;
        Assert.Equal(2, maria.WantedCount);
        Assert.Equal(1_000m, maria.WantedPriceSum);
        Assert.Equal(1, maria.WantedWithoutPrice);
        Assert.Equal(1, maria.HighCount);
        Assert.Equal(1, maria.GiftedCount);
        Assert.Equal(2_500m, maria.GiftedPriceSum);
        Assert.Equal(3, summary.Total.WantedCount);
        Assert.Equal(1_300m, summary.Total.WantedPriceSum);
        Assert.Equal(2_500m, summary.Total.GiftedPriceSum);
    }
}
