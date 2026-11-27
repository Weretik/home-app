using Identity.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Authentication;

/// <summary>
/// Finds the member behind a bearer token on every request (R-005, R-006): an active session whose e-mail is still on
/// the allowlist. Slides the session's expiry at most once an hour. Not a use case: it runs in the authentication
/// handler, before any endpoint.
/// </summary>
public sealed class SessionAuthenticator(IIdentityDbContext db, IdentitySettings settings, TimeProvider clock)
{
    /// <summary>The longest token worth hashing; real tokens are 43 characters.</summary>
    public const int MaxTokenLength = 256;

    public async Task<MemberProfile?> AuthenticateAsync(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token) || token.Length > MaxTokenLength)
            return null;

        var hash = SessionTokens.Hash(token);
        var session = await db.Sessions.SingleOrDefaultAsync(s => s.TokenHash == hash, cancellationToken);
        var now = clock.GetUtcNow();
        if (session is null || !session.IsActiveAt(now) || !settings.Members.Contains(session.Email))
            return null;

        if (session.Touch(now))
            await db.SaveChangesAsync(cancellationToken);
        return session.Member;
    }
}
