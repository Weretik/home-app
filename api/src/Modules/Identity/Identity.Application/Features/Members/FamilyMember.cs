using Identity.Domain.Sessions;

namespace Identity.Application.Features.Members;

/// <summary>The signed-in member as the web app shows it: e-mail, name and the Google picture.</summary>
public sealed record FamilyMember(string Email, string Name, string? PictureUrl)
{
    public static FamilyMember From(MemberProfile profile) => new(profile.Email, profile.Name, profile.PictureUrl);
}
