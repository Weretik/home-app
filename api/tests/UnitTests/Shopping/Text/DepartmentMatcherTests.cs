using System;
using Shopping.Domain.Departments;
using Shopping.Domain.Matching;

namespace UnitTests.Shopping.Text;

/// <summary>The rules of the bot's <c>deptOf_</c>, on a small dictionary taken from «Справочник».</summary>
public sealed class DepartmentMatcherTests
{
    private static readonly DepartmentId Vegetables = new(Guid.NewGuid());
    private static readonly DepartmentId Dairy = new(Guid.NewGuid());
    private static readonly DepartmentId Grocery = new(Guid.NewGuid());
    private static readonly DepartmentId Hygiene = new(Guid.NewGuid());
    private static readonly DepartmentId Chemistry = new(Guid.NewGuid());
    private static readonly DepartmentId Other = new(Guid.NewGuid());

    private static readonly DepartmentMatcher Matcher = new(
    [
        new KeywordRule("картош", Vegetables),
        new KeywordRule("молок", Dairy),
        new KeywordRule("яйц", Dairy),
        new KeywordRule("масло", Dairy),
        new KeywordRule("паст", Grocery),
        new KeywordRule("масло подсолн", Grocery),
        new KeywordRule("зубн", Hygiene),
        new KeywordRule("зубн паст", Hygiene),
        new KeywordRule("соль", Grocery),
        new KeywordRule("соль посуд", Chemistry),
        new KeywordRule("молочко для тела", Hygiene),
    ], Other);

    [Theory]
    [InlineData("Молоко", nameof(Dairy))]
    [InlineData("Картошка молодая", nameof(Vegetables))]
    [InlineData("Яйца", nameof(Dairy))]
    [InlineData("Паста томатная", nameof(Grocery))]
    [InlineData("Масло подсолнечное", nameof(Grocery))]
    [InlineData("Сливочное масло", nameof(Dairy))]
    [InlineData("Соль для посудомойки", nameof(Chemistry))]
    public void The_LongestMatchingKeyword_Wins(string name, string department)
    {
        Assert.Equal(Department(department), Matcher.Match(name));
    }

    [Fact]
    public void A_MultiWordKeyword_BeatsItsShorterPart()
    {
        Assert.Equal(Hygiene, Matcher.Match("Зубная паста"));
    }

    [Fact]
    public void An_ExactName_Scores1000_AndBeatsAnEquallyLongPrefixMatch_GivenEarlier()
    {
        var matcher = new DepartmentMatcher(
            [new KeywordRule("сыр косичка", Grocery), new KeywordRule("сыр-косичка", Dairy)], Other);

        Assert.Equal(Dairy, matcher.Match("Сыр-косичка"));
        Assert.Equal(Grocery, matcher.Match("Сыр косичка копчёный"));
    }

    [Fact]
    public void Yo_AndCase_DoNotMatter()
    {
        var matcher = new DepartmentMatcher([new KeywordRule("мед", Grocery)], Other);

        Assert.Equal(Grocery, matcher.Match("МЁД гречишный"));
    }

    [Fact]
    public void A_Keyword_MustStartAWord_NotSitInsideIt()
    {
        Assert.Equal(Other, Matcher.Match("Шпаклёвка"));
    }

    [Fact]
    public void No_Match_GivesTheFallback()
    {
        Assert.Equal(Other, Matcher.Match("Штуковина"));
        Assert.Equal(Other, new DepartmentMatcher([], Other).Match("Молоко"));
    }

    [Fact]
    public void On_ATie_TheFirstRule_Wins()
    {
        var matcher = new DepartmentMatcher([new KeywordRule("сыр", Dairy), new KeywordRule("сыр", Grocery)], Other);

        Assert.Equal(Dairy, matcher.Match("Сыр плавленый"));
    }

    private static DepartmentId Department(string name) => name switch
    {
        nameof(Vegetables) => Vegetables,
        nameof(Dairy) => Dairy,
        nameof(Grocery) => Grocery,
        nameof(Hygiene) => Hygiene,
        nameof(Chemistry) => Chemistry,
        _ => Other,
    };
}
