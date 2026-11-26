using Expenses.Domain.Categories;
using Expenses.Domain.Entries;
using Expenses.Domain.Parties;

namespace UnitTests.Expenses.Domain;

public sealed class IncomeAndCategoryTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void An_Income_Keeps_ItsImportKey()
    {
        var result = Income.Create(new DateOnly(2026, 10, 6), Person.Maria, 25_025m, "зарплата", Now, "sheet:income:abc:1");

        Assert.True(result.IsSuccess);
        Assert.Equal("sheet:income:abc:1", result.Value.ImportKey);
    }

    [Fact]
    public void An_Income_BeforeTheYear2000_IsRejected()
    {
        var result = Income.Create(new DateOnly(1999, 12, 31), Person.Vitaliy, 1m, null, Now);

        Assert.Equal("expenses.date_out_of_range", result.Error!.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void A_Category_NeedsAName(string name)
    {
        var result = Category.Create(Guid.NewGuid(), name, 10);

        Assert.Equal("expenses.category_name_required", result.Error!.Code);
    }

    [Fact]
    public void A_Category_Name_IsTrimmed()
    {
        var result = Category.Create(Guid.NewGuid(), " Продукты ", 10);

        Assert.Equal("Продукты", result.Value.Name);
    }
}
