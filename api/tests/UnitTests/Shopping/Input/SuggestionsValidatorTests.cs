using Shopping.Application.Features.Items.Queries.ListSuggestions;

namespace UnitTests.Shopping.Input;

public sealed class SuggestionsValidatorTests
{
    private static readonly ListSuggestionsValidator Validator = new();

    [Theory]
    [InlineData("м", true)]
    [InlineData("  Зелёный   чай ", true)]
    [InlineData("50%", true)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void The_Query_IsCheckedNormalised(string? query, bool valid)
    {
        var result = Validator.Validate(new ListSuggestionsQuery(query));

        Assert.Equal(valid, result.IsValid);
        if (!valid)
            Assert.Contains(result.Errors, e => e.PropertyName == "Query");
    }

    [Fact]
    public void The_Query_IsAtMost80Characters_AfterSpacesCollapse()
    {
        Assert.True(Validator.Validate(new ListSuggestionsQuery(new string('а', 80))).IsValid);
        Assert.True(Validator.Validate(new ListSuggestionsQuery(new string('а', 40) + "     " + new string('б', 39))).IsValid);
        Assert.False(Validator.Validate(new ListSuggestionsQuery(new string('а', 81))).IsValid);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(8, true)]
    [InlineData(20, true)]
    [InlineData(0, false)]
    [InlineData(21, false)]
    public void The_Limit_Is1To20(int limit, bool valid)
    {
        Assert.Equal(valid, Validator.Validate(new ListSuggestionsQuery("молоко", limit)).IsValid);
    }

    [Fact]
    public void The_Limit_DefaultsTo8() => Assert.Equal(8, new ListSuggestionsQuery("молоко").Limit);
}
