using Gifts.Domain.Categories;
using Gifts.Domain.People;

namespace UnitTests.Gifts.Domain;

public sealed class CategoryAndDetailTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void A_Category_HasATrimmedName_OfAtMost100Characters()
    {
        Assert.Equal("Книги", Category.Create("  Книги ", 120, Now).Value.Name);
        Assert.Equal("gifts.category_name_required", Category.Create(" ", 120, Now).Error!.Code);
        Assert.Equal("gifts.category_name_too_long", Category.Create(new string('a', 101), 120, Now).Error!.Code);
    }

    [Fact]
    public void A_Rename_KeepsThePosition_AndABrokenOne_ChangesNothing()
    {
        var category = Category.Create("Книги", 120, Now).Value;

        Assert.True(category.Rename("Книги и комиксы").IsSuccess);
        Assert.False(category.Rename("").IsSuccess);

        Assert.Equal("Книги и комиксы", category.Name);
        Assert.Equal(120, category.SortOrder);
    }

    [Fact]
    public void A_Detail_HasALabel_AndAValue()
    {
        var detail = Detail.Create(Recipient.Maria, " Кольцо ", " 18,5–19,0 ", 10, Now).Value;

        Assert.Equal("Кольцо", detail.Label);
        Assert.Equal("18,5–19,0", detail.Value);
        Assert.Equal("gifts.detail_label_required", Detail.Create(Recipient.Maria, "", "1", 10, Now).Error!.Code);
        Assert.Equal("gifts.detail_value_required", Detail.Create(Recipient.Maria, "Обувь", " ", 10, Now).Error!.Code);
        Assert.Equal("gifts.detail_label_too_long", Detail.Create(Recipient.Maria, new string('a', 101), "1", 10, Now).Error!.Code);
        Assert.Equal("gifts.detail_value_too_long", Detail.Create(Recipient.Maria, "Обувь", new string('a', 201), 10, Now).Error!.Code);
    }

    [Fact]
    public void A_Detail_Update_KeepsThePosition()
    {
        var detail = Detail.Create(Recipient.Maria, "Обувь", "38", 20, Now).Value;

        Assert.True(detail.Update(Recipient.Together, "Обувь", "38,5").IsSuccess);

        Assert.Equal(Recipient.Together, detail.Recipient);
        Assert.Equal("38,5", detail.Value);
        Assert.Equal(20, detail.SortOrder);
    }
}
