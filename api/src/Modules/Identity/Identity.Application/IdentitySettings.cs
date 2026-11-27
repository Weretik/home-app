using Identity.Application.Features.Members;

namespace Identity.Application;

/// <summary>
/// The settings of section <c>Identity</c>: the Google OAuth client id the ID tokens must be issued for, and the
/// family allowlist. Without a client id nobody can sign in, and the site stays closed (R-003).
/// </summary>
public sealed record IdentitySettings(string GoogleClientId, FamilyAllowlist Members)
{
    public const string SectionName = "Identity";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(GoogleClientId);
}
