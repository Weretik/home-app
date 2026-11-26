using BuildingBlocks.Application.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using BuildingBlocks.Infrastructure.Outbox.Storage;

namespace BuildingBlocks.Infrastructure.Outbox.Dispatch;

/// <summary>
/// Sends the due messages of the module that owns <typeparamref name="TDbContext"/>. Race guard: one conditional
/// UPDATE claims a batch under a fresh token and a lease, so two ticks running at once never take the same row.
/// The handlers run one by one, and all results are saved with one <c>SaveChanges</c> at the end.
/// </summary>
internal sealed partial class ModuleOutbox<TDbContext>(
    IServiceScopeFactory scopeFactory,
    OutboxMessageTypes types,
    TimeProvider clock,
    ILogger<ModuleOutbox<TDbContext>> logger) : IModuleOutbox
    where TDbContext : DbContext
{
    private const int BatchSize = 20;
    private static readonly TimeSpan Lease = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan Retention = TimeSpan.FromDays(7);

    public async Task<OutboxTickReport> ProcessDueAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var messages = db.Set<OutboxMessage>();
        var now = clock.GetUtcNow();
        var token = Guid.NewGuid();

        var due = messages
            .Where(m => m.ProcessedAtUtc == null && m.FailedAtUtc == null && m.NextAttemptAtUtc <= now)
            .OrderBy(m => m.CreatedAtUtc)
            .Select(m => m.Id)
            .Take(BatchSize);
        await messages
            .Where(m => due.Contains(m.Id) && m.NextAttemptAtUtc <= now)
            .ExecuteUpdateAsync(s => s
                .SetProperty(m => m.ClaimToken, token)
                .SetProperty(m => m.NextAttemptAtUtc, now + Lease), cancellationToken);

        var claimed = await messages.Where(m => m.ClaimToken == token).OrderBy(m => m.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var report = OutboxTickReport.Empty;
        foreach (var message in claimed)
            report = report.Add(await TrySendAsync(scope.ServiceProvider, message, cancellationToken));

        await db.SaveChangesAsync(cancellationToken);
        await messages.Where(m => m.ProcessedAtUtc < now - Retention).ExecuteDeleteAsync(cancellationToken);
        return report;
    }

    private async Task<OutboxTickReport> TrySendAsync(
        IServiceProvider services, OutboxMessage message, CancellationToken cancellationToken)
    {
        try
        {
            await types.HandleAsync(services, message, cancellationToken);
            message.MarkProcessed(clock.GetUtcNow());
            return new OutboxTickReport(1, 0, 0);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            LogAttemptFailed(logger, message.Type, message.Id, exception);
            return message.MarkFailed(clock.GetUtcNow(), exception.Message)
                ? new OutboxTickReport(0, 0, 1)
                : new OutboxTickReport(0, 1, 0);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Outbox message {Type} {Id} failed")]
    private static partial void LogAttemptFailed(ILogger logger, string type, Guid id, Exception exception);
}
