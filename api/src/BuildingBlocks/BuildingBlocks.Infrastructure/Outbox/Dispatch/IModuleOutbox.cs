using BuildingBlocks.Application.Outbox;

namespace BuildingBlocks.Infrastructure.Outbox.Dispatch;

/// <summary>The outbox of one module, processed by the tick.</summary>
public interface IModuleOutbox
{
    Task<OutboxTickReport> ProcessDueAsync(CancellationToken cancellationToken);
}
