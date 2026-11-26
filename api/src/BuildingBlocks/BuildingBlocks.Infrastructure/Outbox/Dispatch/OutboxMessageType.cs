namespace BuildingBlocks.Infrastructure.Outbox.Dispatch;

/// <summary>One message type a module sends through its outbox, with the code that handles it.</summary>
public sealed record OutboxMessageType(
    string Name,
    Type MessageType,
    Func<IServiceProvider, object, CancellationToken, Task> Handle);
