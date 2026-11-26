namespace BuildingBlocks.Application.Outbox;

/// <summary>One pass over the outbox of every module; Cloud Scheduler calls it every few minutes.</summary>
public interface IOutboxTick
{
    Task<OutboxTickReport> RunAsync(CancellationToken cancellationToken);
}
