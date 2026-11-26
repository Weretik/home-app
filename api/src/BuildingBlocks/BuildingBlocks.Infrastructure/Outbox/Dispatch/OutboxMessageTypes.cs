using System.Text.Json;
using BuildingBlocks.Infrastructure.Outbox.Storage;

namespace BuildingBlocks.Infrastructure.Outbox.Dispatch;

/// <summary>
/// The registry of outbox message types: it writes a message as a row and reads a row back into its handler.
/// </summary>
public sealed class OutboxMessageTypes(IEnumerable<OutboxMessageType> types, TimeProvider clock)
{
    private readonly Dictionary<Type, OutboxMessageType> _byType = types.ToDictionary(t => t.MessageType);
    private readonly Dictionary<string, OutboxMessageType> _byName = types.ToDictionary(t => t.Name, StringComparer.Ordinal);

    /// <summary>A new row for <paramref name="message"/>; the caller adds it to its DbContext.</summary>
    public OutboxMessage CreateRow(object message)
    {
        if (!_byType.TryGetValue(message.GetType(), out var type))
            throw new InvalidOperationException($"Outbox message {message.GetType().Name} is not registered.");

        var payload = JsonSerializer.Serialize(message, type.MessageType, JsonSerializerOptions.Web);
        return OutboxMessage.Create(type.Name, payload, clock.GetUtcNow());
    }

    public Task HandleAsync(IServiceProvider services, OutboxMessage row, CancellationToken cancellationToken)
    {
        if (!_byName.TryGetValue(row.Type, out var type))
            throw new InvalidOperationException($"Outbox message type '{row.Type}' is not registered.");

        var message = JsonSerializer.Deserialize(row.Payload, type.MessageType, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException($"Outbox message {row.Id} has an empty payload.");
        return type.Handle(services, message, cancellationToken);
    }
}
