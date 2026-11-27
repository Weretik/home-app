using Identity.Api.Authentication;
using Identity.Application;
using Identity.Application.Features.Members;
using Identity.Contracts;
using Identity.Domain.Sessions;
using Microsoft.AspNetCore.Http;

namespace UnitTests.Identity;

/// <summary>ADR 0040: other modules learn who is signed in through <see cref="ICurrentMember"/>; spec notes/001 R-008.</summary>
public sealed class CurrentMemberTests
{
    private static readonly IdentitySettings Settings =
        new("client-id", new FamilyAllowlist(["vitaliy@example.com", "masha@example.com", "guest@example.com"]));

    private static SignedInMember? MemberOf(HttpContext? context) =>
        new CurrentMember(new HttpContextAccessor { HttpContext = context }, Settings).Get();

    private static DefaultHttpContext SignedIn(string email, string name) =>
        new() { User = MemberClaims.Principal(new MemberProfile(email, name, null), IdentitySchemes.Session) };

    [Theory]
    [InlineData("vitaliy@example.com", HouseholdPerson.Vitaliy)]
    [InlineData("Masha@Example.com", HouseholdPerson.Maria)]
    public void A_Session_Member_IsThePerson_OfTheirPlaceOnTheAllowlist(string email, HouseholdPerson person)
    {
        var member = MemberOf(SignedIn(email, "Ім'я"));

        Assert.Equal(new SignedInMember(email, "Ім'я", person), member);
    }

    [Fact]
    public void A_Member_PastTheFirstTwo_IsSignedIn_ButNobodyInParticular()
    {
        var member = MemberOf(SignedIn("guest@example.com", "Гість"));

        Assert.NotNull(member);
        Assert.Null(member.Person);
    }

    [Fact]
    public void The_SchedulerKey_AndNoRequest_HaveNoMember()
    {
        var scheduler = new DefaultHttpContext
        {
            User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
                [new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "scheduler")], IdentitySchemes.SchedulerKey)),
        };

        Assert.Null(MemberOf(scheduler));
        Assert.Null(MemberOf(new DefaultHttpContext()));
        Assert.Null(MemberOf(null));
    }
}
