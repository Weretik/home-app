using BuildingBlocks.Domain.Results;
using Identity.Application.Authentication;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Features.Sessions.Commands.SignOut;

/// <summary>Revokes the session; signing out of a session that is already gone succeeds too.</summary>
public sealed class SignOutHandler(IIdentityDbContext db, TimeProvider clock) : ICommandHandler<SignOutCommand, Result>
{
    public async ValueTask<Result> Handle(SignOutCommand c, CancellationToken cancellationToken)
    {
        var hash = SessionTokens.Hash(c.Token);
        var session = await db.Sessions.SingleOrDefaultAsync(s => s.TokenHash == hash, cancellationToken);
        if (session is null)
            return Result.Success();

        session.Revoke(clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
