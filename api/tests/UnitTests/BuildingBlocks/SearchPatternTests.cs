using BuildingBlocks.Application.Persistence;

namespace UnitTests.BuildingBlocks;

public sealed class SearchPatternTests
{
    [Fact]
    public void Splits_TheInput_IntoOneLowercasePatternPerWord() =>
        Assert.Equal(["%gel%", "%pen%"], SearchPattern.ContainsEachWord("  Gel   PEN "));

    [Fact]
    public void Escapes_LikeWildcards_SoTheyMatchLiterally() =>
        Assert.Equal(["%50\\%%", "%a\\_b%"], SearchPattern.ContainsEachWord("50% a_b"));

    [Fact]
    public void Empty_Input_GivesNoPatterns() => Assert.Empty(SearchPattern.ContainsEachWord(" "));

    [Fact]
    public void StartsWith_MatchesTheWholeValuesStart_WithWildcardsEscaped()
    {
        Assert.Equal("мол%", SearchPattern.StartsWith("Мол"));
        Assert.Equal("50\\%\\_\\\\x%", SearchPattern.StartsWith("50%_\\x"));
    }

    [Fact]
    public void WordStartsWith_MatchesAWordAfterASpace() =>
        Assert.Equal("% чай\\_%", SearchPattern.WordStartsWith("Чай_"));
}
