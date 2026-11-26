namespace Expenses.Application.Idempotency;

/// <summary>
/// A create that has already happened: its <c>Idempotency-Key</c>, a fingerprint of what was sent, and the id it
/// produced. Saved in the same transaction as the new row, under a unique key (ADR 0007).
/// </summary>
public sealed class CreateRequest
{
    public const int MaxKeyLength = 100;

    public CreateRequest(string key, string kind, string fingerprint, Guid entityId, DateTimeOffset createdAtUtc)
    {
        Key = key;
        Kind = kind;
        Fingerprint = fingerprint;
        EntityId = entityId;
        CreatedAtUtc = createdAtUtc;
    }

    public string Key { get; private set; }

    /// <summary>What was created: <c>expense</c>, <c>income</c> or <c>savings</c>.</summary>
    public string Kind { get; private set; }

    public string Fingerprint { get; private set; }

    public Guid EntityId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
}
