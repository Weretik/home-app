namespace BuildingBlocks.Application.Outbox;

/// <summary>
/// Does the follow-up work of one outbox message type, outside the request that stored it. A message can be
/// delivered more than once (after a crash or a timeout), so a handler must be safe to repeat.
/// </summary>
public interface IOutboxMessageHandler<in TMessage>
    where TMessage : class
{
    Task HandleAsync(TMessage message, CancellationToken cancellationToken);
}
