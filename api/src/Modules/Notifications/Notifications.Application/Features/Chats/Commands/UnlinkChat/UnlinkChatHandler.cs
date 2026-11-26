using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Notifications.Domain.Chats;

namespace Notifications.Application.Features.Chats.Commands.UnlinkChat;

/// <summary>One <c>DELETE</c>; 404 when nothing was linked.</summary>
public sealed class UnlinkChatHandler(INotificationsDbContext db) : ICommandHandler<UnlinkChatCommand, Result>
{
    public async ValueTask<Result> Handle(UnlinkChatCommand c, CancellationToken cancellationToken)
    {
        var deleted = await db.Chats.Where(x => x.Audience == c.Audience).ExecuteDeleteAsync(cancellationToken);
        return deleted == 0 ? ChatErrors.NotLinked(c.Audience) : Result.Success();
    }
}
