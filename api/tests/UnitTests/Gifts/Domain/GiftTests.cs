using BuildingBlocks.Domain.Results;
using Gifts.Domain.Categories;
using Gifts.Domain.People;
using Gifts.Domain.Wishlist;

namespace UnitTests.Gifts.Domain;

public sealed class GiftTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);
    private static readonly CategoryId Jewellery = new(Guid.NewGuid());

    private static Result<Gift> Create(
        string name = "Кольцо",
        decimal? price = null,
        string? link = null,
        string? occasion = null,
        DateOnly? occasionDate = null,
        string? notes = null,
        IReadOnlyList<VariantDraft>? variants = null) =>
        Gift.Create(Recipient.Maria, name, Jewellery, Priority.High, price, link, occasion, occasionDate, notes, variants ?? [], Now);

    [Fact]
    public void A_New_Gift_IsWanted_WithTrimmedText_AndVariantsInOrder()
    {
        var result = Create(
            name: "  Кольцо с камнем  ",
            occasion: "  День рождения ",
            occasionDate: new DateOnly(2040, 3, 14),
            notes: "   ",
            variants:
            [
                new VariantDraft(" Pandora ", "https://pandora.example/ring", 4_500m),
                new VariantDraft(null, "https://shop.example/r", null),
            ]);

        Assert.True(result.IsSuccess);
        var gift = result.Value;
        Assert.Equal(GiftStatus.Wanted, gift.Status);
        Assert.Null(gift.GiftedOn);
        Assert.Equal("Кольцо с камнем", gift.Name);
        Assert.Equal("День рождения", gift.Occasion);
        Assert.Null(gift.Notes);
        Assert.Null(gift.Price);
        Assert.Equal(Now, gift.CreatedAtUtc);
        Assert.Equal(["Pandora", null], gift.Variants.Select(v => v.Title));
        Assert.Equal([0, 1], gift.Variants.Select(v => v.SortOrder));
        Assert.Equal(4_500m, gift.Variants[0].Price);
    }

    [Theory]
    [InlineData("", "gifts.name_required")]
    [InlineData("   ", "gifts.name_required")]
    public void A_Gift_NeedsAName(string name, string code) =>
        Assert.Equal(code, Create(name: name).Error!.Code);

    [Fact]
    public void A_Name_IsAtMost200Characters()
    {
        Assert.True(Create(name: new string('a', 200)).IsSuccess);
        Assert.Equal("gifts.name_too_long", Create(name: new string('a', 201)).Error!.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(10_000_000.01)]
    [InlineData(1.234)]
    public void A_Price_WhenKnown_FollowsTheMoneyRules(decimal price) =>
        Assert.Equal("gifts.price_invalid", Create(price: price).Error!.Code);

    [Theory]
    [InlineData("ftp://example.com/x")]
    [InlineData("example.com")]
    [InlineData("/relative/path")]
    public void A_Link_IsAnAbsoluteWebAddress(string link) =>
        Assert.Equal("gifts.link_invalid", Create(link: link).Error!.Code);

    [Fact]
    public void A_Link_IsAtMost2000Characters_AndEmptyMeansNone()
    {
        Assert.Equal("gifts.link_invalid", Create(link: "https://x.example/" + new string('a', 1983)).Error!.Code);
        Assert.Null(Create(link: "  ").Value.Link);
        Assert.Equal("http://x.example/a", Create(link: " http://x.example/a ").Value.Link);
    }

    [Fact]
    public void Occasion_Date_AndNotes_HaveLimits()
    {
        Assert.Equal("gifts.occasion_too_long", Create(occasion: new string('a', 101)).Error!.Code);
        Assert.Equal("gifts.occasion_date_out_of_range", Create(occasionDate: new DateOnly(1999, 12, 31)).Error!.Code);
        Assert.Equal("gifts.occasion_date_out_of_range", Create(occasionDate: new DateOnly(2101, 1, 1)).Error!.Code);
        Assert.Equal("gifts.notes_too_long", Create(notes: new string('a', 2001)).Error!.Code);
    }

    [Fact]
    public void A_Gift_HasAtMost20Variants_EachWithAValidLink()
    {
        var twenty = Enumerable.Range(0, 20).Select(i => new VariantDraft(null, $"https://x.example/{i}", null)).ToList();

        Assert.True(Create(variants: twenty).IsSuccess);
        Assert.Equal("gifts.too_many_variants", Create(variants: [.. twenty, new VariantDraft(null, "https://x.example/21", null)]).Error!.Code);
        Assert.Equal("gifts.variant_invalid", Create(variants: [new VariantDraft("Без ссылки", "", null)]).Error!.Code);
        Assert.Equal("gifts.variant_invalid", Create(variants: [new VariantDraft(null, "https://x.example", 0m)]).Error!.Code);
        Assert.Equal("gifts.variant_invalid", Create(variants: [new VariantDraft(new string('a', 201), "https://x.example", null)]).Error!.Code);
    }

    [Fact]
    public void An_Update_ReplacesTheWholeVariantList_AndKeepsTheStatus()
    {
        var gift = Create(variants: [new VariantDraft("A", "https://a.example", null), new VariantDraft("B", "https://b.example", null)]).Value;
        gift.ChangeStatus(GiftStatus.Gifted, new DateOnly(2026, 11, 20), new DateOnly(2026, 11, 26));
        var oldIds = gift.Variants.Select(v => v.Id).ToList();

        var updated = gift.Update(
            Recipient.Together, "Плед", Jewellery, Priority.Someday, 900m, "https://home.example", null, null, "мягкий",
            [new VariantDraft("C", "https://c.example", 950m)], Now.AddDays(1));

        Assert.True(updated.IsSuccess);
        Assert.Equal("Плед", gift.Name);
        Assert.Equal(Recipient.Together, gift.Recipient);
        Assert.Equal(GiftStatus.Gifted, gift.Status);
        Assert.Equal(new DateOnly(2026, 11, 20), gift.GiftedOn);
        Assert.Equal(Now, gift.CreatedAtUtc);
        var variant = Assert.Single(gift.Variants);
        Assert.Equal("C", variant.Title);
        Assert.DoesNotContain(variant.Id, oldIds);
    }

    [Fact]
    public void A_Broken_Update_ChangesNothing()
    {
        var gift = Create(variants: [new VariantDraft("A", "https://a.example", null)]).Value;

        var updated = gift.Update(
            Recipient.Vitaliy, "Новое", Jewellery, Priority.Normal, -1m, null, null, null, null, [], Now);

        Assert.Equal("gifts.price_invalid", updated.Error!.Code);
        Assert.Equal("Кольцо", gift.Name);
        Assert.Equal(Recipient.Maria, gift.Recipient);
        Assert.Single(gift.Variants);
    }
}
