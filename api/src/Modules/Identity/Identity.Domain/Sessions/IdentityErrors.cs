using BuildingBlocks.Domain.Results;

namespace Identity.Domain.Sessions;

/// <summary>Why a sign-in was refused, with stable codes for the web app (spec identity/001, R-001…R-003).</summary>
public static class IdentityErrors
{
    public static readonly Error GoogleTokenInvalid = Error.Unauthorized(
        "identity.google_token_invalid", "Google did not confirm the sign-in. Try again.");

    public static readonly Error NotAMember = Error.Forbidden(
        "identity.not_a_member", "This Google account may not use the site.");

    public static readonly Error NotConfigured = Error.Conflict(
        "identity.not_configured", "Sign-in is not set up yet (App Service setting Identity__Google__ClientId).");
}
