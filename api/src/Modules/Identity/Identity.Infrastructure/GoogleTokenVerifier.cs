using Google.Apis.Auth;
using Identity.Application;
using Identity.Application.Google;

namespace Identity.Infrastructure;

/// <summary>
/// Verifies a Google ID token with <see cref="GoogleJsonWebSignature"/>: Google's signature (keys cached by the
/// library), issuer, expiry, and the audience = our OAuth client id (R-001). No client secret is needed.
/// </summary>
internal sealed class GoogleTokenVerifier(IdentitySettings settings) : IGoogleTokenVerifier
{
    private static readonly TimeSpan ClockTolerance = TimeSpan.FromMinutes(5);

    public async Task<GoogleIdentity?> VerifyAsync(string credential, CancellationToken cancellationToken)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(credential, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [settings.GoogleClientId],
                IssuedAtClockTolerance = ClockTolerance,
                ExpirationTimeClockTolerance = ClockTolerance,
            });
            return string.IsNullOrWhiteSpace(payload.Email)
                ? null
                : new GoogleIdentity(payload.Email, payload.EmailVerified, payload.Name, payload.Picture);
        }
        catch (InvalidJwtException)
        {
            return null;
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
