namespace BuildingBlocks.Infrastructure.Outbox.Storage;

/// <summary>
/// A follow-up stored in the same transaction as the business data, and sent later by the outbox tick. If the
/// transaction rolls back, the message is gone too; if it commits, the message is sent at least once.
/// </summary>
public sealed class OutboxMessage
{
    public const int MaxAttempts = 5;
    public const int MaxErrorLength = 1_000;

    private OutboxMessage()
    {
    }

    public Guid Id { get; private set; }

    /// <summary>The registered message name, stable across class renames.</summary>
    public string Type { get; private set; } = string.Empty;

    /// <summary>The message as JSON.</summary>
    public string Payload { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public int Attempts { get; private set; }

    /// <summary>Not before this moment; a claimed message is leased until then.</summary>
    public DateTimeOffset NextAttemptAtUtc { get; private set; }

    /// <summary>Marks the rows one tick claimed, so a parallel tick cannot take them.</summary>
    public Guid? ClaimToken { get; private set; }

    public DateTimeOffset? ProcessedAtUtc { get; private set; }

    /// <summary>Set after the last allowed attempt failed; the message is not tried again.</summary>
    public DateTimeOffset? FailedAtUtc { get; private set; }

    public string? LastError { get; private set; }

    internal static OutboxMessage Create(string type, string payload, DateTimeOffset nowUtc) => new()
    {
        Id = Guid.CreateVersion7(nowUtc),
        Type = type,
        Payload = payload,
        CreatedAtUtc = nowUtc,
        NextAttemptAtUtc = nowUtc,
    };

    internal void MarkProcessed(DateTimeOffset nowUtc)
    {
        Attempts++;
        ProcessedAtUtc = nowUtc;
        LastError = null;
    }

    /// <summary>Records a failed attempt and schedules the next one; returns true when it was the last attempt.</summary>
    internal bool MarkFailed(DateTimeOffset nowUtc, string error)
    {
        Attempts++;
        LastError = error.Length <= MaxErrorLength ? error : error[..MaxErrorLength];
        if (Attempts >= MaxAttempts)
        {
            FailedAtUtc = nowUtc;
            return true;
        }

        // 1, 4, 9, 16 minutes: a short outage heals without manual work.
        NextAttemptAtUtc = nowUtc + TimeSpan.FromMinutes(Attempts * Attempts);
        return false;
    }
}
