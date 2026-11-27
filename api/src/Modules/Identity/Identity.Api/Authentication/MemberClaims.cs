using System.Security.Claims;
using Identity.Application.Features.Members;
using Identity.Domain.Sessions;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Identity.Api.Authentication;

/// <summary>How a member travels through a request: the bearer token in, the claims of the principal out.</summary>
internal static class MemberClaims
{
    public const string Email = "email";
    public const string Name = "name";
    public const string Picture = "picture";

    private const string BearerPrefix = "Bearer ";

    /// <summary>The token of <c>Authorization: Bearer &lt;token&gt;</c>, or null without one.</summary>
    public static string? BearerToken(HttpRequest request)
    {
        var header = request.Headers[HeaderNames.Authorization].ToString();
        return header.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase) && header.Length > BearerPrefix.Length
            ? header[BearerPrefix.Length..].Trim()
            : null;
    }

    public static ClaimsPrincipal Principal(MemberProfile member, string scheme)
    {
        var claims = new List<Claim> { new(Email, member.Email), new(Name, member.Name) };
        if (member.PictureUrl is { } picture)
            claims.Add(new Claim(Picture, picture));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, scheme, Name, ClaimTypes.Role));
    }

    /// <summary>The member of a session principal, or null for any other principal (the scheduler).</summary>
    public static FamilyMember? Member(ClaimsPrincipal principal, FamilyAllowlist family) =>
        principal.FindFirstValue(Email) is { } email
            ? FamilyMember.From(new MemberProfile(email, principal.FindFirstValue(Name) ?? email, principal.FindFirstValue(Picture)), family)
            : null;
}
