using Shopping.Domain.Items;

namespace UnitTests.Shopping.Domain;

public sealed class ShoppingItemTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 10, 0, 0, TimeSpan.Zero);
    private static readonly Guid Dairy = Guid.NewGuid();

    [Fact]
    public void A_NewItem_IsNeeded_WithItsNormalisedName()
    {
        var item = ShoppingItem.Create("  Зелёный  Чай ", " 2 уп ", "  ", Dairy, urgent: true, Person.Maria, Now).Value;

        Assert.Equal("Зелёный  Чай", item.Name);
        Assert.Equal("зеленый чай", item.NormalizedName);
        Assert.Equal("2 уп", item.Quantity);
        Assert.Null(item.Note);
        Assert.True(item.Urgent);
        Assert.Equal(ShoppingItemStatus.Needed, item.Status);
        Assert.Equal(Person.Maria, item.AddedBy);
        Assert.Equal(Now, item.AddedAtUtc);
        Assert.Null(item.BoughtBy);
    }

    [Theory]
    [InlineData("", null, null, "shopping.item_name_required")]
    [InlineData("   ", null, null, "shopping.item_name_required")]
    [InlineData("x", "1234567890123456789012345678901", null, "shopping.quantity_too_long")]
    [InlineData("x", null, "n", null)]
    public void The_Fields_AreChecked(string name, string? quantity, string? note, string? error)
    {
        var result = ShoppingItem.Create(name, quantity, note, Dairy, false, Person.Vitaliy, Now);

        Assert.Equal(error, result.Error?.Code);
    }

    [Fact]
    public void Long_NamesAndNotes_AreRefused()
    {
        Assert.Equal("shopping.item_name_too_long",
            ShoppingItem.Create(new string('a', 81), null, null, Dairy, false, Person.Vitaliy, Now).Error!.Code);
        Assert.Equal("shopping.note_too_long",
            ShoppingItem.Create("a", null, new string('a', 201), Dairy, false, Person.Vitaliy, Now).Error!.Code);
    }

    [Fact]
    public void Marking_Bought_Twice_KeepsTheFirstBuyer()
    {
        var item = New();

        item.SetStatus(ShoppingItemStatus.Bought, Person.Vitaliy, Now);
        item.SetStatus(ShoppingItemStatus.Bought, Person.Maria, Now.AddMinutes(5));

        Assert.Equal(ShoppingItemStatus.Bought, item.Status);
        Assert.Equal(Person.Vitaliy, item.BoughtBy);
        Assert.Equal(Now, item.BoughtAtUtc);
    }

    [Fact]
    public void Marking_Needed_Again_ForgetsTheBuyer()
    {
        var item = New();
        item.SetStatus(ShoppingItemStatus.Bought, Person.Vitaliy, Now);

        item.SetStatus(ShoppingItemStatus.Needed, Person.Maria, Now.AddMinutes(1));

        Assert.Equal(ShoppingItemStatus.Needed, item.Status);
        Assert.Null(item.BoughtBy);
        Assert.Null(item.BoughtAtUtc);
    }

    [Fact]
    public void ReAdding_ANeededItem_ReplacesTheQuantity_OnlyWhenGiven_AndNeverClearsUrgent()
    {
        var item = ShoppingItem.Create("Молоко", "1", null, Dairy, urgent: true, Person.Vitaliy, Now).Value;

        Assert.Equal(ReAddOutcome.Updated, item.AddAgain(null, urgent: false, Person.Maria, Now.AddHours(1)));
        Assert.Equal("1", item.Quantity);
        Assert.True(item.Urgent);
        Assert.Equal(Person.Vitaliy, item.AddedBy);

        item.AddAgain("2", urgent: false, Person.Maria, Now.AddHours(1));
        Assert.Equal("2", item.Quantity);
        Assert.Equal(Now, item.AddedAtUtc);
    }

    [Fact]
    public void ReAdding_AnItemInTheCart_MakesItNeeded_AddedNow_ByThatPerson()
    {
        var item = ShoppingItem.Create("Хлеб", "1", null, Dairy, false, Person.Vitaliy, Now).Value;
        item.SetStatus(ShoppingItemStatus.Bought, Person.Vitaliy, Now.AddHours(1));

        var outcome = item.AddAgain(null, urgent: true, Person.Maria, Now.AddHours(2));

        Assert.Equal(ReAddOutcome.Restored, outcome);
        Assert.Equal(ShoppingItemStatus.Needed, item.Status);
        Assert.Null(item.BoughtBy);
        Assert.Null(item.BoughtAtUtc);
        Assert.Null(item.ClearedAtUtc);
        Assert.Equal(Person.Maria, item.AddedBy);
        Assert.Equal(Now.AddHours(2), item.AddedAtUtc);
        Assert.Equal("1", item.Quantity);
        Assert.True(item.Urgent);
    }

    [Fact]
    public void An_Edit_ReplacesTheFields_AndTheNormalisedName()
    {
        var item = New();
        var other = Guid.NewGuid();

        var result = item.Update(" Сливки 10% ", "", " к кофе ", other, urgent: true);

        Assert.True(result.IsSuccess);
        Assert.Equal("Сливки 10%", item.Name);
        Assert.Equal("сливки 10%", item.NormalizedName);
        Assert.Null(item.Quantity);
        Assert.Equal("к кофе", item.Note);
        Assert.Equal(other, item.DepartmentId);
        Assert.True(item.Urgent);
    }

    [Fact]
    public void An_InvalidEdit_ChangesNothing()
    {
        var item = New();

        var result = item.Update(" ", null, null, Guid.NewGuid(), true);

        Assert.Equal("shopping.item_name_required", result.Error!.Code);
        Assert.Equal("Молоко", item.Name);
        Assert.Equal(Dairy, item.DepartmentId);
    }

    private static ShoppingItem New() =>
        ShoppingItem.Create("Молоко", null, null, Dairy, false, Person.Vitaliy, Now).Value;
}
