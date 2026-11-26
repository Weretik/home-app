namespace BuildingBlocks.Application.Outbox;

/// <summary>
/// Part of a module's DbContext port: <see cref="Publish"/> stores a message for another module (or for an outside
/// system) in the module's outbox, so it is saved by the same <c>SaveChangesAsync</c> as the data (ADR 0035).
/// </summary>
public interface IOutboxPublisher
{
    /// <summary>Adds <paramref name="message"/> to the outbox; it is written with the next save and sent by the tick.</summary>
    void Publish(object message);
}
