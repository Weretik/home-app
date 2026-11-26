using Gifts.Application.Features.Items.Commands.CreateGift;
using Gifts.Application.Features.Items.Commands.UpdateGift;
using Gifts.Domain.People;
using Gifts.Domain.Wishlist;

namespace UnitTests.Gifts.Input;

public sealed class GiftInputValidatorTests
{
    private static readonly Guid Category = Guid.NewGuid();

    private static CreateGiftCommand Create(
        string key = "key",
        string name = "Кольцо",
        decimal? price = null,
        IReadOnlyList<VariantDraft>? variants = null) =>
        new(key, Recipient.Maria, name, Category, Priority.High, price, null, null, null, null, variants ?? []);

    [Fact]
    public void A_Create_NeedsAKey_AName_AndACategory()
    {
        var validator = new CreateGiftValidator();

        Assert.True(validator.Validate(Create()).IsValid);
        Assert.False(validator.Validate(Create(key: "")).IsValid);
        Assert.False(validator.Validate(Create(key: new string('k', 101))).IsValid);
        Assert.False(validator.Validate(Create(name: "")).IsValid);
        Assert.False(validator.Validate(Create() with { CategoryId = Guid.Empty }).IsValid);
        Assert.False(validator.Validate(Create() with { Recipient = (Recipient)9 }).IsValid);
        Assert.False(validator.Validate(Create() with { Priority = (Priority)9 }).IsValid);
    }

    [Fact]
    public void A_Price_IsOptional_ButPositive()
    {
        var validator = new CreateGiftValidator();

        Assert.True(validator.Validate(Create(price: null)).IsValid);
        Assert.True(validator.Validate(Create(price: 1m)).IsValid);
        Assert.False(validator.Validate(Create(price: 0m)).IsValid);
    }

    [Fact]
    public void Variants_AreAtMost20_EachWithALink()
    {
        var validator = new CreateGiftValidator();
        var many = Enumerable.Range(0, 21).Select(i => new VariantDraft(null, $"https://x.example/{i}", null)).ToList();

        Assert.True(validator.Validate(Create(variants: [new VariantDraft("A", "https://a.example", 10m)])).IsValid);
        Assert.False(validator.Validate(Create(variants: many)).IsValid);
        Assert.False(validator.Validate(Create(variants: [new VariantDraft("A", "", null)])).IsValid);
        Assert.False(validator.Validate(Create(variants: [new VariantDraft("A", "https://a.example", -1m)])).IsValid);
    }

    [Fact]
    public void An_Update_NeedsTheIdAndTheVersion()
    {
        var validator = new UpdateGiftValidator();
        var update = new UpdateGiftCommand(Guid.NewGuid(), 5, Recipient.Vitaliy, "Наушники", Category, Priority.Normal, null, null, null, null, null, []);

        Assert.True(validator.Validate(update).IsValid);
        Assert.False(validator.Validate(update with { Version = 0 }).IsValid);
        Assert.False(validator.Validate(update with { Id = Guid.Empty }).IsValid);
        Assert.False(validator.Validate(update with { Name = new string('a', 201) }).IsValid);
    }
}
