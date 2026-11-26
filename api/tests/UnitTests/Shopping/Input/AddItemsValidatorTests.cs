using Shopping.Application.Features.Items.Commands.AddItems;
using Shopping.Domain.Items;

namespace UnitTests.Shopping.Input;

public sealed class AddItemsValidatorTests
{
    private static readonly AddItemsValidator Validator = new();

    [Fact]
    public void A_List_OfItems_IsValid()
    {
        Assert.True(Validator.Validate(Command("молоко, хлеб, яйца 10")).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" , ; ")]
    public void Text_WithoutItems_IsInvalid(string text)
    {
        Assert.False(Validator.Validate(Command(text)).IsValid);
    }

    [Fact]
    public void At_Most50Items_And1000Characters_AreAccepted()
    {
        Assert.True(Validator.Validate(Command(string.Join(",", Enumerable.Repeat("хлеб", 50)))).IsValid);
        Assert.False(Validator.Validate(Command(string.Join(",", Enumerable.Repeat("хлеб", 51)))).IsValid);
        Assert.False(Validator.Validate(Command(new string('я', 1001))).IsValid);
    }

    [Fact]
    public void A_Key_AndAPerson_AreRequired()
    {
        Assert.False(Validator.Validate(Command("хлеб") with { IdempotencyKey = "" }).IsValid);
        Assert.False(Validator.Validate(Command("хлеб") with { AddedBy = (Person)7 }).IsValid);
        Assert.False(Validator.Validate(Command("хлеб") with { DepartmentId = Guid.Empty }).IsValid);
    }

    private static AddItemsCommand Command(string text) => new("key-1", text, Person.Maria, null, false);
}
