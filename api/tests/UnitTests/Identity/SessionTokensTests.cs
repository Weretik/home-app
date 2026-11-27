using Identity.Application.Authentication;

namespace UnitTests.Identity;

/// <summary>Spec identity/001: R-004.</summary>
public sealed class SessionTokensTests
{
    [Fact]
    public void A_Token_Is43UrlSafeCharacters_AndNeverRepeats()
    {
        var tokens = Enumerable.Range(0, 100).Select(_ => SessionTokens.New()).ToList();

        Assert.All(tokens, token => Assert.Matches("^[A-Za-z0-9_-]{43}$", token));
        Assert.Equal(tokens.Count, tokens.Distinct().Count());
    }

    [Fact]
    public void The_Hash_IsLowerCaseHexSha256()
    {
        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", SessionTokens.Hash("abc"));
    }
}
