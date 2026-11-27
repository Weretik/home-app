using Gifts.Application;
using Gifts.Domain.Categories;
using Gifts.Domain.People;
using Gifts.Domain.Wishlist;
using Microsoft.Extensions.Time.Testing;

namespace UnitTests.Gifts.Domain;

public sealed class GiftStatusTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = new(2026, 11, 26);

    private static Gift NewGift() =>
        Gift.Create(Recipient.Vitaliy, "Наушники", new CategoryId(Guid.NewGuid()), Priority.Normal, 3_000m, null, null, null, null, [], Now).Value;

    [Fact]
    public void Gifted_WithoutADay_IsGivenToday()
    {
        var gift = NewGift();

        Assert.True(gift.ChangeStatus(GiftStatus.Gifted, null, Today).IsSuccess);

        Assert.Equal(GiftStatus.Gifted, gift.Status);
        Assert.Equal(Today, gift.GiftedOn);
    }

    [Fact]
    public void Gifted_WithADay_KeepsThatDay_AndARepeat_ChangesNothing()
    {
        var gift = NewGift();
        gift.ChangeStatus(GiftStatus.Gifted, new DateOnly(2026, 6, 1), Today);

        gift.ChangeStatus(GiftStatus.Gifted, null, Today.AddDays(3));

        Assert.Equal(new DateOnly(2026, 6, 1), gift.GiftedOn);
    }

    [Fact]
    public void Dropped_Or_BackToWanted_ClearsTheDay()
    {
        var gift = NewGift();
        gift.ChangeStatus(GiftStatus.Gifted, null, Today);

        gift.ChangeStatus(GiftStatus.Wanted, null, Today);
        Assert.Equal(GiftStatus.Wanted, gift.Status);
        Assert.Null(gift.GiftedOn);

        gift.ChangeStatus(GiftStatus.Dropped, new DateOnly(2026, 1, 1), Today);
        Assert.Equal(GiftStatus.Dropped, gift.Status);
        Assert.Null(gift.GiftedOn);
    }

    [Fact]
    public void A_GiftedDay_OutsideTheCalendar_IsRefused_AndChangesNothing()
    {
        var gift = NewGift();

        var result = gift.ChangeStatus(GiftStatus.Gifted, new DateOnly(1999, 1, 1), Today);

        Assert.Equal("gifts.gifted_on_out_of_range", result.Error!.Code);
        Assert.Equal(GiftStatus.Wanted, gift.Status);
    }

    [Fact]
    public void Today_IsTheDateInKyiv()
    {
        // 23:30 UTC on 26 November is already 27 November in Kyiv (UTC+2 in winter).
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 11, 26, 23, 30, 0, TimeSpan.Zero));

        Assert.Equal(new DateOnly(2026, 11, 27), HouseholdDay.Today(clock));
    }
}
