using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Notifications.Domain.Schedule;

namespace Notifications.Application.Features.Tick.Commands.RecordTick;

/// <summary>One set-based update of the single heartbeat row (seeded by the migration); safe to run twice.</summary>
public sealed class RecordTickHandler(INotificationsDbContext db, TimeProvider clock) : ICommandHandler<RecordTickCommand, Result>
{
    public async ValueTask<Result> Handle(RecordTickCommand command, CancellationToken cancellationToken)
    {
        DateTimeOffset? tickedAt = clock.GetUtcNow();
        await db.Heartbeats.Where(h => h.Id == SchedulerHeartbeat.SingletonId)
            .ExecuteUpdateAsync(s => s.SetProperty(h => h.LastTickAtUtc, tickedAt), cancellationToken);
        return Result.Success();
    }
}
