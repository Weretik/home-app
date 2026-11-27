using Identity.Application.Features.Members;

namespace Identity.Application.Features.Sessions.Commands.SignIn;

/// <summary>A started session: the bearer token (shown only now), when it expires if unused, and who signed in.</summary>
public sealed record IdentitySession(string Token, DateTimeOffset ExpiresAtUtc, FamilyMember Member);
