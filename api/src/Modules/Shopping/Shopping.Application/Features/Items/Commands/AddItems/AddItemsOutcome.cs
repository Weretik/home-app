namespace Shopping.Application.Features.Items.Commands.AddItems;

/// <summary>The ids an add touched, stored with its Idempotency-Key so a repeat can answer with the same items.</summary>
public sealed record AddItemsOutcome(IReadOnlyList<Guid> Added, IReadOnlyList<Guid> Restored, IReadOnlyList<Guid> Updated);
