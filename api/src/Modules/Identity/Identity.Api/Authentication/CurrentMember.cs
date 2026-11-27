using Identity.Application;
using Identity.Application.Features.Members;
using Identity.Contracts;
using Microsoft.AspNetCore.Http;

namespace Identity.Api.Authentication;

/// <summary>
/// <see cref="ICurrentMember"/> over the principal the session handler set for this request (ADR 0040). The person
/// comes from the allowlist order (ADR 0038); the deploy check key and a request-less scope have no member.
/// </summary>
internal sealed class CurrentMember(IHttpContextAccessor http, IdentitySettings settings) : ICurrentMember
{
    public SignedInMember? Get() =>
        http.HttpContext?.User is { } principal && MemberClaims.Member(principal, settings.Members) is { } member
            ? new SignedInMember(member.Email, member.Name, ToHousehold(member.Person))
            : null;

    private static HouseholdPerson? ToHousehold(FamilyPerson? person) => person switch
    {
        FamilyPerson.Vitaliy => HouseholdPerson.Vitaliy,
        FamilyPerson.Maria => HouseholdPerson.Maria,
        _ => null,
    };
}
