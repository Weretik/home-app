namespace BuildingBlocks.Domain.Modeling;

/// <summary>
/// Marks the entry point of a consistency boundary. Only aggregate roots get a <c>DbSet</c>; the rest of the
/// aggregate is changed through the root.
/// </summary>
public interface IAggregateRoot;
