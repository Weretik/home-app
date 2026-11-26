using BuildingBlocks.Application.Outbox;

namespace BuildingBlocks.Infrastructure.Outbox.Dispatch;

/// <summary>Processes the outbox of every module that registered one.</summary>
internal sealed class OutboxTick(IEnumerable<IModuleOutbox> modules) : IOutboxTick
{
    public async Task<OutboxTickReport> RunAsync(CancellationToken cancellationToken)
    {
        var report = OutboxTickReport.Empty;
        foreach (var module in modules)
            report = report.Add(await module.ProcessDueAsync(cancellationToken));

        return report;
    }
}
