using Identity.Domain.Sessions;

namespace Identity.Application.Features.Members;

/// <summary>
/// The signed-in member as the web app shows it: e-mail, name, the Google picture, and who they are in the home (null
/// for an allowlisted e-mail that is neither Vitaliy nor Maria).
/// </summary>
public sealed record FamilyMember(string Email, string Name, string? PictureUrl, FamilyPerson? Person)
{
    public static FamilyMember From(MemberProfile profile, FamilyAllowlist family) =>
        new(profile.Email, profile.Name, profile.PictureUrl, family.PersonOf(profile.Email));
}
