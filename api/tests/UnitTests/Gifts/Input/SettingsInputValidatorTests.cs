using Gifts.Application.Features.Categories.Commands.CreateCategory;
using Gifts.Application.Features.Categories.Commands.RenameCategory;
using Gifts.Application.Features.Details.Commands.CreateDetail;
using Gifts.Application.Features.Details.Commands.UpdateDetail;
using Gifts.Application.Features.Items.Commands.ChangeGiftStatus;
using Gifts.Domain.People;
using Gifts.Domain.Wishlist;

namespace UnitTests.Gifts.Input;

public sealed class SettingsInputValidatorTests
{
    [Fact]
    public void A_Category_NeedsAName_AndACreate_AKey()
    {
        Assert.True(new CreateCategoryValidator().Validate(new CreateCategoryCommand("key", "Книги")).IsValid);
        Assert.False(new CreateCategoryValidator().Validate(new CreateCategoryCommand("", "Книги")).IsValid);
        Assert.False(new CreateCategoryValidator().Validate(new CreateCategoryCommand("key", "")).IsValid);
        Assert.True(new RenameCategoryValidator().Validate(new RenameCategoryCommand(Guid.NewGuid(), "Книги")).IsValid);
        Assert.False(new RenameCategoryValidator().Validate(new RenameCategoryCommand(Guid.NewGuid(), new string('a', 101))).IsValid);
    }

    [Fact]
    public void A_Detail_NeedsALabel_AValue_AndForAnEdit_TheVersion()
    {
        Assert.True(new CreateDetailValidator().Validate(new CreateDetailCommand("key", Recipient.Maria, "Кольцо", "18,5")).IsValid);
        Assert.False(new CreateDetailValidator().Validate(new CreateDetailCommand("key", Recipient.Maria, "", "18,5")).IsValid);
        Assert.False(new CreateDetailValidator().Validate(new CreateDetailCommand("key", (Recipient)7, "Кольцо", "18,5")).IsValid);
        Assert.True(new UpdateDetailValidator().Validate(new UpdateDetailCommand(Guid.NewGuid(), 3, Recipient.Maria, "Кольцо", "19")).IsValid);
        Assert.False(new UpdateDetailValidator().Validate(new UpdateDetailCommand(Guid.NewGuid(), 0, Recipient.Maria, "Кольцо", "19")).IsValid);
    }

    [Fact]
    public void A_StatusChange_NeedsAKnownStatus_AndTheVersion()
    {
        var validator = new ChangeGiftStatusValidator();

        Assert.True(validator.Validate(new ChangeGiftStatusCommand(Guid.NewGuid(), 4, GiftStatus.Gifted, null)).IsValid);
        Assert.False(validator.Validate(new ChangeGiftStatusCommand(Guid.NewGuid(), 0, GiftStatus.Gifted, null)).IsValid);
        Assert.False(validator.Validate(new ChangeGiftStatusCommand(Guid.NewGuid(), 4, (GiftStatus)9, null)).IsValid);
    }
}
