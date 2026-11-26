namespace BuildingBlocks.Application.Outbox;

/// <summary>What one outbox pass did.</summary>
public sealed record OutboxTickReport(int Sent, int Retried, int GaveUp)
{
    public static readonly OutboxTickReport Empty = new(0, 0, 0);

    public OutboxTickReport Add(OutboxTickReport other) =>
        new(Sent + other.Sent, Retried + other.Retried, GaveUp + other.GaveUp);
}
