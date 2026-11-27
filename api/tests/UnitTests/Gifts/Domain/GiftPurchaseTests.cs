using Gifts.Application.Purchases;
using Gifts.Contracts;
using Gifts.Domain.Categories;
using Gifts.Domain.People;
using Gifts.Domain.Wishlist;

namespace UnitTests.Gifts.Domain;

public sealed class GiftPurchaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = new(2026, 11, 26);
    private static readonly GiftLinks Links = new("https://web.example/");

    private static Gift Ring() =>
        Gift.Create(Recipient.Maria, "Кольцо", new CategoryId(Guid.NewGuid()), Priority.High, null, null, null, null, null, [], Now).Value;

    [Fact]
    public void A_Gift_IsPlanned_WhenWanted_WithAGiverAndABuyByDate()
    {
        var gift = Ring();
        Assert.False(gift.IsPurchasePlanned);

        Assert.True(gift.PlanPurchase(Recipient.Vitaliy, new DateOnly(2026, 12, 20)).IsSuccess);
        Assert.True(gift.IsPurchasePlanned);

        Assert.Equal("gifts.buy_by_out_of_range", gift.PlanPurchase(Recipient.Vitaliy, new DateOnly(1999, 12, 31)).Error!.Code);
        Assert.Equal(new DateOnly(2026, 12, 20), gift.BuyBy);
    }

    [Fact]
    public void Planning_PublishesPlanned_WithTheGiftLink_AndAnUnchangedPlan_PublishesNothing()
    {
        var gift = Ring();
        gift.PlanPurchase(Recipient.Vitaliy, new DateOnly(2026, 12, 20));

        var planned = Assert.IsType<GiftPurchasePlanned>(PurchasePlan.Change(null, gift, Links));

        Assert.Equal(GiftParty.Maria, planned.Recipient);
        Assert.Equal(GiftParty.Vitaliy, planned.Giver);
        Assert.Equal($"https://web.example/gifts?gift={gift.Id}", planned.Link);
        Assert.Null(PurchasePlan.Change(PurchasePlan.Of(gift), gift, Links));
    }

    [Fact]
    public void A_Moved_Date_PublishesPlannedAgain()
    {
        var gift = Ring();
        gift.PlanPurchase(Recipient.Vitaliy, new DateOnly(2026, 12, 20));
        var before = PurchasePlan.Of(gift);

        gift.PlanPurchase(Recipient.Vitaliy, new DateOnly(2026, 12, 18));

        Assert.Equal(new DateOnly(2026, 12, 18), Assert.IsType<GiftPurchasePlanned>(PurchasePlan.Change(before, gift, Links)).BuyBy);
    }

    [Theory]
    [InlineData(GiftStatus.Gifted, GiftPurchaseOutcome.Gifted)]
    [InlineData(GiftStatus.Dropped, GiftPurchaseOutcome.Dropped)]
    public void Leaving_Wanted_ClosesThePurchase_WithWhy(GiftStatus status, GiftPurchaseOutcome outcome)
    {
        var gift = Ring();
        gift.PlanPurchase(Recipient.Together, new DateOnly(2026, 12, 20));
        var before = PurchasePlan.Of(gift);

        gift.ChangeStatus(status, status == GiftStatus.Gifted ? Today : null, Today);

        Assert.Equal(new GiftPurchaseClosed(gift.Id.Value, outcome), PurchasePlan.Change(before, gift, Links));
    }

    [Fact]
    public void Clearing_TheGiver_ClosesItAsUnplanned_AndAnUnplannedGift_PublishesNothing()
    {
        var gift = Ring();
        gift.PlanPurchase(Recipient.Vitaliy, new DateOnly(2026, 12, 20));
        var before = PurchasePlan.Of(gift);

        gift.PlanPurchase(null, new DateOnly(2026, 12, 20));

        Assert.Equal(new GiftPurchaseClosed(gift.Id.Value, GiftPurchaseOutcome.Unplanned), PurchasePlan.Change(before, gift, Links));
        Assert.Null(PurchasePlan.Change(null, gift, Links));
    }
}
