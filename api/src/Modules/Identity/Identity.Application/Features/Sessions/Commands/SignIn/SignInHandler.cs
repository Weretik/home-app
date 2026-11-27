using BuildingBlocks.Domain.Results;
using Identity.Application.Authentication;
using Identity.Application.Features.Members;
using Identity.Application.Google;
using Identity.Domain.Sessions;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Features.Sessions.Commands.SignIn;

/// <summary>
/// Verifies the Google token, checks the allowlist, stores a session under the hash of a new token and deletes the
/// member's long-ended sessions in the same save (R-012).
/// </summary>
public sealed class SignInHandler(
    IIdentityDbContext db, IGoogleTokenVerifier google, IdentitySettings settings, TimeProvider clock)
    : ICommandHandler<SignInCommand, Result<IdentitySession>>
{
    public async ValueTask<Result<IdentitySession>> Handle(SignInCommand c, CancellationToken cancellationToken)
    {
        if (!settings.IsConfigured)
            return IdentityErrors.NotConfigured;

        var identity = await google.VerifyAsync(c.Credential, cancellationToken);
        if (identity is not { EmailVerified: true })
            return IdentityErrors.GoogleTokenInvalid;
        if (!settings.Members.Contains(identity.Email))
            return IdentityErrors.NotAMember;

        var now = clock.GetUtcNow();
        var email = FamilyAllowlist.Normalize(identity.Email);
        var token = SessionTokens.New();
        var session = Session.Start(SessionTokens.Hash(token), new MemberProfile(email, identity.Name ?? email, identity.PictureUrl), now);

        var ended = await db.Sessions.Where(s => s.Email == email && (s.RevokedAtUtc != null || s.ExpiresAtUtc < now))
            .ToListAsync(cancellationToken);
        db.Sessions.RemoveRange(ended.Where(s => s.IsStaleAt(now)));
        db.Sessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);

        return new IdentitySession(token, session.ExpiresAtUtc, FamilyMember.From(session.Member, settings.Members));
    }
}
