namespace Shopping.Application.Idempotency;

/// <summary>
/// A create that has already happened: its <c>Idempotency-Key</c>, a fingerprint of what was sent, and what it
/// produced (JSON, because one add can create, revive and update several items). Saved in the same transaction as
/// the rows, under the key as primary key (ADR 0007).
/// </summary>
public sealed class CreateRequest
{
    public const int MaxKeyLength = 100;

    public CreateRequest(string key, string kind, string fingerprint, string outcome, DateTimeOffset createdAtUtc)
    {
        Key = key;
        Kind = kind;
        Fingerprint = fingerprint;
        Outcome = outcome;
        CreatedAtUtc = createdAtUtc;
    }

    public string Key { get; private set; }

    /// <summary>What was created: <c>items</c>, <c>department</c> or <c>keyword</c>.</summary>
    public string Kind { get; private set; }

    public string Fingerprint { get; private set; }

    /// <summary>The ids the create produced, as JSON; a repeat answers from them.</summary>
    public string Outcome { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
}
