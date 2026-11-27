namespace Identity.Application.Google;

/// <summary>Checks a Google ID token: Google's signature, expiry, issuer, and the audience of our OAuth client.</summary>
public interface IGoogleTokenVerifier
{
    /// <returns>The identity in the token, or null when the token is not valid for this site.</returns>
    Task<GoogleIdentity?> VerifyAsync(string credential, CancellationToken cancellationToken);
}
