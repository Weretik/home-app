using Identity.Application.Features.Members;

namespace UnitTests.Identity;

/// <summary>Spec identity/001: R-002.</summary>
public sealed class FamilyAllowlistTests
{
    private static readonly FamilyAllowlist Family = new([" Vitaliy@Example.com ", "masha@example.com", "", "  "]);

    [Theory]
    [InlineData("vitaliy@example.com")]
    [InlineData("VITALIY@EXAMPLE.COM")]
    [InlineData(" masha@example.com")]
    public void A_Member_IsOnTheList_WhateverTheCaseAndSpaces(string email)
    {
        Assert.True(Family.Contains(email));
    }

    [Theory]
    [InlineData("stranger@example.com")]
    [InlineData("")]
    [InlineData(null)]
    public void Anyone_Else_IsNot(string? email)
    {
        Assert.False(Family.Contains(email));
    }

    [Fact]
    public void Empty_Entries_AreIgnored()
    {
        Assert.Equal(2, Family.Count);
        Assert.False(new FamilyAllowlist([]).Contains("vitaliy@example.com"));
    }
}
