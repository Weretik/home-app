using BuildingBlocks.Domain.Results;
using Identity.Application.Authentication;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Features.Sessions.Commands.SignOutEverywhere;

/// <summary>
/// Revokes every active session of the member on every device, so a lost phone or a copied token stops working at
/// once. A member has a few sessions, so they are loaded and revoked in one save. A token that is already gone
/// succeeds and changes nothing; a repeat changes nothing either.
/// </summary>
public sealed class SignOutEverywhereHandler(IIdentityDbContext db, TimeProvider clock)
    : ICommandHandler<SignOutEverywhereCommand, Result>
{
    public async ValueTask<Result> Handle(SignOutEverywhereCommand c, CancellationToken cancellationToken)
    {
        var hash = SessionTokens.Hash(c.Token);
        var email = await db.Sessions
            .Where(s => s.TokenHash == hash)
            .Select(s => s.Email)
            .SingleOrDefaultAsync(cancellationToken);
        if (email is null)
            return Result.Success();

        var now = clock.GetUtcNow();
        var sessions = await db.Sessions
            .Where(s => s.Email == email && s.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);
        foreach (var session in sessions)
            session.Revoke(now);

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
