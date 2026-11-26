namespace BuildingBlocks.Application.Persistence;

/// <summary>
/// The database rejected a save because a unique index already holds the value: two requests raced for the same
/// slug or idempotency key. Infrastructure raises it, so handlers can turn the race into a business result without
/// knowing the database driver.
/// </summary>
public sealed class UniqueConstraintViolationException(string? constraintName, Exception innerException)
    : Exception($"Unique constraint '{constraintName}' was violated.", innerException)
{
    public string? ConstraintName { get; } = constraintName;
}
