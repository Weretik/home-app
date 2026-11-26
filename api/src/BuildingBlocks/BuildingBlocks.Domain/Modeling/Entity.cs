namespace BuildingBlocks.Domain.Modeling;

/// <summary>An object with an identity that stays the same while its other data changes.</summary>
public abstract class Entity<TId>
    where TId : notnull
{
    public TId Id { get; protected init; } = default!;
}
