using Identity.Application.Features.Members;

namespace UnitTests.Identity;

/// <summary>Spec identity/001: R-002; spec identity/002: R-001.</summary>
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

    [Fact]
    public void The_FirstEntry_IsVitaliy_AndTheSecond_IsMaria()
    {
        Assert.Equal(FamilyPerson.Vitaliy, Family.PersonOf("VITALIY@example.com "));
        Assert.Equal(FamilyPerson.Maria, Family.PersonOf("masha@example.com"));
    }

    [Fact]
    public void An_EmptyEntry_KeepsItsPlace()
    {
        var onlyMasha = new FamilyAllowlist([null, "masha@example.com"]);

        Assert.Equal(FamilyPerson.Maria, onlyMasha.PersonOf("masha@example.com"));
        Assert.Equal(1, onlyMasha.Count);
    }

    [Fact]
    public void A_ThirdEntry_SignsIn_AsNobodyInParticular()
    {
        var family = new FamilyAllowlist(["v@example.com", "m@example.com", "guest@example.com"]);

        Assert.True(family.Contains("guest@example.com"));
        Assert.Null(family.PersonOf("guest@example.com"));
        Assert.Null(family.PersonOf("stranger@example.com"));
        Assert.Null(family.PersonOf(null));
    }
}
