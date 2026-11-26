using Shopping.Domain.Parsing;

namespace UnitTests.Shopping.Text;

/// <summary>The cases the Telegram bot's <c>parseItems_</c> handled; the site must read them the same way.</summary>
public sealed class ItemTextParserTests
{
    [Fact]
    public void A_List_IsSplitIntoItems_WithQuantities()
    {
        var items = ItemTextParser.Parse("молоко, хлеб, яйца 10, сыр 300 г");

        Assert.Equal(
            [new ParsedItem("Молоко", null), new ParsedItem("Хлеб", null), new ParsedItem("Яйца", "10"), new ParsedItem("Сыр", "300 г")],
            items);
    }

    [Theory]
    [InlineData("2 кг картошки", "Картошки", "2 кг")]
    [InlineData("молоко x 3", "Молоко", "3")]
    [InlineData("яйца х10", "Яйца", "10")]
    [InlineData("вода × 6 бут", "Вода", "6 бут")]
    [InlineData("сыр - 300г", "Сыр", "300 г")]
    [InlineData("мука: 1.5 кг.", "Мука", "1.5 кг")]
    [InlineData("3 пачки масла", "Масла", "3 пачки")]
    [InlineData("Молоко 2 Л", "Молоко", "2 Л")]
    [InlineData("помидоры черри", "Помидоры черри", null)]
    [InlineData("- хлеб.", "Хлеб", null)]
    [InlineData("• бананы!", "Бананы", null)]
    public void A_Quantity_AtTheEnd_OrAtTheStart_IsSplitOff(string text, string name, string? quantity)
    {
        var item = Assert.Single(ItemTextParser.Parse(text));

        Assert.Equal(new ParsedItem(name, quantity), item);
    }

    [Fact]
    public void Semicolons_AndNewLines_SeparateItems_AndEmptyParts_AreDropped()
    {
        var items = ItemTextParser.Parse("чай;кофе\n\n , ,сахар");

        Assert.Equal(["Чай", "Кофе", "Сахар"], items.Select(i => i.Name));
    }

    [Fact]
    public void Arabic_Digits_Only_CountAsAQuantity()
    {
        var item = Assert.Single(ItemTextParser.Parse("батарейки ٣"));

        Assert.Equal(new ParsedItem("Батарейки ٣", null), item);
    }

    [Fact]
    public void A_LongName_IsCutTo80Characters()
    {
        var item = Assert.Single(ItemTextParser.Parse(new string('я', 100)));

        Assert.Equal(80, item.Name.Length);
        Assert.StartsWith("Я", item.Name, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  , ; \n ")]
    [InlineData(null)]
    public void Nothing_IsRead_FromEmptyText(string? text)
    {
        Assert.Empty(ItemTextParser.Parse(text));
    }

    [Fact]
    public void A_DecimalComma_SplitsTheItem_AsInTheBot()
    {
        Assert.Equal(2, ItemTextParser.Parse("мука 1,5 кг").Count);
    }
}
