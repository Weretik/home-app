namespace Todos.Contracts;

/// <summary>
/// Integration event: a person moved a task that came from another module into «Сделано». For a gift
/// (<see cref="SourceKind"/> = <c>gift</c>) the Gifts module moves the gift to «Подаренные» (ADR 0035).
/// </summary>
public sealed record TodoTaskCompleted(Guid TaskId, string SourceKind, Guid SourceId)
{
    public const string GiftSource = "gift";
}
